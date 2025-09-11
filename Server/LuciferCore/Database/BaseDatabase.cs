using LuciferCore.Interface;
using static LuciferCore.Core.Simulation;

namespace LuciferCore.Database
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho các database cụ thể, 
    /// cung cấp CRUD chung (sync & async).
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu bản ghi tương ứng với bảng.</typeparam>
    public abstract class BaseDatabase : IDatabase
    {
        /// <summary>
        /// Tạo mới một bản ghi trong bảng.
        /// </summary>
        public virtual object Create(object data, string sql) 
            => GetModel<DatabaseHelper>().ExecuteNonQuery(sql, DataMapper.ToParameterDictionary(data));

        /// <summary>
        /// Đọc bản ghi theo Id.
        /// </summary>
        public virtual List<Dictionary<string, object>> Read(object data, string sql)
            => DataMapper.DataTableToList(GetModel<DatabaseHelper>().ExecuteQuery(sql, DataMapper.ToParameterDictionary(data)));

        /// <summary>
        /// Cập nhật bản ghi theo Id.
        /// </summary>
        public virtual object Update(object data, string sql) 
            => GetModel<DatabaseHelper>().ExecuteNonQuery(sql, DataMapper.ToParameterDictionary(data));

        /// <summary>
        /// Xoá bản ghi theo Id hoặc object có Id.
        /// </summary>
        public virtual object Delete(object data, string sql) 
            => GetModel<DatabaseHelper>().ExecuteNonQuery(sql, DataMapper.ToParameterDictionary(data));

        /// <summary>
        /// Tạo mới bản ghi (async).
        /// </summary>
        public virtual async Task<object> CreateAsync(object data, string sql)
            => await GetModel<DatabaseHelper>().ExecuteNonQueryAsync(sql, DataMapper.ToParameterDictionary(data));
  
        /// <summary>
        /// Đọc bản ghi theo Id (async).
        /// </summary>
        public virtual async Task<List<Dictionary<string, object>>> ReadAsync(object data, string sql)
            => DataMapper.DataTableToList(await GetModel<DatabaseHelper>().ExecuteQueryAsync(sql, DataMapper.ToParameterDictionary(data)));

        /// <summary>
        /// Cập nhật bản ghi theo Id (async).
        /// </summary>
        public virtual async Task<object> UpdateAsync(object data, string sql)
            => await GetModel<DatabaseHelper>().ExecuteNonQueryAsync(sql, DataMapper.ToParameterDictionary(data));

        /// <summary>
        /// Xoá bản ghi (async).
        /// </summary>
        public virtual async Task<object> DeleteAsync(object data, string sql)
            => await GetModel<DatabaseHelper>().ExecuteNonQueryAsync(sql, DataMapper.ToParameterDictionary(data));
    }
}
