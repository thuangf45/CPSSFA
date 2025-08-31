using LuciferCore.Interface;
using Server.Source.Database;
using static LuciferCore.Core.Simulation;
namespace Server.Source.Command.AccountCommand
{
    public record CommandCreateAccount(string username, string password, string email) : ICommand<string>
    {
        public string Handle() => GetModel<AccountDatabase>().CreateAccount(this);
    }
    public record CommandLoginAccount(string username, string password) : ICommand<string>
    {
        public string Handle() => GetModel<AccountDatabase>().CheckLoginAccount(this);
    }
    public record CommandChangeUsername(string userId, string username = null) : ICommand<int>
    {
        public int Handle() => GetModel<AccountDatabase>().ChangeUsername(this);
    }
    public record CommandChangePassword(string userId, string oldPassword, string newPassword) : ICommand<int>
    {
        public int Handle() => GetModel<AccountDatabase>().ChangePassword(this);
    }
    public record CommandForgetPassword(string email) : ICommand<string>
    {
        public string Handle() => GetModel<AccountDatabase>().ForgetPassword(this);
    }
}


