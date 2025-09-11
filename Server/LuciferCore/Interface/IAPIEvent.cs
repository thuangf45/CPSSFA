using LuciferCore.NetCoreServer;

namespace LuciferCore.Interface
{
    /// <summary>
    /// Định nghĩa giao diện cho các sự kiện API, cung cấp các thuộc tính để truy cập yêu cầu HTTP và phiên HTTPS.
    /// </summary>
    internal interface IApiEvent
    {
        /// <summary>
        /// Lấy hoặc thiết lập đối tượng yêu cầu HTTP liên quan đến sự kiện API.
        /// </summary>
        public HttpRequest request { get; set; }

        /// <summary>
        /// Lấy hoặc thiết lập đối tượng phiên HTTPS liên quan đến sự kiện API.
        /// </summary>
        public HttpsSession session { get; set; }
    }
}