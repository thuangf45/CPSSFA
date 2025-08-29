using LuciferCore.Core;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;
using LuciferCore.Session;
using System.Net;
using System.Net.Sockets;
using static LuciferCore.Core.Simulation;

namespace LuciferCore.Controller
{
    /// <summary>
    /// HTTPS server controller chịu trách nhiệm khởi tạo server, tạo session cho mỗi kết nối,
    /// và xử lý lỗi trong quá trình vận hành server.
    /// </summary>
    public class WebServer : HttpsServer
    {
        /// <summary>
        /// Khởi tạo một HTTPS server với SSL context, địa chỉ IP và cổng cụ thể.
        /// </summary>
        /// <param name="context">SSL context dùng để mã hóa kết nối.</param>
        /// <param name="address">Địa chỉ IP để lắng nghe.</param>
        /// <param name="port">Cổng để lắng nghe kết nối.</param>
        public WebServer(SslContext context, IPAddress address, int port)
            : base(context, address, port) { }

        /// <summary>
        /// Tạo một session mới khi có kết nối đến.
        /// </summary>
        /// <returns>Phiên làm việc kiểu <see cref="WebSession"/>.</returns>
        protected override SslSession CreateSession()
        {
            return new WebSession(this);
        }

        /// <summary>
        /// Ghi log lỗi khi server gặp lỗi socket.
        /// </summary>
        /// <param name="error">Lỗi socket phát sinh.</param>
        protected override void OnError(SocketError error)
        {
            GetModel<LogManager>().LogSystem($"HTTPS server caught an error: {error}",LogLevel.ERROR);
        }
        protected override void OnStarted()
        {
            GetModel<LogManager>().LogSystem("🚀 Server started!");
        }

        protected override void OnStopped()
        {
            GetModel<LogManager>().LogSystem("🚀 Server stopped!");
        }

    }
}
