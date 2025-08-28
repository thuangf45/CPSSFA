using LuciferCore.NetCoreServer;

namespace LuciferCore.Helper
{
    /// <summary>
    /// Lớp hỗ trợ thao tác với session thông qua cookie trong HttpRequest/HttpResponse.
    /// </summary>
    public static class CookieHelper
    {
        /// <summary>
        /// Thiết lập sessionId vào cookie của response.
        /// </summary>
        /// <param name="response">Đối tượng HttpResponse.</param>
        /// <param name="sessionId">Giá trị sessionId cần lưu trữ.</param>
        /// <param name="minutes">Thời gian tồn tại (phút), mặc định là 60 phút.</param>
        public static void SetSession(HttpResponse response, string sessionId, int minutes = 60)
        {
            response.SetCookie("sessionId", sessionId, maxAge: minutes * 60, path: "/", secure: true, strict: true, httpOnly: true);
        }

        /// <summary>
        /// Xóa sessionId khỏi cookie bằng cách đặt thời gian sống về 0.
        /// </summary>
        /// <param name="response">Đối tượng HttpResponse.</param>
        public static void RemoveSession(HttpResponse response)
        {
            response.SetCookie("sessionId", "", maxAge: 0, path: "/", secure: true, strict: true, httpOnly: true);
        }

        /// <summary>
        /// Trích xuất sessionId từ cookie trong HttpRequest.
        /// </summary>
        /// <param name="request">Đối tượng HttpRequest.</param>
        /// <returns>Giá trị sessionId nếu tồn tại, ngược lại là null.</returns>
        public static string? GetSession(HttpRequest request)
        {
            if (request.Cookies > 0)
            {
                for (int i = 0; i < request.Cookies; i++)
                {
                    var (key, value) = request.Cookie(i);
                    if (key == "sessionId")
                        return value;
                }
            }
            return null;
        }
    }
}
