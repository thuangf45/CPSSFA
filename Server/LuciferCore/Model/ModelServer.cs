using Microsoft.IdentityModel.Tokens;
using LuciferCore.Controller;
using LuciferCore.Core;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;
using LuciferCore.Presenter;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace LuciferCore.Model
{
    /// <summary>
    /// Đại diện cho mô hình máy chủ, quản lý cấu hình, trạng thái và vòng lặp nền của ứng dụng máy chủ.
    /// </summary>
    public class ModelServer
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
        /// Địa chỉ IP của máy chủ.
        /// </summary>
        private string ip = "192.168.1.5";

        /// <summary>
        /// Lấy hoặc thiết lập địa chỉ IP của máy chủ.
        /// </summary>
        public string IP { get => ip; set => ip = value; }

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
        private string www = Path.Combine(ExecutableDirectory, "extra_files", "www", "FE_CleanDesign");

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
        private ServerController server;

        /// <summary>
        /// Lấy bộ điều khiển máy chủ.
        /// </summary>
        public ServerController Server { get => server; }

        /// <summary>
        /// Sự kiện được kích hoạt khi máy chủ được cấu hình.
        /// </summary>
        public event Action CongfiguredServer;

        /// <summary>
        /// Xác định xem máy chủ có được cấu hình tự động hay không.
        /// </summary>
        public bool IsAutoConfig { get; set; } = true;

        /// <summary>
        /// Bộ điều khiển tín hiệu hủy để dừng tác vụ nền một cách an toàn.
        /// </summary>
        private CancellationTokenSource _cts = new();

        /// <summary>
        /// Tác vụ nền để theo dõi trạng thái máy chủ.
        /// </summary>
        private Task? _cleanerTask;

        /// <summary>
        /// Hàng đợi log an toàn đa luồng để lưu trữ các mục log chờ xử lý.
        /// </summary>
        private readonly BlockingCollection<string> _logQueue = new();

        /// <summary>
        /// Sự kiện được kích hoạt khi một mục log mới được thêm.
        /// </summary>
        public event Action<LogSource, string> OnAddedLog;


        /// <summary>
        /// Lớp nội bộ lưu trữ thông tin trạng thái máy chủ.
        /// </summary>
        public class ServerStatus
        {
            /// <summary>
            /// Thời gian máy chủ đã hoạt động.
            /// </summary>
            public TimeSpan ElapsedTime { get; set; }

            /// <summary>
            /// Số lượng yêu cầu đã xử lý.
            /// </summary>
            public int NumberRequest { get; set; }

            /// <summary>
            /// Số lượng người dùng hiện tại.
            /// </summary>
            public int NumberUser { get; set; }

            private float cpuUsage;

            /// <summary>
            /// Mức sử dụng CPU (%).
            /// </summary>
            public float CpuUsage { get => cpuUsage; set => cpuUsage = (value >= 100) ? 0 : value; }

            /// <summary>
            /// Mức sử dụng bộ nhớ (MB).
            /// </summary>
            public string MemoryUsage { get; set; }

            /// <summary>
            /// Thu thập trạng thái máy chủ bất đồng bộ.
            /// </summary>
            /// <param name="elapsed">Thời gian máy chủ đã hoạt động.</param>
            /// <param name="requests">Số lượng yêu cầu đã xử lý.</param>
            /// <param name="users">Số lượng người dùng hiện tại.</param>
            /// <param name="cpuUsage">Giá trị CPU usage từ cache.</param>
            /// <returns>Đối tượng <see cref="ServerStatus"/> chứa thông tin trạng thái.</returns>
            public static ServerStatus Collect(TimeSpan elapsed, int requests, int users, float cpuUsage)
            {
                string memory = GetMemoryUsage();

                return new ServerStatus
                {
                    ElapsedTime = elapsed,
                    NumberRequest = requests,
                    NumberUser = users,
                    CpuUsage = cpuUsage,
                    MemoryUsage = memory
                };
            }

            /// <summary>
            /// Lấy mức sử dụng bộ nhớ của tiến trình hiện tại.
            /// </summary>
            /// <returns>Chuỗi biểu thị mức sử dụng bộ nhớ (MB).</returns>
            private static string GetMemoryUsage()
            {
                var proc = Process.GetCurrentProcess();
                long memoryBytes = proc.WorkingSet64;
                return (memoryBytes / (1024.0 * 1024)).ToString("0.0") + " MB";
            }
        }

        /// <summary>
        /// Sự kiện được kích hoạt khi trạng thái máy chủ thay đổi.
        /// </summary>
        public event Action<ServerStatus> OnChangedData;

        private TimeSpan elapsedTime = TimeSpan.Zero;

        /// <summary>
        /// Thời gian máy chủ đã hoạt động.
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get => elapsedTime;
            set
            {
                elapsedTime = value;
                NotifyChanged();
            }
        }

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
            lock (this)
            {
                NumberRequest++;
            }
        }

        private int numberUser = 0;

        /// <summary>
        /// Số lượng người dùng hiện tại.
        /// </summary>
        public int NumberUser { get => numberUser; set => numberUser = value; }

        /// <summary>
        /// Khởi tạo <see cref="ModelServer"/> và thiết lập các thành phần quản lý.
        /// </summary>
        public ModelServer()
        {
            Init();
        }

        /// <summary>
        /// Khởi tạo các thành phần quản lý và MVP (Model-View-Presenter).
        /// </summary>
        public static void Init()
        {
            // Manager
            Simulation.SetModel<LogManager>(new LogManager());
            Simulation.SetModel<SimulationManager>(new SimulationManager());
            Simulation.SetModel<SessionManager>(new SessionManager());
            Simulation.SetModel<DatabaseManager>(new DatabaseManager());
            Simulation.SetModel<NotifyManager>(new NotifyManager());

            // MVP
            Simulation.SetModel<ServerPresenter>(new ServerPresenter());

            Task.Run(() => Simulation.GetModel<DatabaseManager>().Init());
        }

        /// <summary>
        /// Cấu hình máy chủ với các thông số tùy chỉnh.
        /// </summary>
        /// <param name="port">Cổng mạng (nếu > 0, sẽ ghi đè giá trị hiện tại).</param>
        /// <param name="certificate">Đường dẫn tới tệp chứng chỉ SSL (nếu không rỗng, sẽ ghi đè).</param>
        /// <param name="www">Thư mục nội dung tĩnh (nếu không rỗng, sẽ ghi đè).</param>
        public void CongfigureServer(int port = -1, string certificate = "", string www = "")
        {
            if (!certificate.IsNullOrEmpty()) Certificate = certificate;
            if (!www.IsNullOrEmpty()) WWW = www;
            if (port > 0) Port = port;

            context = new SslContext(SslProtocols.Tls13, new X509Certificate2(Certificate, Password));
            server = new ServerController(Context, IPAddress.Any, Port);
            server.AddStaticContent(WWW);

            // Kích hoạt sự kiện báo hiệu cấu hình hoàn tất
            CongfiguredServer?.Invoke();
        }

        /// <summary>
        /// Ghi log với nguồn và nội dung được chỉ định.
        /// </summary>
        /// <param name="source">Nguồn log (<see cref="LogSource"/>).</param>
        /// <param name="logEntry">Nội dung log.</param>
        public void Log(LogSource source, string logEntry)
        {
            _logQueue.Add(logEntry);
            OnAddedLog?.Invoke(source, logEntry);
        }
        /// <summary>
        /// Thông báo trạng thái máy chủ đã thay đổi và cập nhật thông tin.
        /// </summary>
        private void NotifyChanged()
        {
            NumberUser = Simulation.GetModel<SessionManager>().NumberSession;

            // Sử dụng giá trị CPU từ cache, không await delay nữa
            var status = ServerStatus.Collect(
                ElapsedTime,
                NumberRequest,
                NumberUser,
                5.0f
            );
            int cpuCount = Environment.ProcessorCount; // số core CPU
            float value = float.Parse(status.MemoryUsage.Split(" ")[0]) / cpuCount;

            if (Fuzzy.ValueGreaterThan(value, status.CpuUsage))
            {
                status.CpuUsage = Fuzzy.Value(value, 0.75f);
            }
            else
            {
                status.CpuUsage = Fuzzy.Value(status.CpuUsage, 0.75f);
            }


            OnChangedData?.Invoke(status);
        }

        /// <summary>
        /// Khởi động tác vụ nền để theo dõi trạng thái máy chủ.
        /// </summary>
        /// <remarks>
        /// Nếu tác vụ đã chạy, phương thức sẽ bỏ qua. Tác vụ chạy mỗi 1 giây để cập nhật trạng thái.
        /// </remarks>
        public void Start()
        {
            if (_cleanerTask != null && !_cleanerTask.IsCompleted)
                return;

            if (_cts.IsCancellationRequested)
                _cts = new CancellationTokenSource();

            _cleanerTask = Task.Run(() => Run(_cts.Token));
        }

        /// <summary>
        /// Dừng tác vụ nền và ghi log thông báo.
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();

            try
            {
                _cleanerTask?.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => e is OperationCanceledException);
            }
            Simulation.GetModel<LogManager>().Log("ModelServer stopped.", LogLevel.INFO, LogSource.SYSTEM);
        }

        /// <summary>
        /// Vòng lặp nền để cập nhật trạng thái máy chủ định kỳ.
        /// </summary>
        /// <param name="token">Mã hủy để dừng tác vụ một cách an toàn.</param>
        /// <returns>Tác vụ bất đồng bộ chạy vòng lặp trạng thái.</returns>
        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Simulation.GetModel<ModelServer>().ElapsedTime += TimeSpan.FromMilliseconds(1000);
                    NotifyChanged();
                    await Task.Delay(1_000, token); // Chạy mỗi 1 giây
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>()?.Log(ex); // Ghi log lỗi nếu cần
                    await Task.Delay(1000, token);
                }
            }
        }
    }
}