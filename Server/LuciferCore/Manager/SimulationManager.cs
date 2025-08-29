using LuciferCore.Core;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý vòng lặp mô phỏng (simulation) chạy nền, đảm bảo xử lý các sự kiện định kỳ và an toàn đa luồng.
    /// </summary>
    public class SimulationManager : ManagerBase
    {
        /// <summary>
        /// Giới hạn số lượng tác vụ xử lý sự kiện đồng thời, mặc định là 20.
        /// </summary>
        public readonly SemaphoreSlim Limiter = new SemaphoreSlim(25);

        /// <summary>
        /// Vòng lặp chính chạy nền của <see cref="SimulationManager"/>, gọi <see cref="Simulation.Tick"/> định kỳ.
        /// </summary>
        /// <param name="token">Mã hủy để dừng tác vụ một cách an toàn.</param>
        /// <returns>Tác vụ bất đồng bộ chạy vòng lặp mô phỏng.</returns>
        protected override async Task Run(CancellationToken token)
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
                await Task.Delay(50, token);
            }
        }

        protected override void OnStarted()
        {
            Simulation.GetModel<LogManager>().Log("SimulationManager started.", LogLevel.INFO, LogSource.SYSTEM);
        }

        protected override void OnStopped()
        {
            Simulation.GetModel<LogManager>().Log("SimulationManager stopped.", LogLevel.INFO, LogSource.SYSTEM);
        }
    }
}