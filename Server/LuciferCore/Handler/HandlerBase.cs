using LuciferCore.Attributes;
using LuciferCore.Helper;
using LuciferCore.Interface;
using LuciferCore.NetCoreServer;
using System.Reflection;

namespace Server.LuciferCore.Handler
{
    /// <summary>
    /// Cung cấp lớp cơ sở cho các handler xử lý HTTP request theo phương thức (GET, POST, PUT, DELETE).
    /// Tự động phân tuyến đến hàm xử lý phù hợp dựa trên URL và phương thức HTTP.
    /// </summary>
    public abstract class HandlerBase : IHandler
    {
        /// <summary>
        /// Tên định danh cho handler, dùng để khớp phần đầu URL.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Các route xử lý HEAD request.
        /// </summary>
        protected Dictionary<string, Action<HttpRequest, HttpsSession>> HeadRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Các route xử lý GET request.
        /// </summary>
        protected Dictionary<string, Action<HttpRequest, HttpsSession>> GetRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Các route xử lý POST request.
        /// </summary>
        protected Dictionary<string, Action<HttpRequest, HttpsSession>> PostRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Các route xử lý PUT request.
        /// </summary>
        protected Dictionary<string, Action<HttpRequest, HttpsSession>> PutRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Các route xử lý DELETE request.
        /// </summary>
        protected Dictionary<string, Action<HttpRequest, HttpsSession>> DeleteRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Các route xử lý OPTION request.
        /// </summary>
        protected Dictionary<string, Action<HttpRequest, HttpsSession>> OptionsRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Các route xử lý TRACE request.
        /// </summary>
        protected Dictionary<string, Action<HttpRequest, HttpsSession>> TraceRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);


