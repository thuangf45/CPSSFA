using LuciferCore.Manager;
using LuciferCore.NetCoreServer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using static LuciferCore.Core.Simulation;
using static LuciferCore.Helper.LogHelper;

namespace LuciferCore.Server
{
    /// <summary>
    /// Đại diện cho mô hình máy chủ, quản lý cấu hình, trạng thái và vòng lặp nền của ứng dụng máy chủ.
    /// </summary>
    public class HostServer : ManagerBase
    {
        /// <summary>
        /// Thư mục thực thi của ứng dụng.
        /// </summary>
        private static readonly string ExecutableDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Đường dẫn tới tệp chứng chỉ SSL.
        /// </summary>
        private string certificate = Path.Combine(ExecutableDirectory, "extra_files", "tools", "certificates", "server.pfx");

        /// <summary>
        /// Lấy hoặc thiết lập đường dẫn tới tệp chứng chỉ SSL.
        /// </summary>
        public string Certificate { get => certificate; set => certificate = value; }

        /// <summary>
        /// Mật khẩu cho tệp chứng chỉ SSL.
        /// </summary>
        private string password = "qwerty";

        /// <summary>
        /// Lấy hoặc thiết lập mật khẩu cho tệp chứng chỉ SSL.
        /// </summary>
        public string Password { get => password; set => password = value; }

        /// <summary>
        /// Cổng mạng của máy chủ.
        /// </summary>
        private int port = 2000;

        /// <summary>
        /// Lấy hoặc thiết lập cổng mạng của máy chủ.
        /// </summary>
        public int Port { get => port; set => port = value; }

        /// <summary>
        /// Thư mục chứa nội dung tĩnh (web) của máy chủ.
        /// </summary>
        private string www = Path.Combine(ExecutableDirectory, "extra_files", "www", "User");

        /// <summary>
        /// Lấy hoặc thiết lập thư mục chứa nội dung tĩnh của máy chủ.
        /// </summary>
        public string WWW { get => www; set => www = value; }

        /// <summary>
        /// Ngữ cảnh SSL cho máy chủ.   
        /// </summary>
        private SslContext context;  

        /// <summary>
        /// Lấy ngữ cảnh SSL của máy chủ.
        /// </summary>
        public SslContext Context { get => context; }

        /// <summary>
        /// Bộ điều khiển máy chủ.
        /// </summary>
        private WebServer server;

        /// <summary>
        /// Lấy bộ điều khiển máy chủ.
        /// </summary>
        public WebServer Server { get => server; }

        private int numberRequest = 0;

        /// <summary>
        /// Số lượng yêu cầu đã xử lý.
        /// </summary>
        public int NumberRequest { get => numberRequest; set => numberRequest = value; }

        /// <summary>
        /// Cập nhật số lượng yêu cầu đã xử lý, đảm bảo an toàn đa luồng.
        /// </summary>
        public void UpdateNumberRequest()
        {
            Interlocked.Increment(ref numberRequest);
        }

        private int numberUser = 0;

        /// <summary>
        /// Số lượng người dùng hiện tại.
        /// </summary>
        public int NumberUser { get => numberUser; set => numberUser = value; }


        /// <summary>
        /// Lớp nội bộ lưu trữ thông tin trạng thái máy chủ.
        /// </summary>
        public object GetServerStatus()
        {
            return new { NumberRequest, NumberUser, currentState };
        }

        private NextCommand nextCommand = NextCommand.None;
        private ServerState currentState = ServerState.Stopped;

        public ServerState CurrentState => currentState;

        // Gọi từ bên ngoài để đặt lệnh
        public void RequestStart() => nextCommand = NextCommand.Start;
        public void RequestStop() => nextCommand = NextCommand.Stop;
        public void RequestRestart() => nextCommand = NextCommand.Restart;

        /// <summary>
        /// Khởi tạo các thành phần quản lý và MVP (Model-View-Presenter).
        /// </summary>
        public void Init()
        {
            // Manager
            SetModel(new LogManager());
            SetModel(new SimulationManager());
            SetModel(new SessionManager());
            SetModel(new NotifyManager());

            Setup();
        }

        /// <summary>
        /// Cấu hình máy chủ với các thông số tùy chỉnh.
        /// </summary>
        /// <param name="port">Cổng mạng (nếu > 0, sẽ ghi đè giá trị hiện tại).</param>
        /// <param name="certificate">Đường dẫn tới tệp chứng chỉ SSL (nếu không rỗng, sẽ ghi đè).</param>
        /// <param name="www">Thư mục nội dung tĩnh (nếu không rỗng, sẽ ghi đè).</param>
        public void Setup(int port = -1, string certificate = "", string www = "")
        {
            if (!certificate.IsNullOrEmpty()) Certificate = certificate;
            if (!www.IsNullOrEmpty()) WWW = www;
            if (port > 0) Port = port;

            context = new SslContext(SslProtocols.Tls13, new X509Certificate2(Certificate, Password));
            server = new WebServer(Context, IPAddress.Any, Port);
            server.AddStaticContent(WWW);
        }

        /// <summary>
        /// Khởi chạy các thành phần quản lý và máy chủ trong luồng nền.
        /// </summary>
        public void StartService()
        {
            // Nếu server đã null hoặc đã bị dừng -> tạo lại từ đầu
            if (server == null || !server.IsStarted)
            {
                Setup(); // tạo lại context + server mới
            }

            Server.Start();
        }
        public void StopService()
        {
            Server.Stop();
        }

        public void RestartService()
        {
            Server.Restart();
        }


        protected override async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    switch (nextCommand)
                    {
                        case NextCommand.Stop:
                            if (currentState != ServerState.Stopped && currentState != ServerState.Stopping)
                            {
                                currentState = ServerState.Stopping;
                                StopService();
                                currentState = ServerState.Stopped;
                            }
                            nextCommand = NextCommand.None;
                            break;

                        case NextCommand.Start:
                            if (currentState != ServerState.Started && currentState != ServerState.Starting)
                            {
                                currentState = ServerState.Starting;
                                StartService();
                                currentState = ServerState.Started;
                            }
                            nextCommand = NextCommand.None;
                            break;

                        case NextCommand.Restart:
                            currentState = ServerState.Restarting;
                            RestartService();
                            currentState = ServerState.Started;
                            nextCommand = NextCommand.None;
                            break;

                        default:
                            break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    GetModel<LogManager>().Log(ex);
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
            }
        }

        protected override void OnStarted()
        {
            LogConsole("✅ HostServer started.", LogLevel.INFO, LogSource.SYSTEM);
            // Khởi chạy server và các manager khác
            GetModel<LogManager>().Start();
            GetModel<SimulationManager>().Start();
            GetModel<SessionManager>().Start();
            GetModel<NotifyManager>().Start();

        }

        protected override void OnStopping()
        {
            // Chuẩn bị dừng
            currentState = ServerState.Stopping;
            StopService();
            GetModel<NotifyManager>().Stop();
            GetModel<SessionManager>().Stop();
            GetModel<SimulationManager>().Stop();
            Thread.Sleep(1);
            GetModel<LogManager>().Stop();
        }

        protected override void OnStopped()
        {
            currentState = ServerState.Stopped;
            LogConsole("✅ HostServer stopped", LogLevel.INFO, LogSource.SYSTEM);
        }

    }
    public enum NextCommand
    {
        None,
        Start,
        Stop,
        Restart
    }

    public enum ServerState
    {
        None,
        Starting,
        Started,
        Stopping,
        Stopped,
        Restarting
    }
}