using LuciferCore.Database;

namespace Server.Source.Database
{
    public class CommentDatabase: BaseDatabase
    {
        public virtual int CreateComment(object data)
        {
            string sql = $"Placeholder";
            var result = Create(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }
        public virtual List<Dictionary<string, object>> GetComment(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetCommentByUser(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetCommentByReview(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;

        }

        public virtual int SetComment(object data)
        {
            string sql = $"Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual int DeleteComment(object data)
        {
            string sql = $"Placeholder";
            var result = Delete(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }
    }
}
