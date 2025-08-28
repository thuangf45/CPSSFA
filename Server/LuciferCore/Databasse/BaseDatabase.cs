using LuciferCore.Core;
using LuciferCore.Interface;
using LuciferCore.Manager;
using Server.LuciferCore.Model;
using System.Data;
using System.Reflection;

namespace Server.LuciferCore.Databasse
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho các database cụ thể, cung cấp các thao tác chung như Get, Delete, và thực thi truy vấn SQL.
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

        private static readonly Dictionary<string, Func<string, IdParams>> TableIdParamFactory = new()
        {
            ["user"] = id => new UserIdParams(id),
            ["review"] = id => new ReviewIdParams(id),
            ["game"] = id => new GameIdParams(id),
            ["reaction"] = id => new ReactionIdParams(id),
            ["comment"] = id => new CommentIdParams(id),
        };

        public virtual int Create<T>(T data)
        {
            return 0;
        }
        /// <summary>
        /// Lấy bản ghi từ database theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi cần lấy.</param>
        /// <returns>Bản ghi đầu tiên tìm được, hoặc null nếu không có.</returns>
        public virtual T Read(string id)
        {
            var param = TableIdParamFactory[TableName](id);

            var sqlPath = $"{TableName}/get_{TableName.ToLower()}";

            var list = ExecuteQuery<T, IdParams>(sqlPath, param);

            //ObjectHelper.LogObjectProperties(list.FirstOrDefault());

            return list.FirstOrDefault();
        }

        public virtual int Update(string id, object data)
        {

            return 0;
        }

        /// <summary>
        /// Xóa một bản ghi khỏi database.
        /// </summary>
        /// <param name="data">Dữ liệu cần xóa, phải phù hợp với kiểu DeleteRequestBase.</param>
        /// <returns>Số lượng bản ghi bị xóa.</returns>
        public virtual int Delete(object data)
        {
            var sqlPath = $"{TableName}/delete_{TableName.ToLower()}";

            var result = ExecuteScalar<DeleteBaseParams>(sqlPath, data);
            
            return GetScalarValue<int>(result);
        }

        /// <summary>
        /// Thực thi truy vấn kiểu ExecuteScalar (trả về 1 giá trị duy nhất).
        /// </summary>
        /// <typeparam name="TParam">Kiểu của tham số đầu vào.</typeparam>
        /// <param name="sqlPath">Đường dẫn truy vấn SQL tương ứng.</param>
        /// <param name="data">Dữ liệu truyền vào (cần match kiểu TParam).</param>
        /// <returns>Giá trị trả về đầu tiên từ truy vấn hoặc null nếu lỗi.</returns>
        protected object? ExecuteScalar<TParam>(string sqlPath, object data)
        {
            if (data is not TParam model)
                return null;
            var db = Simulation.GetModel<DatabaseManager>();
            db.OpenConnection();

            var param = ToDictionary(model);
            //ObjectHelper.LogObjectProperties(param);
            var result = db.ExecuteScalar(sqlPath, param);

            db.CloseConnection();

            return result;
        }

        /// <summary>
        /// Thực thi truy vấn kiểu ExecuteQuery (trả về danh sách bản ghi).
        /// </summary>
        /// <typeparam name="T">Kiểu bản ghi kết quả.</typeparam>
        /// <typeparam name="TParam">Kiểu tham số truyền vào.</typeparam>
        /// <param name="sqlPath">Đường dẫn file SQL.</param>
        /// <param name="data">Tham số truy vấn (nên đúng kiểu TParam).</param>
        /// <returns>Danh sách bản ghi kết quả hoặc danh sách rỗng nếu sai kiểu.</returns>
        protected List<T> ExecuteQuery<T, TParam>(string sqlPath, object data) where T : new()
        {
            if (data is not TParam model)
                return new List<T>();
            var db = Simulation.GetModel<DatabaseManager>();
            db.OpenConnection();
            var param = ToDictionary(model);
            var dt = db.ExecuteQuery(sqlPath, param);
            db.CloseConnection();
            return MapToList<T>(dt);
        }


        /// <summary>
        /// Chuyển object thành Dictionary với tên cột và giá trị (hỗ trợ prefix @ cho tên)
        /// </summary>
        public static Dictionary<string, object> ToDictionary<T>(T obj)
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                dict[prop.Name.StartsWith("@") ? prop.Name : "@" + prop.Name] = prop.GetValue(obj) ?? DBNull.Value;
            }
            return dict;
        }

        /// <summary>
        /// Chuyển JSON thành Dictionary với tên cột có prefix @ và xử lý null
        /// </summary>
        public static Dictionary<string, object> JsonToParameterDictionary(string json, string parameterName = "@JsonData")
        {
            var dict = new Dictionary<string, object>();
            dict[parameterName] = string.IsNullOrEmpty(json) ? DBNull.Value : json;
            return dict;
        }

        /// <summary>
        /// Chuyển mảng tham số (tuple name-value) thành Dictionary có prefix @
        /// </summary>
        public static Dictionary<string, object> ToParameterDictionary(params (string name, object value)[] parameters)
        {
            var dict = new Dictionary<string, object>();
            foreach (var (name, value) in parameters)
            {
                dict[name.StartsWith("@") ? name : "@" + name] = value ?? DBNull.Value;
            }
            return dict;
        }

        /// <summary>
        /// Chuyển Dictionary input thành Dictionary có prefix @ và xử lý null
        /// </summary>
        public static Dictionary<string, object> ToParameterDictionary(Dictionary<string, object> input)
        {
            return input.ToDictionary(
                kvp => kvp.Key.StartsWith("@") ? kvp.Key : "@" + kvp.Key,
                kvp => kvp.Value ?? DBNull.Value
            );
        }

        /// <summary>
        /// Chuyển đổi DataTable thành danh sách đối tượng T
        /// </summary>
        public static List<T> MapToList<T>(DataTable dt) where T : new()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                T obj = new T();

                foreach (var prop in properties)
                {
                    if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                    }
                }

                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// Lấy giá trị đơn từ object đầu vào, trả về kiểu T hoặc giá trị mặc định nếu lỗi
        /// </summary>
        public static T? GetScalarValue<T>(object? value, T? defaultValue = default)
        {
            try
            {
                if (value == null || value == DBNull.Value) return defaultValue;

                if (typeof(T) == typeof(string))
                    return (T)(object)value.ToString();

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Lấy phần tử đầu tiên từ DataTable đã map về kiểu T (hoặc null nếu không có)
        /// </summary>
        public static T? MapToSingle<T>(DataTable dt) where T : new()
        {
            return MapToList<T>(dt).FirstOrDefault();
        }

        public static List<T> MapPrimitiveList<T>(DataTable table)
        {
            List<T> list = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                if (row[0] != DBNull.Value)
                {
                    list.Add((T)Convert.ChangeType(row[0], typeof(T)));
                }
            }
            return list;
        }
    }
}
