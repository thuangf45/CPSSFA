using LuciferCore.Interface;
using Server.Source.Database;
using static LuciferCore.Core.Simulation;

namespace Server.Source.Command.GameCommand
{
    public record CommandGetGame(string gameId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<GameDatabase>().GetGame(this);
    }

    public record CommandGetGamePagination(int page, int limit) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<GameDatabase>().GetGamePagination(this);
    }

    public record CommandGetGameByUser(string userId, int page, int limit) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<GameDatabase>().GetGameByUser(this);
    }

}
