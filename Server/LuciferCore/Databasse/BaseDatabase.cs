using LuciferCore.Core;
using LuciferCore.Interface;
using LuciferCore.Helpers;
using Server.LuciferCore.Model;

namespace LuciferCore.Databasse
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho các database cụ thể, 
    /// cung cấp CRUD chung (sync & async).
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu bản ghi tương ứng với bảng.</typeparam>
    public abstract class BaseDatabase<T> : IDatabase<T> where T : class, new()
    {
        /// <summary>
        /// Tên bảng tương ứng trong database. 
        /// Mỗi class con phải override.
        /// </summary>
        protected abstract string TableName { get; }

        /// <summary>
        /// Trả về chính instance hiện tại (theo interface IDatabase).
        /// </summary>
        public IDatabase<T> GetInstance() => this;

        /// <summary>
        /// Dictionary ánh xạ tên bảng sang factory tạo IdParams.
        /// </summary>
        private static readonly Dictionary<string, Func<string, IdParams>> TableIdParamFactory = new()
        {
            ["user"] = id => new UserIdParams(id),
            ["review"] = id => new ReviewIdParams(id),
            ["game"] = id => new GameIdParams(id),
            ["reaction"] = id => new ReactionIdParams(id),
            ["comment"] = id => new CommentIdParams(id),
        };

        /// <summary>
        /// Lấy IdParams tương ứng với bảng hiện tại.
        /// </summary>
        protected IdParams GetIdParam(string id)
        {
            if (!TableIdParamFactory.TryGetValue(TableName, out var factory))
                throw new InvalidOperationException($"Chưa khai báo IdParams cho bảng {TableName}");
            return factory(id);
        }

        #region CRUD Sync
        /// <summary>
        /// Tạo mới một bản ghi trong bảng.
        /// </summary>
        public virtual int Create(T data)
        {
            var db = Simulation.GetModel<DatabaseHelper>();
            return db.ExecuteNonQuery($"EXEC {TableName}_Create", DataMapper.ToParameterDictionary(data));
        }

        /// <summary>
        /// Đọc bản ghi theo Id.
        /// </summary>
        public virtual T? Read(string id)
        {
            var param = GetIdParam(id);
            var db = Simulation.GetModel<DatabaseHelper>();
            var dt = db.ExecuteQuery($"EXEC {TableName}_GetById @Id", DataMapper.ToParameterDictionary(param));
            return DataMapper.MapToSingle<T>(dt);
        }

        /// <summary>
        /// Cập nhật bản ghi theo Id.
        /// </summary>
        public virtual int Update(string id, object data)
        {
            var param = DataMapper.ToParameterDictionary(data);
            param["@Id"] = id; // luôn kèm Id
            var db = Simulation.GetModel<DatabaseHelper>();
            return db.ExecuteNonQuery($"EXEC {TableName}_Update", param);
        }

        /// <summary>
        /// Xoá bản ghi theo Id hoặc object có Id.
        /// </summary>
        public virtual int Delete(object data)
        {
            var db = Simulation.GetModel<DatabaseHelper>();
            return db.ExecuteNonQuery($"EXEC {TableName}_Delete @Id", DataMapper.ToParameterDictionary(data));
        }
        #endregion

        #region CRUD Async
        /// <summary>
        /// Tạo mới bản ghi (async).
        /// </summary>
        public virtual async Task<int> CreateAsync(T data)
        {
            var db = Simulation.GetModel<DatabaseHelper>();
            return await db.ExecuteNonQueryAsync($"EXEC {TableName}_Create", DataMapper.ToParameterDictionary(data));
        }

        /// <summary>
        /// Đọc bản ghi theo Id (async).
        /// </summary>
        public virtual async Task<T?> ReadAsync(string id)
        {
            var param = GetIdParam(id);
            var db = Simulation.GetModel<DatabaseHelper>();
            var dt = await db.ExecuteQueryAsync($"EXEC {TableName}_GetById @Id", DataMapper.ToParameterDictionary(param));
            return DataMapper.MapToSingle<T>(dt);
        }

        /// <summary>
        /// Cập nhật bản ghi theo Id (async).
        /// </summary>
        public virtual async Task<int> UpdateAsync(string id, object data)
        {
            var param = DataMapper.ToParameterDictionary(data);
            param["@Id"] = id;
            var db = Simulation.GetModel<DatabaseHelper>();
            return await db.ExecuteNonQueryAsync($"EXEC {TableName}_Update", param);
        }

        /// <summary>
        /// Xoá bản ghi (async).
        /// </summary>
        public virtual async Task<int> DeleteAsync(object data)
        {
            var db = Simulation.GetModel<DatabaseHelper>();
            return await db.ExecuteNonQueryAsync($"EXEC {TableName}_Delete @Id", DataMapper.ToParameterDictionary(data));
        }
        #endregion
    }
}
