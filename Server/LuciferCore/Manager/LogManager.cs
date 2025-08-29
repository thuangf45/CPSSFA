using LuciferCore.Core;
using System.Collections.Concurrent;
using static LuciferCore.Core.Simulation;

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
        private BlockingCollection<(LogSource source, string message)> _logQueue = new();

        /// <summary>
        /// Đường dẫn tệp log cho người dùng, được tạo theo ngày.
        /// </summary>
        private string _logUserFilePath;

        /// <summary>
        /// Đường dẫn tệp log cho hệ thống, được tạo theo ngày.
        /// </summary>
        private string _logSystemFilePath;

        public LogManager()
        {
            InitLogFiles();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

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
            var queue = _logQueue; // snapshot để tránh bị thay đổi giữa chừng
            if (queue != null && !queue.IsAddingCompleted)
            {
                queue.Add((source, logEntry)); // Lưu cả nguồn và thông điệp đã định dạng
            }
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

        public void InitLogFiles()
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
                    writerU ??= new StreamWriter(_logUserFilePath, true) { AutoFlush = true };
                    writerS ??= new StreamWriter(_logSystemFilePath, true) { AutoFlush = true };

                    foreach (var (source, log) in _logQueue.GetConsumingEnumerable(token))
                    {
                        if (token.IsCancellationRequested) break;

                        // Rotate theo ngày
                        if (DateTime.Now.Date != currentDate)
                        {
                            currentDate = DateTime.Now.Date;

                            writerU.Dispose();
                            writerS.Dispose();

                            InitLogFiles();

                            writerU = new StreamWriter(_logUserFilePath, true) { AutoFlush = true };
                            writerS = new StreamWriter(_logSystemFilePath, true) { AutoFlush = true };
                        }

                        if (source == LogSource.USER)
                        {
                            await writerU.WriteLineAsync(log);
                        }
                        else
                        {
                            Console.WriteLine(log);
                            await writerS.WriteLineAsync(log);
                        }
                    }

                    // CompleteAdding => thoát luôn
                    break;
                }
                catch (OperationCanceledException)
                {
                    break; // Dừng hợp lệ
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[LogManager] Error: {ex}");
                    writerU?.Dispose();
                    writerS?.Dispose();
                    writerU = null;
                    writerS = null;
                    await Task.Delay(1000, token); // nghỉ 1s rồi retry
                }
            }

            writerU?.Dispose();
            writerS?.Dispose();
        }
        public override void Restart()
        {
            Stop(); // Dừng task cũ + CompleteAdding()
            _logQueue = new BlockingCollection<(LogSource source, string message)>(); // Tạo queue mới
            Start(); // Chạy task mới với queue mới
        }

        protected override void OnStarted()
        {
            GetModel<LogManager>().LogSystem("⚙️ LogManager started");
        }

        protected override void OnStopping()
        {
            _logQueue.CompleteAdding(); // Không cho thêm log mới
        }

        protected override void OnStopped()
        {
            // Không push thêm log nữa, chỉ in thẳng ra console
            Console.WriteLine("⚙️ LogManager stopped");
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