using LuciferCore.Core;
using LuciferCore.Manager;
using System.Data;
using System.Reflection;

namespace LuciferCore.Database
{
    public static class DataMapper
    {
        /// <summary>
        /// Chuẩn hóa key parameter (thêm @ nếu thiếu).
        /// </summary>
        private static string NormalizeKey(string key)
            => key.StartsWith("@") ? key : "@" + key;

        /// <summary>
        /// Chuẩn hóa giá trị parameter: null => DBNull, DataTable giữ nguyên.
        /// </summary>
        private static object NormalizeValue(object? value)
            => value ?? DBNull.Value;

        /// <summary>
        /// Chuyển đổi một object hoặc dictionary thành parameter dictionary
        /// dùng cho database command.
        /// - Nếu là Dictionary: chuẩn hóa key (thêm @ nếu thiếu).
        /// - Nếu là DataTable: coi như table-valued parameter.
        /// - Nếu là object thường: ánh xạ property public thành key/value.
        /// </summary>
        public static Dictionary<string, object> ToParameterDictionary(object obj)
        {
            if (obj is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(
                    kvp => NormalizeKey(kvp.Key),
                    kvp => NormalizeValue(kvp.Value)
                );
            }

            if (obj is DataTable dt)
            {
                return new Dictionary<string, object>
                {
                    ["@TableParam"] = dt // key phải trùng tên tham số SP
                };
            }

            var result = new Dictionary<string, object>();
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                result[NormalizeKey(prop.Name)] = NormalizeValue(prop.GetValue(obj));
            }
            return result;
        }

        public static void LogDictionary(Dictionary<string, object> dict)
        {
            if (dict == null) return;
            foreach (var kv in dict)
            {
                Simulation.GetModel<LogManager>().Log($"[Dict] {kv.Key} = {kv.Value ?? "null"}");
            }
        }

        /// <summary>
        /// Tạo parameter dictionary nhanh bằng tuple (name, value).
        /// Key tự động thêm @ nếu thiếu. Null => DBNull.Value.
        /// </summary>
        public static Dictionary<string, object> ToParameterDictionary(params (string name, object value)[] parameters)
        {
            return parameters.ToDictionary(
                p => NormalizeKey(p.name),
                p => NormalizeValue(p.value)
            );
        }

        public static List<Dictionary<string, object>> DataTableToList(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            return list;
        }

        public static List<T> MapToObjects<T>(List<Dictionary<string, object>> rows) where T : new()
        {
            var list = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var row in rows)
            {
                T obj = new();
                foreach (var prop in props)
                {
                    if (row.ContainsKey(prop.Name) && row[prop.Name] != null)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                    }
                }
                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// Map một DataTable thành danh sách object kiểu T.
        /// Ánh xạ theo tên cột trùng với property public của T.
        /// </summary>
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

        /// <summary>
        /// Lấy 1 object duy nhất từ DataTable (hoặc null nếu rỗng).
        /// </summary>
        public static T? MapToSingle<T>(DataTable dt) where T : new()
        {
            return MapToList<T>(dt).FirstOrDefault();
        }

        /// <summary>
        /// Map DataTable thành danh sách kiểu nguyên thủy (string, int, ...).
        /// Chỉ lấy giá trị ở cột đầu tiên của mỗi row.
        /// </summary>
        public static List<T> MapPrimitiveList<T>(DataTable dt)
        {
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                if (row[0] != DBNull.Value)
                    list.Add((T)Convert.ChangeType(row[0], typeof(T)));
            }
            return list;
        }

        /// <summary>
        /// Trích xuất giá trị scalar từ object (ví dụ kết quả ExecuteScalar).
        /// Nếu null/DBNull hoặc lỗi convert -> trả về defaultValue.
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
    }
}
