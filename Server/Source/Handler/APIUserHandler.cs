using LuciferCore.Core;
using LuciferCore.Database;
using LuciferCore.Helper;
using LuciferCore.Manager;
using LuciferCore.Handler;
using LuciferCore.NetCoreServer;
using Microsoft.IdentityModel.Tokens;
using Server.Source.Command.UserCommand;
using Server.Source.Command.AccountCommand;
using LuciferCore.Attributes;

namespace Server.Source.Handler
{
    /// <summary>
    /// Xử lý các API liên quan đến người dùng, như đăng ký, đăng nhập, cập nhật email, avatar, username, v.v.
    /// </summary>
    internal class APIUserHandler : HandlerBase
    {
        /// <summary>
        /// Kiểu đường dẫn chính của handler. Dùng để xác định handler phù hợp khi routing.
        /// </summary>
        public override string Type => "/api/user";

        /// <summary>
        /// Xử lý yêu cầu GET để lấy thông tin người dùng theo userId hoặc token.
        /// </summary>
        [HttpGet("")]
        protected override void GetHandle(HttpRequest request, HttpsSession session)
        {
            var userId = ResolveUserId(request);
            if (string.IsNullOrEmpty(userId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin user!");
                return;
            }
            var cmd = new CommandGetUser(userId);
            var userInfo = cmd.Handle();
            OkHandle(session, userInfo);
        }
        private string ResolveUserId(HttpRequest request)
        {
            var userId = DecodeHelper.GetParamWithURL("userId", request.Url);
            if (!string.IsNullOrEmpty(userId)) return userId;

            var username = DecodeHelper.GetParamWithURL("username", request.Url);
            if (!string.IsNullOrEmpty(username))
            {
                var cmd = new CommandGetUserIdByUsername(username);
                return cmd.Handle();
            }

            return Simulation.GetModel<SessionManager>().GetUserIdFromRequest(request);
        }

        [HttpGet("/follower")]
        protected void GetFollowerHandle(HttpRequest request, HttpsSession session)
        {
            var page = DecodeHelper.GetParamWithURL("page", request.Url);
            var limit = DecodeHelper.GetParamWithURL("limit", request.Url);
            var userId = DecodeHelper.GetUserIdFromRequest(request)
                         ?? Simulation.GetModel<SessionManager>().GetUserIdFromRequest(request);

            if (string.IsNullOrEmpty(userId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin user!");
                return;
            }

            var cmd = new CommandGetUserFollower(userId, DataMapper.GetScalarValue<int>(page), DataMapper.GetScalarValue<int>(limit));
            var userFollower = cmd.Handle();
            OkHandle(session, userFollower);
        }

        [HttpGet("/following")]
        protected void GetFollowingHandle(HttpRequest request, HttpsSession session)
        {
            var page = DecodeHelper.GetParamWithURL("page", request.Url);
            var limit = DecodeHelper.GetParamWithURL("limit", request.Url);
            var userId = DecodeHelper.GetUserIdFromRequest(request)
                         ?? Simulation.GetModel<SessionManager>().GetUserIdFromRequest(request);

            if (string.IsNullOrEmpty(userId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin user!");
                return;
            }

            var cmd = new CommandGetUserFollowing(userId, DataMapper.GetScalarValue<int>(page), DataMapper.GetScalarValue<int>(limit));
            var userFollowing = cmd.Handle();
            OkHandle(session, userFollowing);
        }

        /// <summary>
        /// Xử lý POST cho việc đăng ký tài khoản người dùng.
        /// </summary>
        [HttpPost("")]
        protected override void PostHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();

            var cmd = JsonHelper.Deserialize<CommandCreateAccount>(request.Body);

            if (cmd == null)
            {
                ErrorHandle(session, "Thông tin đăng ký không có giá trị hoặc được sử dụng bởi người khác");
                return;
            }

            string userId = cmd.Handle();
            if (!userId.IsNullOrEmpty())
            {
                var response = ResponseHelper.NewUserSession(userId, SessionManager.UserRole.User, session.Response);
                session.SendResponseAsync(response);
                return;
            }

            ErrorHandle(session, "Đăng ký không thành công");

        }

        [HttpPost("/follow")]
        protected void PostFollowHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandFollow>(request.Body, "userId", userId);

            // Validate input
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.targetId) || cmd.targetId == userId)
            {
                ErrorHandle(session, "Dữ liệu follow không hợp lệ");
                return;
            }

            // DB insert
            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Follow không thành công");
                return;
            }

            OkHandle(session, "Follow thành công");
        }


        /// <summary>Cập nhật email người dùng.</summary>
        [HttpPut("/email")]
        protected void PutUserEmail(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandChangeEmail>(request.Body, "userId", userId);

            if (cmd == null || cmd.Handle() != 1)
            {
                ErrorHandle(session, "Đổi email không thành công");
                return;
            }
            OkHandle(session, "Đổi email thành công");
        }

        /// <summary>Cập nhật avatar người dùng.</summary>
        [HttpPut("/avatar")]
        protected void PutUserAvatar(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandChangeAvatar>(request.Body, "userId", userId);
            
            if (cmd == null || cmd.Handle() != 1)
            {
                ErrorHandle(session, "Đổi avatar không thành công!");
                return;
            }

            OkHandle(session, "Đổi avatar thành công!");
        }

        /// <summary>Cập nhật username người dùng.</summary>
        [HttpPut("/username")]
        protected void PutUserUsername(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandChangeUsername>(request.Body, "userId", userId);

            if (cmd == null ||  cmd.Handle() != 1)
            {
                ErrorHandle(session, "Đổi username không thành công!");
                return;
            }
            OkHandle(session, "Đổi username thành công");
        }

        /// <summary>Cập nhật password người dùng.</summary>
        [HttpPut("/password")]
        protected void PutUserPassword(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandChangePassword>(request.Body, "userId", userId);


            if (cmd == null || cmd.Handle() != 1)
            {
                ErrorHandle(session, "Đổi mật khẩu không thành công!");
                return;
            }
            OkHandle(session, "Đổi mật khẩu thành công!");
        }

        /// <summary>
        /// Xử lý xóa tài khoản người dùng.
        /// </summary>
        [HttpDelete("")]
        protected override void DeleteHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandDeleteUser>(request.Body, "userId", userId);

            if (cmd == null || cmd.Handle() != 1)
            {
                ErrorHandle(session, "Xoá tài khoản không thành công!");
                return;
            }

            OkHandle(session, "Đã xoá tài khoản thành công!");
        }

        [HttpDelete("/follow")]
        protected void DeleteFollowHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandUnFollow>(request.Body, "userId", userId);

            // Validate input
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.targetId) || cmd.targetId == userId)
            {
                ErrorHandle(session, "Dữ liệu unfollow không hợp lệ");
                return;
            }

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "UnFollow không thành công");
                return;
            }

            OkHandle(session, "UnFollow thành công");
        }

    }
}

