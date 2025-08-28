using LuciferCore.Core;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý vòng lặp mô phỏng (simulation) chạy nền, đảm bảo xử lý các sự kiện định kỳ và an toàn đa luồng.
    /// </summary>
    public class SimulationManager
    {
        /// <summary>
        /// Bộ điều khiển tín hiệu hủy để dừng tác vụ nền một cách an toàn.
        /// </summary>
        private CancellationTokenSource _cts = new();

        /// <summary>
        /// Tác vụ nền xử lý vòng lặp mô phỏng.
        /// </summary>
        private Task _simulationTask;

        /// <summary>
        /// Giới hạn số lượng tác vụ xử lý sự kiện đồng thời, mặc định là 20.
        /// </summary>
        public readonly SemaphoreSlim Limiter = new SemaphoreSlim(20);

        /// <summary>
        /// Khởi động <see cref="SimulationManager"/> để chạy vòng lặp mô phỏng trong nền.
        /// </summary>
        /// <remarks>
        /// Nếu tác vụ đã chạy, phương thức sẽ bỏ qua. Nếu tác vụ đã bị hủy trước đó, một <see cref="CancellationTokenSource"/> mới sẽ được tạo.
        /// </remarks>
        public void Start()
        {
            if (_simulationTask != null && !_simulationTask.IsCompleted)
                return; // Đã chạy rồi

            if (_cts.IsCancellationRequested)
                _cts = new CancellationTokenSource(); // Reset token nếu đã hủy trước đó

            _simulationTask = Task.Run(() => Run(_cts.Token));
        }

        /// <summary>
        /// Khởi động lại <see cref="SimulationManager"/> bằng cách dừng và khởi chạy lại.
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Dừng vòng lặp mô phỏng và tất cả các tác vụ nền, ghi log thông báo.
        /// </summary>
        /// <remarks>
        /// Gửi tín hiệu hủy và đợi tác vụ nền hoàn tất. Ghi log thông báo dừng vào <see cref="LogManager"/>.
        /// </remarks>
        public void Stop()
        {
            _cts.Cancel();

            try
            {
                _simulationTask?.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => e is OperationCanceledException);
            }

            Simulation.GetModel<LogManager>().Log("SimulationManager stopped.", LogLevel.INFO, LogSource.SYSTEM);
        }

        /// <summary>
        /// Vòng lặp chính chạy nền của <see cref="SimulationManager"/>, gọi <see cref="Simulation.Tick"/> định kỳ.
        /// </summary>
        /// <param name="token">Mã hủy để dừng tác vụ một cách an toàn.</param>
        /// <returns>Tác vụ bất đồng bộ chạy vòng lặp mô phỏng.</returns>
        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Simulation.Tick();
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>().Log(ex);
                    await Task.Delay(1000, token);
                }
                await Task.Delay(10, token);
            }
        }
    }
}