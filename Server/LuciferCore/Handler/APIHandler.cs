using LuciferCore.Core;
using LuciferCore.Event;
using LuciferCore.Interface;
using LuciferCore.NetCoreServer;
using Server.LuciferCore.Handler;
using System.Reflection;
using static LuciferCore.Manager.SessionManager;

namespace LuciferCore.Handler
{
    internal static class APIHandler
    {
        private static readonly Dictionary<string, (Type Handler, UserRole MinRole)> routeMap = new();

        // Cache sẵn method Schedule<T>(float)
        private static readonly MethodInfo scheduleGeneric = typeof(Simulation).GetMethod(
            nameof(Simulation.Schedule),
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[] { typeof(float) },
            null
        ) ?? throw new InvalidOperationException("Không tìm thấy Simulation.Schedule<T>(float)!");

        /// <summary>
        /// Đăng ký 1 URL mapping tới Handler
        /// </summary>
        public static void AddAPI<THandler>(string url, UserRole minRole)
            where THandler : HandlerBase, new()
        {
            routeMap[url] = (typeof(THandler), minRole);
        }

        public static bool CanAccess(string url, UserRole role)
        {
            if (routeMap.TryGetValue(url, out var entry))
            {
                return role >= entry.MinRole;
            }
            return false;
        }

        /// <summary>
        /// Bỏ đăng ký route
        /// </summary>
        public static void RemoveAPI(string url) => routeMap.Remove(url);

        /// <summary>
        /// Xử lý request: map url → handler → ApiEvent
        /// </summary>
        public static void Handle(HttpRequest request, HttpsSession session)
        {
            if (!routeMap.TryGetValue(request.Url, out var entry))
                return;

            var handlerType = entry.Handler;
            var eventType = typeof(ApiEvent<>).MakeGenericType(handlerType);
            var genericMethod = scheduleGeneric.MakeGenericMethod(eventType);
            var result = genericMethod.Invoke(null, new object[] { 0.25f });

            if (result is not IApiEvent ev)
                throw new InvalidOperationException($"Không thể tạo ApiEvent cho {handlerType.Name}");

            ev.request = request;
            ev.session = session;
        }
    }
}
