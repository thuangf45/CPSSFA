using LuciferCore.Core;
using LuciferCore.Helper;
using LuciferCore.NetCoreServer;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý các phiên người dùng (session) với cơ chế lưu trữ an toàn đa luồng và tự động dọn dẹp các phiên hết hạn.
    /// </summary>
    public class SessionManager
    {
        /// <summary>
        /// Khóa đọc-ghi an toàn đa luồng để bảo vệ truy cập vào danh sách phiên.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new();

        /// <summary>
        /// Danh sách các phiên được lưu trữ, với khóa là ID phiên và giá trị là thông tin phiên.
        /// </summary>
        private readonly Dictionary<string, SessionEntry> _sessions = new();

        /// <summary>
        /// Số lượng phiên hiện tại trong hệ thống.
        /// </summary>
        public int NumberSession => _sessions.Count;

        /// <summary>
        /// Bộ điều khiển tín hiệu hủy để dừng tác vụ nền một cách an toàn.
        /// </summary>
        private CancellationTokenSource _cts = new();

        /// <summary>
        /// Tác vụ nền để dọn dẹp các phiên hết hạn.
        /// </summary>
        private Task? _cleanerTask;

        /// <summary>
        /// Lớp nội bộ lưu trữ thông tin phiên người dùng.
        /// </summary>
        private class SessionEntry
        {
            /// <summary>
            /// ID của người dùng liên kết với phiên.
            /// </summary>
            public string UserId;

            /// <summary>
            /// Thời điểm phiên hết hạn.
            /// </summary>
            public DateTime ExpireAt;

            /// <summary>
            /// Kiểm tra xem phiên đã hết hạn hay chưa.
            /// </summary>
            public bool IsExpired => DateTime.UtcNow > ExpireAt;
        }

        /// <summary>
        /// Lưu trữ một phiên mới với ID phiên, ID người dùng và thời gian sống (TTL).
        /// </summary>
        /// <param name="sessionId">ID của phiên.</param>
        /// <param name="userId">ID của người dùng liên kết với phiên.</param>
        /// <param name="ttl">Thời gian sống của phiên (mặc định là 1 giờ).</param>
        public void Store(string sessionId, string userId, TimeSpan? ttl = null)
        {
            ttl ??= TimeSpan.FromHours(1);
            using (new WriteLock(_lock))
            {
                _sessions[sessionId] = new SessionEntry
                {
                    UserId = userId,
                    ExpireAt = DateTime.UtcNow + ttl.Value,
                };
            }
        }

        /// <summary>
        /// Lấy ID người dùng từ ID phiên, nếu phiên hợp lệ.
        /// </summary>
        /// <param name="sessionId">ID của phiên cần kiểm tra.</param>
        /// <returns>ID người dùng nếu phiên hợp lệ, ngược lại trả về <c>null</c>.</returns>
        public string? GetUserId(string sessionId)
        {
            using (new ReadLock(_lock))
            {
                if (_sessions.TryGetValue(sessionId, out var entry))
                {
                    if (!entry.IsExpired)
                    {
                        return entry.UserId;
                    }
                }
            }

            RemoveSession(sessionId); // Xóa nếu hết hạn
            return null;
        }

        /// <summary>
        /// Kiểm tra xem một phiên có hợp lệ và liên kết với người dùng hay không.
        /// </summary>
        /// <param name="sessionId">ID của phiên cần kiểm tra.</param>
        /// <returns>Trả về <c>true</c> nếu phiên hợp lệ, ngược lại trả về <c>false</c>.</returns>
        public bool IsUser(string sessionId)
        {
            using (new ReadLock(_lock))
            {
                if (_sessions.TryGetValue(sessionId, out var entry))
                {
                    if (!entry.IsExpired)
                    {
                        return true;
                    }
                }
            }

            RemoveSession(sessionId); // Xóa nếu hết hạn
            return false;
        }

        /// <summary>
        /// Xác thực yêu cầu HTTP dựa trên mã thông báo và lấy ID người dùng.
        /// </summary>
        /// <param name="request">Yêu cầu HTTP chứa mã thông báo.</param>
        /// <param name="userId">ID người dùng được trích xuất (nếu xác thực thành công).</param>
        /// <param name="session">Phiên HTTPS để xóa mã thông báo nếu xác thực thất bại (tùy chọn).</param>
        /// <returns>Trả về <c>true</c> nếu xác thực thành công, ngược lại trả về <c>false</c>.</returns>
        public bool Authorization(HttpRequest request, out string userId, HttpsSession session = null)
        {
            userId = "";
            string token = TokenHelper.GetToken(request);
            if (TokenHelper.TryParseToken(token, out var sessionId))
            {
                string? id = GetUserId(sessionId);
                if (id != null)
                {
                    userId = id;
                    return true;
                }
                else if (session != null)
                {
                    TokenHelper.RemoveToken(session.Response);
                }
            }
            return false;
        }

        /// <summary>
        /// Xóa phiên hiện tại dựa trên mã thông báo trong yêu cầu HTTP.
        /// </summary>
        /// <param name="request">Yêu cầu HTTP chứa mã thông báo.</param>
        /// <param name="session">Phiên HTTPS để xóa mã thông báo (tùy chọn).</param>
        /// <returns>Trả về <c>true</c> nếu xóa thành công, ngược lại trả về <c>false</c>.</returns>
        public bool RemoveCurrentSession(HttpRequest request, HttpsSession session = null)
        {
            string token = TokenHelper.GetToken(request);

            if (TokenHelper.TryParseToken(token, out var sessionId))
            {
                if (sessionId != null)
                {
                    RemoveSession(sessionId);
                }
                TokenHelper.RemoveToken(session.Response);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Xóa một phiên khỏi danh sách mà không cần khóa.
        /// </summary>
        /// <param name="sessionId">ID của phiên cần xóa.</param>
        private void RemoveSessionInternal(string sessionId)
        {
            _sessions.Remove(sessionId);
        }

        /// <summary>
        /// Xóa một phiên khỏi danh sách với khóa an toàn đa luồng.
        /// </summary>
        /// <param name="sessionId">ID của phiên cần xóa.</param>
        private void RemoveSession(string sessionId)
        {
            using (new WriteLock(_lock))
            {
                RemoveSessionInternal(sessionId);
            }
        }

        /// <summary>
        /// Xóa tất cả các phiên của một người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng có các phiên cần xóa.</param>
        public void RemoveAllSessionOfUser(string userId)
        {
            using (new WriteLock(_lock))
            {
                var sessionIds = _sessions
                    .Where(kv => kv.Value.UserId == userId)
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var sessionId in sessionIds)
                {
                    RemoveSessionInternal(sessionId); // Không khóa lại
                }
            }
        }

        /// <summary>
        /// Xóa tất cả các phiên của một người dùng, ngoại trừ một phiên được chỉ định.
        /// </summary>
        /// <param name="userId">ID của người dùng có các phiên cần xóa.</param>
        /// <param name="sessionIdToKeep">ID của phiên được giữ lại.</param>
        public void RemoveAllSessionsExcept(string userId, string sessionIdToKeep)
        {
            using (new WriteLock(_lock))
            {
                var sessionIds = _sessions
                    .Where(kv => kv.Value.UserId == userId && kv.Key != sessionIdToKeep)
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var sessionId in sessionIds)
                {
                    RemoveSessionInternal(sessionId);
                }
            }
        }

        /// <summary>
        /// Xóa toàn bộ các phiên trong danh sách.
        /// </summary>
        public void Clear()
        {
            using (new WriteLock(_lock)) _sessions.Clear();
        }

        /// <summary>
        /// Dọn dẹp các phiên đã hết hạn trong danh sách.
        /// </summary>
        public void CleanExpiredSessions()
        {
            using (new WriteLock(_lock))
            {
                var expired = _sessions
                    .Where(kv => kv.Value.ExpireAt <= DateTime.UtcNow)
                    .Select(kv => kv.Key)
                    .ToList(); // Tránh sửa collection khi đang duyệt

                foreach (var sessionId in expired)
                    _sessions.Remove(sessionId);
            }
        }

        /// <summary>
        /// Khởi động tác vụ nền để dọn dẹp các phiên hết hạn định kỳ.
        /// </summary>
        /// <remarks>
        /// Nếu tác vụ đã chạy, phương thức sẽ bỏ qua. Tác vụ dọn dẹp chạy mỗi 600 giây (10 phút).
        /// </remarks>
        public void Start()
        {
            if (_cleanerTask != null && !_cleanerTask.IsCompleted)
                return;

            if (_cts.IsCancellationRequested)
                _cts = new CancellationTokenSource();

            _cleanerTask = Task.Run(() => Run(_cts.Token));
        }

        /// <summary>
        /// Dừng tác vụ nền dọn dẹp các phiên và ghi log thông báo.
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();

            try
            {
                _cleanerTask?.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => e is OperationCanceledException);
            }
            Simulation.GetModel<LogManager>().Log("SessionManager stopped.", LogLevel.INFO, LogSource.SYSTEM);
        }

        /// <summary>
        /// Vòng lặp nền để dọn dẹp các phiên hết hạn định kỳ.
        /// </summary>
        /// <param name="token">Mã hủy để dừng tác vụ một cách an toàn.</param>
        /// <returns>Tác vụ bất đồng bộ xử lý dọn dẹp.</returns>
        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    CleanExpiredSessions();
                    await Task.Delay(600_000, token); // Dọn mỗi 600 giây
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>()?.Log(ex); // Ghi log lỗi nếu cần
                    await Task.Delay(1000, token);
                }
            }
        }
    }
}