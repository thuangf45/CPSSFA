using LuciferCore.Attributes;
using LuciferCore.Core;
using LuciferCore.Handler;
using LuciferCore.Helper;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;
using Microsoft.IdentityModel.Tokens;
using Server.Source.Command.AccountCommand;
using static LuciferCore.Manager.SessionManager;


namespace Server.Source.Handler
{
    internal class APIAuthHandler : HandlerBase
    {
        public override string Type => "/api/auth";

        [HttpPost("/login")]
        protected void PostLogin(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();

            var value = request.Body;
            var cmd = JsonHelper.Deserialize<CommandLoginAccount>(value);
            if (cmd == null)
            {
                ErrorHandle(session, "Tài khoản hoặc mật khẩu không có giá trị!");
                return;
            }
            // Đăng nhập thành công:
            string userId = cmd.Handle();

            if (!userId.IsNullOrEmpty())
            {
                var response = ResponseHelper.NewUserSession(userId, UserRole.User, session.Response);

                session.SendResponseAsync(response); // gửi response
                return;
            }

            ErrorHandle(session, "Tài khoản không hợp lệ hoặc đã được dùng!");

        }
        [HttpPost("/logout")]
        protected void PostLogout(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();

            if (sessionManager.RemoveCurrentSession(request, session))
                OkHandle(session);
            else
                ErrorHandle(session, "Có lỗi khi đăng xuất, hãy thử lại!");
        }

        /// <summary>Thực hiện quên mật khẩu cho người dùng.</summary>
        [HttpPost("/forgetpassword")]
        protected void PostForgetPassword(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();

            var cmd = JsonHelper.Deserialize<CommandForgetPassword>(request.Body);
            
            if (cmd == null)
            {
                ErrorHandle(session, "Email không có giá trị");
                return;
            }
            var newPassword = cmd.Handle();

            if (newPassword != null)
            {
                Simulation.GetModel<NotifyManager>().SendMailResetPassword(cmd.email, newPassword);
                OkHandle(session, "Mật khẩu của bạn đã reset, hãy check mail của bạn!");
                return;
            }

            ErrorHandle(session, "Reset mật khẩu không thành công!");
        }

    }
}
