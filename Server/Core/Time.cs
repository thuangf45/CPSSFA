using System.Diagnostics;

namespace Server.Source.Core
{
    /// <summary>
    /// Cung cấp thời gian chạy (elapsed time) kể từ khi hệ thống khởi động,
    /// sử dụng Stopwatch để đo thời gian thực theo giây.
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Stopwatch dùng để đo thời gian kể từ khi ứng dụng bắt đầu chạy.
        /// </summary>
        private static readonly Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Trả về thời gian đã trôi qua (tính bằng giây, kiểu float) kể từ khi hệ thống bắt đầu.
        /// </summary>
        public static float time => (float)stopwatch.Elapsed.TotalSeconds;
    }
}
