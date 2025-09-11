using LuciferCore.NetCoreServer;
using System.Text;

namespace LuciferCore.Helper
{
    /// <summary>
    /// Cung cấp các phương thức hỗ trợ để tạo, phân tích và quản lý mã thông báo (token) xác thực trong ứng dụng máy chủ web.
    /// </summary>
    public static class TokenHelper
    {
        /// <summary>
        /// Tạo mã thông báo (token) dựa trên ID phiên và thời gian hết hạn.
        /// </summary>
        /// <param name="sessionId">ID của phiên người dùng.</param>
        /// <param name="minutes">Thời gian hết hạn của token (tính bằng phút).</param>
        /// <returns>Chuỗi mã thông báo được mã hóa Base64.</returns>
        public static string CreateToken(string sessionId, int minutes)
        {
            var expire = DateTimeOffset.UtcNow.AddMinutes(minutes).ToUnixTimeSeconds();
            var payload = $"{sessionId}:{expire}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        }

        /// <summary>
        /// Thử phân tích mã thông báo để lấy ID phiên và kiểm tra tính hợp lệ của nó.
        /// </summary>
        /// <param name="token">Chuỗi mã thông báo cần phân tích.</param>
        /// <param name="sessionId">ID phiên được trích xuất từ mã thông báo (nếu hợp lệ).</param>
        /// <returns>
        /// Trả về <c>true</c> nếu mã thông báo hợp lệ và chưa hết hạn, ngược lại trả về <c>false</c>.
        /// </returns>
        public static bool TryParseToken(string token, out string sessionId)
        {
            sessionId = "";
            try
            {
                var raw = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = raw.Split(':');
                if (parts.Length != 2) return false;

                sessionId = parts[0];
                var expireUnix = long.Parse(parts[1]);
                var expireTime = DateTimeOffset.FromUnixTimeSeconds(expireUnix).UtcDateTime;

                return DateTime.UtcNow <= expireTime;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Thiết lập mã thông báo xác thực trong tiêu đề của phản hồi HTTP.
        /// </summary>
        /// <param name="response">Đối tượng HttpResponse để thiết lập tiêu đề.</param>
        /// <param name="sessionId">ID phiên để tạo mã thông báo.</param>
        /// <param name="minutes">Thời gian hết hạn của token (tính bằng phút). Mặc định là 60 phút.</param>
        public static void SetToken(HttpResponse response, string sessionId, int minutes = 60)
        {
            response.SetHeader("X_Token_Authorization", $"{CreateToken(sessionId, minutes)}");
        }

        /// <summary>
        /// Xóa mã thông báo xác thực khỏi tiêu đề của phản hồi HTTP bằng cách đặt giá trị rỗng.
        /// </summary>
        /// <param name="response">Đối tượng HttpResponse để xóa tiêu đề mã thông báo.</param>
        public static void RemoveToken(HttpResponse response)
        {
            response.SetHeader("X_Token_Authorization", "");
        }

        /// <summary>
        /// Lấy mã thông báo xác thực từ tiêu đề của yêu cầu HTTP.
        /// </summary>
        /// <param name="request">Đối tượng HttpRequest chứa các tiêu đề.</param>
        /// <returns>
        /// Chuỗi mã thông báo nếu tìm thấy trong tiêu đề, ngược lại trả về <c>null</c>.
        /// </returns>
        public static string? GetToken(HttpRequest request)
        {
            for (int i = 0; i < request.Headers; i++)
            {
                var (key, value) = request.Header(i);
                if (string.Equals(key, "X_Token_Authorization", StringComparison.OrdinalIgnoreCase))
                    return value;
            }
            return null;
        }

    }
}