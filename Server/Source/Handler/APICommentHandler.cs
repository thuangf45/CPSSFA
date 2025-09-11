using LuciferCore.Attributes;
using LuciferCore.Core;
using LuciferCore.Handler;
using LuciferCore.Helper;
using LuciferCore.Manager;
using LuciferCore.NetCoreServer;
using Server.Source.Command.CommentCommand;

namespace Server.Source.Handler
{
    internal class APICommentHandler : HandlerBase
    {
        public override string Type => "/api/comment";

        [HttpGet("")]
        protected override void GetHandle(HttpRequest request, HttpsSession session)
        {
            var commentId = DecodeHelper.GetParamWithURL("commentId", request.Url);

            if (string.IsNullOrEmpty(commentId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin comment!");
                return;
            }

            var cmd = new CommandGetComment(commentId);
            var commentInfo = cmd.Handle();
            OkHandle(session, commentInfo);
        }

        [HttpGet("/review")]
        protected void GetCommentByReviewHandle(HttpRequest request, HttpsSession session)
        {
            var reviewId = DecodeHelper.GetParamWithURL("reviewId", request.Url);
            if (string.IsNullOrEmpty(reviewId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin!");
                return;
            }

            var cmd = new CommandGetCommentByReview(reviewId);
            var commentList = cmd.Handle();
            OkHandle(session, commentList);
        }

        [HttpGet("/user")]
        protected void GetCommentByUserHandle(HttpRequest request, HttpsSession session)
        {
            var userId = DecodeHelper.GetUserIdFromRequest(request);
            if (string.IsNullOrEmpty(userId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin!");
                return;
            }

            var cmd = new CommandGetCommentByUser(userId);
            var commentList = cmd.Handle();
            OkHandle(session, commentList);
        }
        [HttpPost("")]
        protected override void PostHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandCreateComment>(
                request.Body, "userId", userId
            );

            if (cmd == null)
            {
                ErrorHandle(session, "Dữ liệu comment không hợp lệ");
                return;
            }

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Tạo comment không thành công");
                return;
            }

            OkHandle(session, "Tạo comment thành công");
        }

        [HttpPut("")]
        protected override void PutHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandSetComment>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Cập nhật comment không thành công");
                return;
            }

            OkHandle(session, "Cập nhật comment thành công");
        }

        [HttpDelete("")]
        protected override void DeleteHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandDeleteComment>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Xoá comment không thành công!");
                return;
            }

            OkHandle(session, "Đã xoá comment thành công!");
        }
    }
}
