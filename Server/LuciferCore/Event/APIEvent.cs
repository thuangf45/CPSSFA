using LuciferCore.Core;
using LuciferCore.Handler;
using LuciferCore.Helper;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;

namespace LuciferCore.Event
{
    internal class APIEvent : Simulation.Event<APIEvent>
    {
        public HttpRequest? request;
        public HttpsSession? session;
        public override void Execute()
        {
            if (request != null && session != null)
            {
                if (request != null && session != null)
                {
                    // Chạy luồng bất đồng bộ
                    Task.Run(async () =>
                    {
                        // Đảm bảo đồng độ ko quá nhiều luồng
                        await Simulation.GetModel<SimulationManager>().Limiter.WaitAsync();
                        try
                        {
                            APIHandler.Handle(request, session);
                        }
                        catch (Exception ex)
                        {
                            session.SendResponseAsync(ResponseHelper.MakeJsonResponse(session.Response, 500));
                            Simulation.GetModel<LogManager>().Log("Lỗi trong API: " + ex.ToString(), LogLevel.ERROR, LogSource.SYSTEM);
                        }
                        finally
                        {
                            // Giải phóng luồng đã hoàn thành
                            Simulation.GetModel<SimulationManager>().Limiter.Release();
                        }
                    });

                }
            }
            
        }
    }
}
