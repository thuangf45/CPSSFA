using Microsoft.IdentityModel.Tokens;
using LuciferCore.Core;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;

namespace LuciferCore.Helper
{
    /// <summary>
    /// Cung cấp các phương thức hỗ trợ để tạo phản hồi HTTP JSON chuẩn hóa và quản lý phiên người dùng trong ứng dụng máy chủ web.
    /// </summary>
    public static class ResponseHelper
    {
        /// <summary>
        /// Tạo phản hồi JSON chuẩn hóa với thông điệp được xác định trước dựa trên mã trạng thái HTTP.
        /// </summary>
        /// <param name="response">Đối tượng HttpResponse cần được chỉnh sửa.</param>
        /// <param name="status">Mã trạng thái HTTP (ví dụ: 200, 400, 401, v.v.). Mặc định là 200.</param>
        /// <param name="token">Mã thông báo JWT tùy chọn để bao gồm trong tiêu đề phản hồi. Mặc định là chuỗi rỗng.</param>
        /// <returns>Đối tượng HttpResponse đã được chỉnh sửa với trạng thái, tiêu đề và thân JSON phù hợp.</returns>
        public static HttpResponse MakeJsonResponse(this HttpResponse response, int status = 200, string token = "")
        {
            switch (status)
            {
                case 200:
                    return response.MakeJsonResponse(new { success = true, message = "OK" }, status, token);
                case 201:
                    return response.MakeJsonResponse(new { success = true, message = "Tài nguyên được tạo thành công" }, status, token);
                case 400:
                    return response.MakeJsonResponse(new { success = false, message = "Yêu cầu không hợp lệ" }, status, token);
                case 401:
                    return response.MakeJsonResponse(new { success = false, message = "Yêu cầu xác thực" }, status, token);
                case 404:
                    return response.MakeJsonResponse(new { success = false, message = "Không tìm thấy" }, status, token);
                case 500:
                    return response.MakeJsonResponse(new { success = false, message = "Lỗi máy chủ nội bộ" }, status, token);
                default:
                    return response.MakeJsonResponse(null, status, token);
            }
        }

        /// <summary>
        /// Tạo phản hồi JSON với dữ liệu tùy chỉnh, mã trạng thái và mã thông báo tùy chọn.
        /// </summary>
        /// <param name="response">Đối tượng HttpResponse cần được chỉnh sửa.</param>
        /// <param name="data">Đối tượng dữ liệu để tuần tự hóa thành JSON cho thân phản hồi.</param>
        /// <param name="status">Mã trạng thái HTTP. Mặc định là 200.</param>
        /// <param name="token">Mã thông báo JWT tùy chọn để bao gồm trong tiêu đề phản hồi. Mặc định là chuỗi rỗng.</param>
        /// <returns>Đối tượng HttpResponse đã được chỉnh sửa với trạng thái, tiêu đề và thân JSON đã tuần tự hóa.</returns>
        public static HttpResponse MakeJsonResponse(this HttpResponse response, object? data, int status = 200, string token = "")
        {
            response.SetBegin(status);

            if (!token.IsNullOrEmpty())
            {
                response.SetHeader("Access-Control-Expose-Headers", "X_Token_Authorization");
                response.SetHeader("X_Token_Authorization", $"{token}");
            }

            response.SetContentType(".json");
            if (data != null)
            {
                response.SetBody(JsonHelper.Serialize(data));
            }
            return response;
        }

        /// <summary>
        /// Tạo phiên người dùng mới, sinh ID phiên, lưu trữ và trả về phản hồi JSON với mã thông báo JWT.
        /// </summary>
        /// <param name="userId">ID của người dùng liên kết với phiên.</param>
        /// <param name="response">Đối tượng HttpResponse cần được chỉnh sửa.</param>
        /// <returns>Đối tượng HttpResponse đã được chỉnh sửa chứa mã thông báo phiên và phản hồi thành công.</returns>
        public static HttpResponse NewUserSession(string userId, HttpResponse response)
        {
            string newSessionId = Guid.NewGuid().ToString(); // sessionId mới
            Simulation.GetModel<SessionManager>().Store(newSessionId, userId); // lưu phiên

            var token = TokenHelper.CreateToken(newSessionId, 60); // tạo token
            response = response.MakeJsonResponse(200, token);

            return response;
        }
    }
}