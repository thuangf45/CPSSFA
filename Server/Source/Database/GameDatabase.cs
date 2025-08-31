using LuciferCore.Database;

namespace Server.Source.Database
{
    public class GameDatabase: BaseDatabase
    {
        public virtual List<Dictionary<string, object>> GetGame(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }
        public virtual List<Dictionary<string, object>> GetGamePagination(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetGameByUser(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }
    }
}
