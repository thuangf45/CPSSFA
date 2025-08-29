using LuciferCore.Core;
using LuciferCore.Interface;
using Server.LuciferCore.Model;
using System.Data;
using System.Reflection;

namespace LuciferCore.Databasse
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho các database cụ thể, cung cấp CRUD chung (sync & async).
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu bản ghi tương ứng với bảng.</typeparam>
    public abstract class BaseDatabase<T> : IDatabase<T> where T : class, new()
    {
        /// <summary>
        /// Tên bảng tương ứng trong database. Mỗi class con phải override.
        /// </summary>
        protected abstract string TableName { get; }

        /// <summary>
        /// Trả về chính instance hiện tại (theo interface IDatabase).
        /// </summary>
        public IDatabase<T> GetInstance() => this;

        #region Table ID Factory
        private static readonly Dictionary<string, Func<string, IdParams>> TableIdParamFactory = new()
        {
            ["user"] = id => new UserIdParams(id),
            ["review"] = id => new ReviewIdParams(id),
            ["game"] = id => new GameIdParams(id),
            ["reaction"] = id => new ReactionIdParams(id),
            ["comment"] = id => new CommentIdParams(id),
        };

        protected IdParams GetIdParam(string id)
        {
            if (!TableIdParamFactory.TryGetValue(TableName, out var factory))
                throw new InvalidOperationException($"Chưa khai báo IdParams cho bảng {TableName}");
            return factory(id);
        }
        #endregion

        #region CRUD Sync
        public virtual int Create(T data) => 0;

        public virtual T? Read(string id)
        {
            var param = GetIdParam(id);
            var db = Simulation.GetModel<DatabaseHelper>();
            var dt = db.ExecuteQuery($"EXEC {TableName}_GetById @Id", ToParameterDictionary(param));
            return MapToSingle(dt);
        }

        public virtual int Update(string id, object data) => 0;

        public virtual int Delete(object data)
        {
            var db = Simulation.GetModel<DatabaseHelper>();
            return db.ExecuteNonQuery($"EXEC {TableName}_Delete @Id", ToParameterDictionary(data));
        }
        #endregion

        #region CRUD Async
        public virtual async Task<T?> ReadAsync(string id)
        {
            var param = GetIdParam(id);
            var db = Simulation.GetModel<DatabaseHelper>();
            var dt = await db.ExecuteQueryAsync($"EXEC {TableName}_GetById @Id", ToParameterDictionary(param));
            return MapToSingle(dt);
        }

        public virtual async Task<int> DeleteAsync(object data)
        {
            var db = Simulation.GetModel<DatabaseHelper>();
            return await db.ExecuteNonQueryAsync($"EXEC {TableName}_Delete @Id", ToParameterDictionary(data));
        }
        #endregion

        #region Helper Methods

        public static Dictionary<string, object> ToDictionary<TObj>(TObj obj)
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in typeof(TObj).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                dict[prop.Name.StartsWith("@") ? prop.Name : "@" + prop.Name] =
                    prop.GetValue(obj) ?? DBNull.Value;
            }
            return dict;
        }

        public static Dictionary<string, object> JsonToParameterDictionary(string json, string parameterName = "@JsonData")
        {
            return new()
            {
                [parameterName] = string.IsNullOrEmpty(json) ? DBNull.Value : json
            };
        }

        public static Dictionary<string, object> ToParameterDictionary(params (string name, object value)[] parameters)
        {
            return parameters.ToDictionary(
                p => p.name.StartsWith("@") ? p.name : "@" + p.name,
                p => p.value ?? DBNull.Value
            );
        }

        public static Dictionary<string, object> ToParameterDictionary(object obj)
        {
            if (obj is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(
                    kvp => kvp.Key.StartsWith("@") ? kvp.Key : "@" + kvp.Key,
                    kvp => kvp.Value ?? DBNull.Value
                );
            }
            return ToDictionary(obj);
        }

        public static List<T> MapToList<T>(DataTable dt) where T : new()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                T obj = new();
                foreach (var prop in properties)
                {
                    if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                }
                list.Add(obj);
            }
            return list;
        }

        public static T? MapToSingle(DataTable dt)
        {
            return MapToList<T>(dt).FirstOrDefault();
        }

        public static List<T> MapPrimitiveList(DataTable dt)
        {
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                if (row[0] != DBNull.Value)
                    list.Add((T)Convert.ChangeType(row[0], typeof(T)));
            }
            return list;
        }

        public static TResult? GetScalarValue<TResult>(object? value, TResult? defaultValue = default)
        {
            try
            {
                if (value == null || value == DBNull.Value) return defaultValue;
                if (typeof(TResult) == typeof(string))
                    return (TResult)(object)value.ToString();
                return (TResult)Convert.ChangeType(value, typeof(TResult));
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion
    }
}
