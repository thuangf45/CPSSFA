using LuciferCore.Attributes;
using LuciferCore.Database;
using LuciferCore.Helper;
using LuciferCore.NetCoreServer;
using LuciferCore.Handler;
using Server.Source.Command.GameCommand;

namespace Server.Source.Handler
{
    internal class APIGameHandler : HandlerBase
    {
        public override string Type => "/api/game";

        [HttpGet("")]
        protected override void GetHandle(HttpRequest request, HttpsSession session)
        {
            var gameId = DecodeHelper.GetParamWithURL("gameId", request.Url);

            if (string.IsNullOrEmpty(gameId))
            {
                ErrorHandle(session, "Không tìm thấy thông tin game!");
                return;
            }

            var gameInfo = new CommandGetGame(gameId).Handle();
            OkHandle(session, gameInfo);
        }

        [HttpGet("/pagination")]
        protected void GetGamePaginateHandle(HttpRequest request, HttpsSession session)
        {
            var page = DecodeHelper.GetParamWithURL("page", request.Url);
            var limit = DecodeHelper.GetParamWithURL("limit", request.Url);

            var cmd = new CommandGetGamePagination(DataMapper.GetScalarValue<int>(page), DataMapper.GetScalarValue<int>(limit));
            var games = cmd.Handle();

            OkHandle(session, games);
        }

        [HttpGet("/user")]
        protected void GetGameByUserHandle(HttpRequest request, HttpsSession session)
        {
            var userId = DecodeHelper.GetUserIdFromRequest(request);

            var cmd = new CommandGetGameByUser(userId, 1, 100);

            var games = cmd.Handle();

            OkHandle(session, games);
        }

    }
}
