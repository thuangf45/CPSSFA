using LuciferCore.NetCoreServer;

namespace LuciferCore.Interface
{
    /// <summary>
    /// Interface định nghĩa cho các handler xử lý API hoặc route trong server.
    /// Mỗi handler đại diện cho một nhóm endpoint cụ thể.
    /// </summary>
    internal interface IHandler
    {
        /// <summary>
        /// Định danh kiểu hoặc tiền tố route mà handler này xử lý (ví dụ: "/api/user").
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Kiểm tra xem handler này có thể xử lý đường dẫn được yêu cầu không.
        /// </summary>
        /// <param name="path">Đường dẫn được yêu cầu.</param>
        /// <returns>True nếu handler này xử lý được, false nếu không.</returns>
        bool CanHandle(string path);

        /// <summary>
        /// Xử lý một yêu cầu HTTP cụ thể.
        /// </summary>
        /// <param name="request">Yêu cầu HTTP đến từ client.</param>
        /// <param name="session">Phiên kết nối hiện tại của client.</param>
        void Handle(HttpRequest request, HttpsSession session);
    }
}
