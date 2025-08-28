using LuciferCore.Event;
using LuciferCore.Interface;
using LuciferCore.NetCoreServer;
using static LuciferCore.Core.Simulation;

namespace LuciferCore.Handler
{
    internal class APIHandler
    {
        private readonly static List<IHandler> handlers = new()
        {
            //GetModel<APICacheHandler>(),
            //GetModel<APIAuthHandler>(),
            //GetModel<APIUserHandler>(),
            //GetModel<APIGameHandler>(),
            //GetModel<APIReviewHandler>(),
            //GetModel<APICommentHandler>(),
            //GetModel<APIReactionHandler>()
        };

        // Trả về IApiEvent thay vì Event gốc
        private readonly static Dictionary<Type, Func<IApiEvent>> handlerEventMap = new()
        {
            //{ typeof(APICacheHandler), () => Schedule<CacheEvent>(0.25f) },
            //{ typeof(APIAuthHandler), () => Schedule<APIAuthEvent>(0.25f) },
            //{ typeof(APIUserHandler), () => Schedule<APIUserEvent>(0.25f) },
            //{ typeof(APIGameHandler), () => Schedule<APIGameEvent>(0.25f) },
            //{ typeof(APIReviewHandler), () => Schedule<APIReviewEvent>(0.25f) },
            //{ typeof(APICommentHandler), () => Schedule<APICommentEvent>(0.25f) },
            //{ typeof(APIReactionHandler), () => Schedule<APIReactionEvent>(0.25f) }
        };

        public static void AddAPI()
        {

        }

        public static void Remove()
        {

        }

        public static void Handle(HttpRequest request, HttpsSession session)
        {
            string key = request.Url;
            var handler = handlers.FirstOrDefault(h => h.CanHandle(key));

            if (handler != null && handlerEventMap.TryGetValue(handler.GetType(), out var scheduleFunc))
            {
                var ev = scheduleFunc();  // Đây là IApiEvent
                ev.request = request;
                ev.session = session;
            }
        }
    }
}
