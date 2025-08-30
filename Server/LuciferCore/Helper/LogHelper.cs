using LogLevel = LuciferCore.Manager.LogLevel;
using LogSource = LuciferCore.Manager.LogSource;
namespace LuciferCore.Helper
{
    public static class LogHelper
    {
        public static void LogConsole(string message, LogLevel level = LogLevel.INFO, LogSource source = LogSource.SYSTEM)
        {
            var log = FormatLog(message, level);
            Console.WriteLine(log);
        }
        public static void LogConsole(string log)
        {
            Console.WriteLine(log);
        }


        /// <summary>
        /// Định dạng log với thời gian, mức độ log và nội dung thông điệp.
        /// </summary>
        /// <param name="message">Nội dung log.</param>
        /// <param name="level">Mức độ log (<see cref="LogLevel"/>).</param>
        /// <returns>Chuỗi log được định dạng.</returns>
        public static string FormatLog(string message, LogLevel level)
            => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
    }
}
