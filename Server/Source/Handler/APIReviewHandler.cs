using LuciferCore.Core;
using LuciferCore.Database;
using LuciferCore.Helper;
using LuciferCore.Manager;
using LuciferCore.Handler;
using LuciferCore.NetCoreServer;
using Server.Source.Command.ReviewCommand;
using LuciferCore.Attributes;

namespace Server.Source.Handler
{
    internal class APIReviewHandler : HandlerBase
    {
        public override string Type => "/api/review";

        [HttpGet("")]
        protected override void GetHandle(HttpRequest request, HttpsSession session)
        {
            var reviewId = DecodeHelper.GetParamWithURL("reviewId", request.Url);

            if (string.IsNullOrEmpty(reviewId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin review!");
                return;
            }

            var cmd = new CommandGetReview(reviewId);
            var reviewInfo = cmd.Handle();
            OkHandle(session, reviewInfo);
        }

        [HttpGet("/game")]
        protected void GetReviewByGameHandle(HttpRequest request, HttpsSession session)
        {
            var gameId = DecodeHelper.GetParamWithURL("gameId", request.Url);
            if (string.IsNullOrEmpty(gameId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin!");
                return;
            }

            var cmd = new CommandGetReviewByGame(gameId);
            var reviews = cmd.Handle();
            OkHandle(session, reviews);

        }

        [HttpGet("/user")]
        protected void GetReviewByUserHandle(HttpRequest request, HttpsSession session)
        {
            var userId = DecodeHelper.GetUserIdFromRequest(request);

            if (string.IsNullOrEmpty(userId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin!");
                return;
            }

            var cmd = new CommandGetReviewByUser(userId);
            var reviews = cmd.Handle();
            OkHandle(session, reviews);
        }

        [HttpPost("")]
        protected override void PostHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandCreateReview>(request.Body, "userId", userId);

            // Validate input
            if (cmd == null)
            {
                ErrorHandle(session, "Dữ liệu follow không hợp lệ");
                return;
            }

            // DB insert
            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Tạo review không thành công");
                return;
            }

            OkHandle(session, "Tạo review thành công");
        }

        [HttpPut("")]
        protected override void PutHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandSetReview>(request.Body, "userId", userId);

            // DB update
            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Cập nhật review không thành công");
                return;
            }

            OkHandle(session, "Cập nhật review thành công");

        }

        [HttpDelete("")]
        protected override void DeleteHandle(HttpRequest request, HttpsSession session)
        {
            var sessionManager = Simulation.GetModel<SessionManager>();
            var userId = sessionManager.GetUserIdFromRequest(request);

            var cmd = JsonHelper.AddPropertyAndDeserialize<CommandDeleteReview>(request.Body, "userId", userId);

            if (cmd.Handle() < 1)
            {
                ErrorHandle(session, "Xoá review không thành công!");
                return;
            }

            OkHandle(session, "Đã xoá review thành công!");
        }

    }
}