        protected HandlerBase()
        {
            var methods = GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var routeAttrs = method.GetCustomAttributes<RouteAttribute>(true);
                foreach (var attr in routeAttrs)
                {
                    // Tạo delegate từ method để gọi được
                    var action = (Action<HttpRequest, HttpsSession>)
                        Delegate.CreateDelegate(typeof(Action<HttpRequest, HttpsSession>), this, method);

                    // Ghép Type (prefix) + path (sub route)
                    var fullPath = (Type + attr.Path).ToLower();

                    switch (attr.Method)
                    {
                        case "HEAD": HeadRoutes[fullPath] = action; break;
                        case "GET": GetRoutes[fullPath] = action; break;
                        case "POST": PostRoutes[fullPath] = action; break;
                        case "PUT": PutRoutes[fullPath] = action; break;
                        case "DELETE": DeleteRoutes[fullPath] = action; break;
                        case "OPTIONS": OptionsRoutes[fullPath] = action; break;
                        case "TRACE": TraceRoutes[fullPath] = action; break;
                    }
                }
            }

        }

        /// <summary>
        /// Kiểm tra xem handler này có thể xử lý path đã cho hay không.
        /// </summary>
        /// <param name="path">Đường dẫn URL được yêu cầu.</param>
        /// <returns>True nếu handler có thể xử lý path này.</returns>
        public virtual bool CanHandle(string path) => path.StartsWith(Type, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Xử lý request chính. Tự động phân tuyến dựa trên phương thức HTTP và đường dẫn.
        /// </summary>
        /// <param name="request">Request nhận được từ client.</param>
        /// <param name="session">Phiên kết nối hiện tại.</param>
        public virtual void Handle(HttpRequest request, HttpsSession session)
        {
            string path = DecodeHelper.GetBasePath(request.Url);

            Dictionary<string, Action<HttpRequest, HttpsSession>>? routes = request.Method.ToUpper() switch
            {
                "HEAD" => HeadRoutes,
                "GET" => GetRoutes,
                "POST" => PostRoutes,
                "PUT" => PutRoutes,
                "DELETE" => DeleteRoutes,
                "TRACE" => TraceRoutes,
                "OPTIONS" => OptionsRoutes,
                _ => null
            };

            if (routes != null && routes.TryGetValue(path, out var action))
            {
                action(request, session);
            }
            else
            {
                ErrorHandle(session);
            }
        }

        /// <summary>
        /// Xử lý HTTP HEAD request. Trả về header mà không có nội dung.
        /// </summary>
        /// <param name="session">Phiên kết nối hiện tại.</param>
        protected virtual void HeadHandle(HttpRequest request, HttpsSession session)
        {
            session.SendResponseAsync(session.Response.MakeHeadResponse());
        }

        /// <summary>
        /// Xử lý HTTP GET request. Ghi đè nếu cần custom riêng.
        /// </summary>
        protected virtual void GetHandle(HttpRequest request, HttpsSession session)
        {
            ErrorHandle(session, "Method GET chưa được triển khai cho endpoint này!");
        }

        /// <summary>
        /// Xử lý HTTP POST request. Ghi đè nếu cần custom riêng.
        /// </summary>
        protected virtual void PostHandle(HttpRequest request, HttpsSession session)
        {
            ErrorHandle(session, "Method POST chưa được triển khai cho endpoint này!");
        }

        /// <summary>
        /// Xử lý HTTP PUT request. Ghi đè nếu cần custom riêng.
        /// </summary>
        protected virtual void PutHandle(HttpRequest request, HttpsSession session)
        {
            ErrorHandle(session, "Method PUT chưa được triển khai cho endpoint này!");
        }

        /// <summary>
        /// Xử lý HTTP DELETE request. Ghi đè nếu cần custom riêng.
        /// </summary>
        protected virtual void DeleteHandle(HttpRequest request, HttpsSession session)
        {
            ErrorHandle(session, "Method DELETE chưa được triển khai cho endpoint này!");
        }

        /// <summary>
        /// Xử lý HTTP OPTIONS request. Trả về các phương thức được hỗ trợ.
        /// </summary>
        protected virtual void OptionsHandle(HttpRequest request, HttpsSession session)
        {
            session.SendResponseAsync(session.Response.MakeOptionsResponse());
        }

        /// <summary>
        /// Xử lý HTTP TRACE request. Trả về nội dung của chính request gửi lên.
        /// </summary>
        protected virtual void TraceHandle(HttpRequest request, HttpsSession session)
        {
            session.SendResponseAsync(session.Response.MakeTraceResponse(request));
        }

        /// <summary>
        /// Gửi phản hồi dạng JSON với mã trạng thái và dữ liệu cho client.
        /// </summary>
        /// <param name="session">Phiên kết nối hiện tại.</param>
        /// <param name="data">Dữ liệu JSON muốn trả về (nếu có).</param>
        /// <param name="statusCode">Mã trạng thái HTTP.</param>
        protected virtual void SendJsonResponse(HttpsSession session, object data, int statusCode)
        {
            var response = data != null
                ? session.Response.MakeJsonResponse(data, statusCode)
                : session.Response.MakeJsonResponse(statusCode);
            session.SendResponseAsync(response);
        }

        /// <summary>
        /// Gửi phản hồi lỗi dạng JSON với mã lỗi 400 (Bad Request).
        /// </summary>
        /// <param name="session">Phiên kết nối hiện tại.</param>
        /// <param name="data">Thông tin lỗi bổ sung (nếu có).</param>
        public virtual void ErrorHandle(HttpsSession session, object data = null, int status = 400)
        {
            if (data is string message)
                data = new { message };

            SendJsonResponse(session, data, status);
        }

        /// <summary>
        /// Gửi phản hồi thành công dạng JSON với mã 200 (OK).
        /// </summary>
        /// <param name="session">Phiên kết nối hiện tại.</param>
        /// <param name="data">Dữ liệu muốn trả về (nếu có).</param>
        public virtual void OkHandle(HttpsSession session, object data = null, int status = 200)
        {
            if (data is string message)
                data = new { message };

            SendJsonResponse(session, data, status);
        }

    }
}
