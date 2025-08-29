using System.Data;
using System.Reflection;

namespace LuciferCore.Helpers
{
    public static class DataMapper
    {
        /// <summary>
        /// Chuyển đổi một object hoặc dictionary thành parameter dictionary
        /// dùng cho database command. 
        /// - Nếu là Dictionary: chuẩn hóa key (thêm @ nếu thiếu).
        /// - Nếu là object: ánh xạ property public thành key/value.
        /// Null => DBNull.Value.
        /// </summary>
        public static Dictionary<string, object> ToParameterDictionary(object obj)
        {
            if (obj is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(
                    kvp => kvp.Key.StartsWith("@") ? kvp.Key : "@" + kvp.Key,
                    kvp => kvp.Value ?? DBNull.Value
                );
            }

            var result = new Dictionary<string, object>();
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                result[prop.Name.StartsWith("@") ? prop.Name : "@" + prop.Name] =
                    prop.GetValue(obj) ?? DBNull.Value;
            }
            return result;
        }

        /// <summary>
        /// Tạo parameter dictionary nhanh bằng tuple (name, value).
        /// Key tự động thêm @ nếu thiếu. Null => DBNull.Value.
        /// </summary>
        public static Dictionary<string, object> ToParameterDictionary(params (string name, object value)[] parameters)
        {
            return parameters.ToDictionary(
                p => p.name.StartsWith("@") ? p.name : "@" + p.name,
                p => p.value ?? DBNull.Value
            );
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
