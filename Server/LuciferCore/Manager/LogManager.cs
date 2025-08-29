using LuciferCore.Core;
using System.Collections.Concurrent;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý hệ thống ghi log chạy nền, đảm bảo an toàn đa luồng. Ghi log vào tệp theo ngày và phát sự kiện <see cref="OnLogPrinted"/> để hiển thị log.
    /// </summary>
    public class LogManager : ManagerBase
    {
        /// <summary>
        /// Hàng đợi log an toàn đa luồng để lưu trữ các mục log chờ xử lý.
        /// </summary>
        private readonly BlockingCollection<(LogSource source, string message)> _logQueue = new();

        /// <summary>
        /// Sự kiện được kích hoạt khi có log mới, cho phép các lớp khác (như UI) lắng nghe và hiển thị log.
        /// </summary>
        public event Action<LogSource, string> OnLogPrinted;

        /// <summary>
        /// Đường dẫn tệp log cho người dùng, được tạo theo ngày.
        /// </summary>
        private string _logUserFilePath;

        /// <summary>
        /// Đường dẫn tệp log cho hệ thống, được tạo theo ngày.
        /// </summary>
        private string _logSystemFilePath;

        /// <summary>
        /// Định dạng log với thời gian, mức độ log và nội dung thông điệp.
        /// </summary>
        /// <param name="message">Nội dung log.</param>
        /// <param name="level">Mức độ log (<see cref="LogLevel"/>).</param>
        /// <returns>Chuỗi log được định dạng.</returns>
        private string FormatLog(string message, LogLevel level)
            => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

        /// <summary>
        /// Thêm một mục log mới vào hàng đợi để xử lý nền.
        /// </summary>
        /// <param name="message">Nội dung log.</param>
        /// <param name="level">Mức độ log (INFO, WARN, ERROR, DEBUG). Mặc định là <see cref="LogLevel.INFO"/>.</param>
        /// <param name="source">Nguồn log (USER hoặc SYSTEM). Mặc định là <see cref="LogSource.USER"/>.</param>
        public void Log(string message, LogLevel level = LogLevel.INFO, LogSource source = LogSource.USER)
        {
            var logEntry = FormatLog(message, level);
            _logQueue.Add((source, logEntry)); // Lưu cả nguồn và thông điệp đã định dạng
        }

        /// <summary>
        /// Ghi log cho một ngoại lệ hệ thống.
        /// </summary>
        /// <param name="ex">Ngoại lệ cần ghi log.</param>
        /// <param name="level">Mức độ log (mặc định là <see cref="LogLevel.ERROR"/>).</param>
        /// <param name="source">Nguồn log (mặc định là <see cref="LogSource.SYSTEM"/>).</param>
        public void Log(Exception ex, LogLevel level = LogLevel.ERROR, LogSource source = LogSource.SYSTEM)
            => Log(ex.ToString(), level, source);

        public void LogSystem(string message, LogLevel level = LogLevel.INFO)
        {
            Log(message, level, LogSource.SYSTEM);
        }
        public void LogUser(string message, LogLevel level = LogLevel.INFO)
        {
            Log(message, level, LogSource.USER);
        }

        public LogManager()
        {
            // Tạo thư mục 'logs' nếu chưa tồn tại
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            var log_user = Path.Combine(logDir, "log_user");
            var log_system = Path.Combine(logDir, "log_system");
            Directory.CreateDirectory(logDir);
            Directory.CreateDirectory(log_user);
            Directory.CreateDirectory(log_system);

            // Tạo tệp log tên theo ngày, ví dụ: logs/log_user/log_2025-06-27.txt
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            _logUserFilePath = Path.Combine(log_user, $"log_{date}.txt");
            _logSystemFilePath = Path.Combine(log_system, $"log_{date}.txt");
        }

        /// <summary>
        /// Vòng lặp xử lý log nền, lấy log từ hàng đợi, ghi vào tệp và phát sự kiện <see cref="OnLogPrinted"/>.
        /// </summary>
        /// <param name="token">Mã hủy để dừng tác vụ một cách an toàn.</param>
        /// <returns>Tác vụ bất đồng bộ xử lý log.</returns>
        protected override async Task Run(CancellationToken token)
        {
            var currentDate = DateTime.Now.Date;
            StreamWriter writerU = null;
            StreamWriter writerS = null;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Mỗi lần có lỗi sẽ mở lại writer để tránh lỗi writer bị đóng hoặc bị khóa
                    writerU = new StreamWriter(_logUserFilePath, true) { AutoFlush = true };
                    writerS = new StreamWriter(_logSystemFilePath, true) { AutoFlush = true };

                    // Ghi log cho đến khi bị hủy hoặc CompleteAdding()
                    foreach (var (source, log) in _logQueue.GetConsumingEnumerable(token))
                    {
                        // Nếu bị hủy giữa chừng
                        if (token.IsCancellationRequested) break;

                        // Gửi log cho UI
                        OnLogPrinted?.Invoke(source, log);

                        // Ghi log ra tệp
                        if (source == LogSource.USER)
                        {
                            writerU.WriteLine(log);
                        }
                        else
                        {
                            writerS.WriteLine(log);
                        }
                    }

                    // Nếu ra khỏi foreach do CompleteAdding(), thì kết thúc luôn
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Được phép dừng hợp lệ
                    break;
                }
                catch (Exception ex)
                {
                    // Ghi lỗi ra stderr và tiếp tục vòng while để thử lại
                    Log(ex);
                    await Task.Delay(1000, token);
                }
                finally
                {
                    writerU?.Dispose();
                    writerS?.Dispose();
                }

                await Task.Delay(100, token);
            }
        }

        protected override void OnStarted()
        {
            Simulation.GetModel<LogManager>().Log("SimulationManager started.", LogLevel.INFO, LogSource.SYSTEM);
        }

        protected override void OnStopping()
        {
            _logQueue.CompleteAdding(); // Không cho thêm log mới
        }

        protected override void OnStopped()
        {
            Simulation.GetModel<LogManager>().Log("SimulationManager stopped.", LogLevel.INFO, LogSource.SYSTEM);
        }
    }

    /// <summary>
    /// Các mức độ log được hỗ trợ bởi <see cref="LogManager"/>.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Thông tin chung.</summary>
        INFO,
        /// <summary>Cảnh báo có thể ảnh hưởng.</summary>
        WARN,
        /// <summary>Lỗi nghiêm trọng cần xử lý.</summary>
        ERROR,
        /// <summary>Thông tin chi tiết phục vụ debug.</summary>
        DEBUG
    }

    /// <summary>
    /// Các nguồn log được hỗ trợ bởi <see cref="LogManager"/>.
    /// </summary>
    public enum LogSource
    {
        /// <summary>Log từ hành động của người dùng.</summary>
        USER,
        /// <summary>Log từ hệ thống hoặc lỗi hệ thống.</summary>
        SYSTEM
    }
}