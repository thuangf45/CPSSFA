using LuciferCore.Database;
namespace Server.Source.Database
{
    /// <summary>
    /// Lớp quản lý dữ liệu bảng <c>user</c> trong cơ sở dữ liệu.
    /// Cung cấp các chức năng cập nhật email và avatar của người dùng.
    /// </summary>
    public class UserDatabase : BaseDatabase
    {
        public virtual List<Dictionary<string, object>> GetUser(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual string GetUserIdByUsername(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return DataMapper.GetScalarValue<string>(result);
        }

        /// <summary>
        /// Thay đổi email người dùng.
        /// </summary>
        /// <param name="data">Thông tin cần thiết để thay đổi email (kiểu ParamsChangeEmail).</param>
        /// <returns>1 nếu thành công, 0 nếu thất bại.</returns>
        public virtual int ChangeEmail(object data)
        {
            string sql = $"Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        /// <summary>
        /// Thay đổi avatar người dùng.
        /// </summary>
        /// <param name="data">Thông tin avatar mới (kiểu ParamsChangeAvatar).</param>
        /// <returns>1 nếu thành công, 0 nếu thất bại.</returns>
        public virtual int ChangeAvatar(object data)
        {
            string sql = $"Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }
        public virtual int Follow(object data)
        {
            string sql = $"Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual int UnFollow(object data)
        {
            string sql = $"Placeholder";
            var result = Delete(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        public virtual List<Dictionary<string, object>> GetUserFollower(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }

        public virtual List<Dictionary<string, object>> GetUserFollowing(object data)
        {
            string sql = $"Placeholder";
            var result = Read(data, sql);
            return result;
        }
        public virtual int DeleteUser(object data)
        {
            string sql = $"Placeholder";
            var result = Delete(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

    }
}
