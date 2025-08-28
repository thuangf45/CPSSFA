using LuciferCore.Core;
//using LuciferCore.Event;
using LuciferCore.Extra;
using LuciferCore.Manager;
using LuciferCore.Model;
using LuciferCore.NetCoreServer;
using System.Net.Sockets;
using static LuciferCore.Core.Simulation;

namespace LuciferCore.Controller
{
    /// <summary>
    /// Phiên làm việc HTTPS xử lý từng yêu cầu từ client.
    /// Bao gồm cập nhật số lượng request, kiểm tra xác thực người dùng,
    /// ghi log và lập lịch sự kiện xử lý.
    /// </summary>
    public class SessionController : HttpsSession
    {
        /// <summary>
        /// Khởi tạo một session mới với server đã cho.
        /// </summary>
        /// <param name="server">Server chủ quản session này.</param>
        public SessionController(HttpsServer server) : base(server) { }

        /// <summary>
        /// Được gọi khi nhận một yêu cầu HTTP hợp lệ từ client.
        /// - Cập nhật số lượng request.
        /// - Xác thực người dùng từ request.
        /// - Ghi log thông tin request.
        /// - Lập lịch sự kiện API để xử lý request bất đồng bộ.
        /// </summary>
        /// <param name="request">Đối tượng HttpRequest nhận từ client.</param>
        protected override void OnReceivedRequest(HttpRequest request)
        {
            Simulation.GetModel<ModelServer>().UpdateNumberRequest();

            if (Simulation.GetModel<SessionManager>().Authorization(request, out string userId, this))
            {
                Simulation.GetModel<LogManager>().Log($"[UserID: {userId} ] Request {request.Method} {request.Url}");
            }
            else
            {
                Simulation.GetModel<LogManager>().Log($"[UserID: {userId} Unknown] Request {request.Method} {request.Url}");
            }

            // Lập lịch sự kiện xử lý API
            //var ev = Schedule<APIEvent>(0.25f);
            //ev.request = new HttpRequestCopy(request);
            //ev.session = this;
        }

        /// <summary>
        /// Được gọi khi xảy ra lỗi trong quá trình nhận request.
        /// Ghi lại thông tin lỗi vào hệ thống log.
        /// </summary>
        /// <param name="request">Yêu cầu HTTP gây lỗi.</param>
        /// <param name="error">Thông báo lỗi chi tiết.</param>
        protected override void OnReceivedRequestError(HttpRequest request, string error)
        {
            Simulation.GetModel<LogManager>().Log($"Request error: {error}", LogLevel.ERROR, LogSource.SYSTEM);
        }

        /// <summary>
        /// Được gọi khi session gặp lỗi socket trong quá trình giao tiếp.
        /// Ghi lại thông tin lỗi socket vào log hệ thống.
        /// </summary>
        /// <param name="error">Loại lỗi socket gặp phải.</param>
        protected override void OnError(SocketError error)
        {
            Simulation.GetModel<LogManager>().Log($"HTTPS session caught an error: {error}", LogLevel.ERROR, LogSource.SYSTEM);
        }
    }
}
