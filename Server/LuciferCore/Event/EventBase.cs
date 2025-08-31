using LuciferCore.Core;
using LuciferCore.Helper;
using LuciferCore.Interface;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;
using LuciferCore.Handler;

namespace LuciferCore.Event
{
    /// <summary>
    /// Event API tổng quát, map trực tiếp 1 handler cụ thể để xử lý request.
    /// </summary>
    /// <typeparam name="THandler">Handler tương ứng</typeparam>
    public class EventBase<THandler>
        : Simulation.Event<EventBase<THandler>>, IApiEvent
        where THandler : HandlerBase, new()
    {
        public HttpRequest request { get; set; }
        public HttpsSession session { get; set; }

        public override void Execute()
        {
            if (request != null && session != null)
            {
                Task.Run(async () =>
                {
                    await Simulation.GetModel<SimulationManager>().Limiter.WaitAsync();
                    try
                    {
                        Simulation.GetModel<THandler>().Handle(request, session);
                    }
                    catch (Exception ex)
                    {
                        session.SendResponseAsync(
                            ResponseHelper.MakeJsonResponse(session.Response, 500)
                        );
                        Simulation.GetModel<LogManager>().Log(
                            $"Lỗi trong API {typeof(THandler).Name}: {ex}",
                            LogLevel.ERROR,
                            LogSource.SYSTEM
                        );
                    }
                    finally
                    {
                        Simulation.GetModel<SimulationManager>().Limiter.Release();
                    }
                });
            }
        }
    }
}
