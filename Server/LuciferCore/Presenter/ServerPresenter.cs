using LuciferCore.Core;
using LuciferCore.Manager;
using LuciferCore.Model;
using static LuciferCore.Model.ModelServer;

namespace LuciferCore.Presenter
{
    /// <summary>
    /// Điều phối các hoạt động giữa <see cref="ModelServer"/>, <see cref="ViewServer"/> và các thành phần quản lý khác, xử lý các sự kiện và cập nhật giao diện.
    /// </summary>
    public class ServerPresenter
    {
        /// <summary>
        /// Khởi tạo <see cref="ServerPresenter"/> và thiết lập các sự kiện lắng nghe từ <see cref="ViewServer"/>, <see cref="ModelServer"/> và <see cref="LogManager"/>.
        /// </summary>
        public ServerPresenter()
        {
        }

        /// <summary>
        /// Khởi tạo các sự kiện lắng nghe để điều phối giữa model, view và các thành phần quản lý.
        /// </summary>
        /// <remarks>
        /// Thiết lập các sự kiện:
        /// - Lắng nghe sự kiện khởi động và dừng từ <see cref="ViewServer"/>.
        /// - Lắng nghe sự kiện cấu hình máy chủ và cập nhật log, trạng thái từ <see cref="ModelServer"/>.
        /// - Lắng nghe sự kiện log từ <see cref="LogManager"/>.
        /// </remarks>
        public void Init()
        {
            // Lắng nghe Model
            Simulation.GetModel<ModelServer>().CongfiguredServer += Start;

            // Lắng nghe LogManager
            Simulation.GetModel<LogManager>().OnLogPrinted += Log;

            // Lắng nghe DatabaseManager
            Simulation.GetModel<DatabaseManager>().FailedConnectDB += ErrorStop;
        }

        /// <summary>
        /// Khởi chạy các thành phần quản lý và máy chủ trong luồng nền.
        /// </summary>
        private static void Run()
        {
            Simulation.GetModel<LogManager>().Log("⚙️ LogManager.Start()", LogLevel.INFO, LogSource.SYSTEM);
            Simulation.GetModel<LogManager>().Start();

            Simulation.GetModel<LogManager>().Log("⚙️ SimulationManager.Start()", LogLevel.INFO, LogSource.SYSTEM);
            Simulation.GetModel<SimulationManager>().Start();

            Simulation.GetModel<LogManager>().Log("⚙️ SessionManager.Start()", LogLevel.INFO, LogSource.SYSTEM);
            Simulation.GetModel<SessionManager>().Start();

            Simulation.GetModel<LogManager>().Log("⚙️ NotifyManager.Start()", LogLevel.INFO, LogSource.SYSTEM);
            Simulation.GetModel<NotifyManager>().Start();

            Simulation.GetModel<LogManager>().Log("🚀 Model.Start()", LogLevel.INFO, LogSource.SYSTEM);
            Simulation.GetModel<ModelServer>().Start();

            Simulation.GetModel<LogManager>().Log("🚀 Server.Start()", LogLevel.INFO, LogSource.SYSTEM);
            Simulation.GetModel<ModelServer>().Server.Start();

            Simulation.GetModel<LogManager>().Log("✅ Server started!", LogLevel.INFO, LogSource.SYSTEM);
        }

        /// <summary>
        /// Khởi chạy máy chủ trong luồng nền khi được kích hoạt từ sự kiện cấu hình.
        /// </summary>
        /// <remarks>
        /// Chạy <see cref="Run"/> trong một luồng nền và ghi lại bất kỳ lỗi nào vào <see cref="LogManager"/>.
        /// </remarks>
        private void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    Run();
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>().Log(ex);
                }
            });
        }

        /// <summary>
        /// Dừng tất cả các thành phần quản lý và máy chủ trong luồng nền.
        /// </summary>
        /// <remarks>
        /// Dừng các thành phần theo thứ tự: <see cref="SessionManager"/>, <see cref="SimulationManager"/>, <see cref="ModelServer"/>, và máy chủ. Ghi log thông báo dừng hoặc lỗi vào <see cref="LogManager"/>.
        /// </remarks>
        private void Stop()
        {
            Task.Run(() =>
            {
                try
                {
                    Simulation.GetModel<SessionManager>().Stop();
                    Simulation.GetModel<SimulationManager>().Stop();
                    Simulation.GetModel<NotifyManager>().Stop();
                    Simulation.GetModel<ModelServer>().Stop();
                    Simulation.GetModel<ModelServer>().Server.Stop();
                    Simulation.GetModel<LogManager>().Log("Server stopped.", LogLevel.INFO, LogSource.SYSTEM);
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>().Log(ex);
                }
            });
        }

        private void ErrorStop()
        {
            Task.Run(() =>
            {
                try
                {
                    
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>().Log(ex);
                }
            });
        }

        /// <summary>
        /// Cấu hình máy chủ trong luồng nền khi được kích hoạt từ giao diện.
        /// </summary>
        /// <remarks>
        /// Gọi phương thức <see cref="ModelServer.CongfigureServer"/> và ghi lại bất kỳ lỗi nào vào <see cref="LogManager"/>.
        /// </remarks>
        private void SetUp()
        {
            Task.Run(() =>
            {
                try
                {
                    Simulation.GetModel<ModelServer>().CongfigureServer();
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>().Log(ex);
                }
            });
        }

        /// <summary>
        /// Ghi log vào <see cref="ModelServer"/> trong luồng nền.
        /// </summary>
        /// <param name="source">Nguồn log (<see cref="LogSource"/>).</param>
        /// <param name="newlog">Nội dung log.</param>
        /// <remarks>
        /// Chuyển log tới <see cref="ModelServer.Log"/> và ghi lại bất kỳ lỗi nào vào <see cref="LogManager"/>.
        /// </remarks>
        private void Log(LogSource source, string newlog)
        {
            Task.Run(() =>
            {
                try
                {
                    Simulation.GetModel<ModelServer>().Log(source, newlog);
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>().Log(ex);
                }
            });
        }
    }
}