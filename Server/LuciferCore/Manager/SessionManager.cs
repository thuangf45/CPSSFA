using LuciferCore.Core;
using LuciferCore.Helper;
using LuciferCore.NetCoreServer;
using static LuciferCore.Core.Simulation;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý các phiên người dùng (session) với cơ chế lưu trữ an toàn đa luồng và tự động dọn dẹp các phiên hết hạn.
    /// </summary>
    public class SessionManager : ManagerBase
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
        /// Lớp nội bộ lưu trữ thông tin phiên người dùng.
        /// </summary>
        
        public enum UserRole
        {
            Guest = 0,
            User = 1,
            Admin = 2
        }

        private class SessionEntry
        {
            /// <summary>
            /// ID của người dùng liên kết với phiên.
            /// </summary>
            public string UserId;

            public UserRole Role;

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
        public void Store(string sessionId, string userId, UserRole role, TimeSpan? ttl = null)
        {
            ttl ??= TimeSpan.FromHours(1);
            using (new WriteLock(_lock))
            {
                _sessions[sessionId] = new SessionEntry
                {
                    UserId = userId,
                    Role = role,
                    ExpireAt = DateTime.UtcNow + ttl.Value
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
        /// Lấy ID người dùng từ HttpRequest dựa trên token, nếu hợp lệ.
        /// </summary>
        /// <param name="request">Yêu cầu HTTP chứa token.</param>
        /// <returns>ID người dùng nếu hợp lệ, ngược lại trả về null.</returns>
        public string? GetUserIdFromRequest(HttpRequest request)
        {
            string token = TokenHelper.GetToken(request);

            if (TokenHelper.TryParseToken(token, out var sessionId))
            {
                return GetUserId(sessionId); // Dùng lại method có sẵn
            }

            return null;
        }

        /// <summary>
        /// Kiểm tra role người dùng từ HttpRequest.
        /// </summary>
        public UserRole GetRoleFromRequest(HttpRequest request)
        {
            string token = TokenHelper.GetToken(request);

            if (TokenHelper.TryParseToken(token, out var sessionId))
            {
                using (new ReadLock(_lock))
                {
                    if (_sessions.TryGetValue(sessionId, out var entry) && !entry.IsExpired)
                    {
                        return entry.Role;
                    }
                }

                RemoveSession(sessionId); // hết hạn
            }

            return UserRole.Guest;
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
        /// Xác thực yêu cầu HTTP dựa trên mã thông báo và lấy userId + role.
        /// </summary>
        /// <param name="request">Yêu cầu HTTP chứa mã thông báo.</param>
        /// <param name="userId">ID người dùng được trích xuất (nếu xác thực thành công).</param>
        /// <param name="session">Phiên HTTPS để xóa mã thông báo nếu xác thực thất bại (tùy chọn).</param>
        /// <returns>Trả về <c>true</c> nếu xác thực thành công, ngược lại trả về <c>false</c>.</returns>
        public bool Authorization(HttpRequest request, out string userId, out UserRole role, HttpsSession session = null)
        {
            userId = "guest";
            role = UserRole.Guest;

            string token = TokenHelper.GetToken(request);
            if (TokenHelper.TryParseToken(token, out var sessionId))
            {
                using (new ReadLock(_lock))
                {
                    if (_sessions.TryGetValue(sessionId, out var entry) && !entry.IsExpired)
                    {
                        userId = entry.UserId;
                        role = entry.Role;
                        return true;
                    }
                }

                RemoveSession(sessionId); // hết hạn
                if (session != null)
                    TokenHelper.RemoveToken(session.Response);
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
        /// Vòng lặp nền để dọn dẹp các phiên hết hạn định kỳ.
        /// </summary>
        /// <param name="token">Mã hủy để dừng tác vụ một cách an toàn.</param>
        /// <returns>Tác vụ bất đồng bộ xử lý dọn dẹp.</returns>
        protected override async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    CleanExpiredSessions();
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
                await Task.Delay(600_000, token); // Dọn mỗi 600 giây
            }
        }

        protected override void OnStarted()
        {
            GetModel<LogManager>().LogSystem("⚙️ SessionManager started");
        }

        protected override void OnStopped()
        {
            GetModel<LogManager>().LogSystem("⚙️ SessionManager stopped");
        }
    }
}