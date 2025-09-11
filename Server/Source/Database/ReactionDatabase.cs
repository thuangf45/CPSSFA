using LuciferCore.Database;

namespace Server.Source.Database
{
    public class ReactionDatabase: BaseDatabase
    {
        public virtual int CreateReactionForComment(object data)
        {
            string sql = $"Placeholder";
            var result = Create(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual int CreateReactionForReview(object data)
        {
            string sql = $"Placeholder";
            var result = Create(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual int DeleteReactionComment(object data)
        {
            string sql = $"Placeholder";
            var result = Delete(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual int DeleteReactionReview(object data)
        {
            string sql = $"Placeholder";
            var result = Delete(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual int SetReactionComment(object data)
        {
            string sql = $"Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual List<Dictionary<string, object>> GetReactionByComment(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetReactionByReview(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetReactionByUser(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual int SetReactionReview(object data)
        {
            string sql = $"Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

    }
}
