using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuciferCore.Helper
{
    /// <summary>
    /// JsonHelper cung cấp các tiện ích để:<br/>
    /// - Chuyển object thành JSON string.<br/>
    /// - Parse JSON string thành object kiểu cụ thể hoặc Dictionary.<br/>
    /// - Thêm hoặc xoá thuộc tính trong JSON string trước khi parse.<br/>
    /// - Xử lý an toàn lỗi khi format JSON không hợp lệ.<br/>
    /// Sử dụng System.Text.Json, cấu hình sẵn để:<br/>
    /// - Bỏ qua thuộc tính null khi serialize.<br/>
    /// - Không phân biệt chữ hoa/thường khi parse.<br/>
    /// - Cho phép dấu phẩy cuối và bỏ qua comment.<br/>
    /// </summary>
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Chuyển object thành JSON string để gửi response.
        /// </summary>
        public static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, DefaultOptions);
        }

        /// <summary>
        /// Parse JSON string từ request thành object kiểu T.
        /// Trả về null nếu lỗi format hoặc không parse được.
        /// </summary>
        public static T? Deserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, DefaultOptions);
            }
            catch (JsonException)
            {
                // Có thể log lỗi ở đây nếu cần
                return default;
            }
        }
        /// <summary>
        /// Parse JSON string thành Dictionary để xử lý động.
        /// </summary>
        public static Dictionary<string, object?>? DeserializeToDict(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var props = doc.RootElement.EnumerateObject();

                var dict = new Dictionary<string, object?>();

                foreach (var prop in props)
                {
                    object? value = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.TryGetInt64(out long l) ? l : prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => prop.Value.ToString()
                    };

                    dict[prop.Name] = value;
                }

                return dict;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Thêm 1 thuộc tính vào JSON string và parse thành object kiểu T.
        /// Trả về default nếu lỗi.
        /// </summary>
        public static T? AddPropertyAndDeserialize<T>(string json, string key, object? value)
        {
            try
            {
                var dict = DeserializeToDict(json);
                if (dict == null) return default;

                dict[key] = value;
                var jsonModified = Serialize(dict);

                return Deserialize<T>(jsonModified);
            }
            catch
            {
                return default;
            }
        }


        /// <summary>
        /// Xoá 1 thuộc tính khỏi JSON string và parse lại thành object kiểu T.
        /// Trả về default nếu lỗi.
        /// </summary>
        public static T? RemovePropertyAndDeserialize<T>(string json, string keyToRemove)
        {
            try
            {
                var dict = Deserialize<Dictionary<string, object?>>(json);
                if (dict == null) return default;

                dict.Remove(keyToRemove);
                var modifiedJson = Serialize(dict);
                return Deserialize<T>(modifiedJson);
            }
            catch
            {
                return default;
            }
        }


    }
}
