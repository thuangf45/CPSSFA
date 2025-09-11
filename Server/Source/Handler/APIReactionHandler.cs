using LuciferCore.Attributes;
using LuciferCore.Core;
using LuciferCore.Helper;
using LuciferCore.Manager;
using LuciferCore.Handler;
using LuciferCore.NetCoreServer;
using Server.Source.Command.ReactionCommand;

namespace Server.Source.Handler
{
    internal class APIReactionHandler : HandlerBase
    {
        public override string Type => "/api/reaction";

        [HttpGet("/user")]
        protected void GetReactionByUserHandle(HttpRequest request, HttpsSession session)
        {
            var userId = DecodeHelper.GetUserIdFromRequest(request);

            if (string.IsNullOrEmpty(userId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin!");
                return;
            }

            var cmd = new CommandGetReactionByUser(userId);
            var reactionInfo = cmd.Handle();
            OkHandle(session, reactionInfo);
        }

        // GET reactions by comment
        [HttpGet("/comment")]
        protected void GetReactionByCommentHandle(HttpRequest request, HttpsSession session)
        {
            var commentId = DecodeHelper.GetParamWithURL("commentId", request.Url);
            if (string.IsNullOrEmpty(commentId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin comment!");
                return;
            }

            var cmd = new CommandGetReactionByComment(commentId);
            var reactions = cmd.Handle();
            OkHandle(session, reactions);
        }

        // GET reactions by review
        [HttpGet("/review")]
        protected void GetReactionByReviewHandle(HttpRequest request, HttpsSession session)
        {
            var reviewId = DecodeHelper.GetParamWithURL("reviewId", request.Url);
            if (string.IsNullOrEmpty(reviewId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin review!");
                return;
            }

            var cmd = new CommandGetReactionByReview(reviewId);
            var reactions = cmd.Handle();
            OkHandle(session, reactions);
        }

        // POST reaction for comment
        [HttpPost("/comment")]
        protected void PostReactionForCommentHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandCreateReactionForComment>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Tạo reaction không thành công");
                return;
            }

            OkHandle(session, "Tạo reaction thành công");
        }

        // POST reaction for review
        [HttpPost("/review")]
        protected void PostReactionForReviewHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandCreateReactionForReview>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Tạo reaction không thành công");
                return;
            }

            OkHandle(session, "Tạo reaction thành công");
        }

        // PUT reaction for comment
        [HttpPut("/comment")]
        protected void PutReactionForCommentHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd =JsonHelper.AddPropertyAndDeserialize<CommandSetReactionComment>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Cập nhật reaction không thành công");
                return;
            }

            OkHandle(session, "Cập nhật reaction  thành công");
        }

        // PUT reaction for review
        [HttpPut("/review")]
        protected void PutReactionForReviewHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandSetReactionReview>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Cập nhật reaction không thành công");
                return;
            }

            OkHandle(session, "Cập nhật reaction  thành công");
        }



        // DELETE reaction by comment
        [HttpDelete("/comment")]
        protected void DeleteReactionForCommentHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandDeleteReactionComment>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Xoá reaction không thành công");
                return;
            }

            OkHandle(session, "Đã xoá reaction thành công!");
        }

        // DELETE reaction by review
        [HttpDelete("/review")]
        protected void DeleteReactionForReviewHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandDeleteReactionReview>(request.Body, "userId", userId);


            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Xoá reaction không thành công");
                return;
            }

            OkHandle(session, "Đã xoá reaction thành công!");
        }
    }
}
