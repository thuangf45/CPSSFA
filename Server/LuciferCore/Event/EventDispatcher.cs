using LuciferCore.Core;
using LuciferCore.Interface;
using LuciferCore.NetCoreServer;
using LuciferCore.Handler;
using System.Reflection;
using static LuciferCore.Manager.SessionManager;

namespace LuciferCore.Event
{
    internal static class EventDispatcher
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

        // Static constructor: chạy tự động 1 lần khi class được gọi
        static EventDispatcher()
        {
            Initialize();
        }

        /// <summary>
        /// Đăng ký 1 URL mapping tới Handler (manual override)
        /// </summary>
        public static void AddAPI<THandler>(UserRole minRole, string url = "")
            where THandler : HandlerBase, new()
        {
            var handler = new THandler();
            if (string.IsNullOrWhiteSpace(url))
                url = handler.Type; // lấy Type từ instance

            routeMap[url.ToLower()] = (typeof(THandler), minRole); // override luôn
        }

        /// <summary>
        /// Bỏ đăng ký route
        /// </summary>
        public static void RemoveAPI(string url) => routeMap.Remove(url.ToLower());

        public static bool CanAccess(string url, UserRole role)
        {
            if (routeMap.TryGetValue(url.ToLower(), out var entry))
                return role >= entry.MinRole;
            return false;
        }

        /// <summary>
        /// Quét toàn bộ Assembly để auto-register HandlerBase.
        /// Gọi tự động lúc static ctor, có thể gọi lại thủ công nếu muốn refresh.
        /// </summary>
        public static void Initialize(UserRole defaultRole = UserRole.User)
        {
            var handlers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(HandlerBase).IsAssignableFrom(t));

            foreach (var handlerType in handlers)
            {
                var inst = (HandlerBase)Activator.CreateInstance(handlerType)!;
                var url = inst.Type.ToLower();
                if (!routeMap.ContainsKey(url)) // không overwrite
                {
                    routeMap[url] = (handlerType, defaultRole);
                }
            }
        }

        /// <summary>
        /// Fallback: tìm handler theo prefix nếu chưa có trong map
        /// </summary>
        private static (Type Handler, UserRole MinRole)? ResolveHandler(string url)
        {
            if (routeMap.TryGetValue(url.ToLower(), out var entry))
                return entry;

            var handlers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(HandlerBase).IsAssignableFrom(t));

            foreach (var handlerType in handlers)
            {
                var inst = (HandlerBase)Activator.CreateInstance(handlerType)!;
                if (url.StartsWith(inst.Type, StringComparison.OrdinalIgnoreCase))
                {
                    var newEntry = (handlerType, UserRole.User);
                    routeMap[inst.Type.ToLower()] = newEntry;
                    return newEntry;
                }
            }
            return null;
        }

        /// <summary>
        /// Xử lý request: map url → handler → ApiEvent
        /// </summary>
        public static void Handle(HttpRequest request, HttpsSession session)
        {
            var entry = ResolveHandler(request.Url);
            if (entry == null)
                return;

            var handlerType = entry.Value.Handler;
            var eventType = typeof(EventBase<>).MakeGenericType(handlerType);
            var genericMethod = scheduleGeneric.MakeGenericMethod(eventType);
            var result = genericMethod.Invoke(null, new object[] { 0.25f });

            if (result is not IApiEvent ev)
                throw new InvalidOperationException($"Không thể tạo ApiEvent cho {handlerType.Name}");

            ev.request = request;
            ev.session = session;
        }
    }
}
