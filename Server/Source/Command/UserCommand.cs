using LuciferCore.Interface;
using Server.Source.Database;
using static LuciferCore.Core.Simulation;

namespace Server.Source.Command.UserCommand
{
    public record CommandGetUser(string userId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<UserDatabase>().GetUser(this);
    }
    public record CommandGetUserIdByUsername(string username) : ICommand<string>
    {
        public string Handle() => GetModel<UserDatabase>().GetUserIdByUsername(this);
    }

    public record CommandChangeEmail(string userId, string email) : ICommand<int>
    {
        public int Handle() => GetModel<UserDatabase>().ChangeEmail(this);
    }

    public record CommandChangeAvatar(string userId, string avatar) : ICommand<int>
    {
        public int Handle() => GetModel<UserDatabase>().ChangeAvatar(this);
    }

    public record CommandFollow(string userId, string targetId) : ICommand<int>
    {
        public int Handle() => GetModel<UserDatabase>().Follow(this);
    }

    public record CommandUnFollow(string userId, string targetId) : ICommand<int>
    {
        public int Handle() => GetModel<UserDatabase>().UnFollow(this);
    }

    public record CommandGetUserFollower(string userId, int page, int limit) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<UserDatabase>().GetUserFollower(this);
    }

    public record CommandGetUserFollowing(string userId, int page, int limit) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<UserDatabase>().GetUserFollowing(this);
    }

    public record CommandDeleteUser(string userId, string password) : ICommand<int>
    {
        public int Handle() => GetModel<UserDatabase>().DeleteUser(this);
    }
}
