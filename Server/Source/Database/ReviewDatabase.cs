using LuciferCore.Database;

namespace Server.Source.Database
{
    public class ReviewDatabase: BaseDatabase
    {
        public virtual int CreateReview(object data)
        {
            string sql = $"Placeholder";
            var result = Create(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }
        public virtual List<Dictionary<string, object>> GetReview(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetReviewByUser(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetReviewByGame(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual int SetReview(object data)
        {
            string sql = $"Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual int DeleteReview(object data)
        {
            string sql = $"Placeholder";
            var result = Delete(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }
    }
}
