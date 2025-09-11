namespace LuciferCore.Interface
{
    /// <summary>
    /// Interface định nghĩa các thao tác cơ bản cho lớp quản lý cơ sở dữ liệu.
    /// Gồm truy xuất instance, lấy dữ liệu theo ID và xóa dữ liệu.
    /// </summary>
    public interface IDatabase
    {
        object Create(object data, string sql);
        List<Dictionary<string, object>> Read(object data, string sql);
        object Update(object data, string sql);
        object Delete(object data, string sql);
        Task<object> CreateAsync(object data, string sql);
        Task<List<Dictionary<string, object>>> ReadAsync(object data, string sql);
        Task<object> UpdateAsync(object data, string sql);
        Task<object> DeleteAsync(object data, string sql);
    }
}
