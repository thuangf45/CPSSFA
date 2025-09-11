using Newtonsoft.Json;
using LuciferCore.NetCoreServer;
using System.Text;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý bộ nhớ cache để lưu trữ và truy xuất dữ liệu dưới dạng JSON, sử dụng <see cref="FileCache"/> làm cơ chế lưu trữ.
    /// </summary>
    public class CacheManager
    {
        private readonly FileCache _fileCache = new FileCache();

        /// <summary>
        /// Lưu trữ một đối tượng bất kỳ dưới dạng JSON vào cache.
        /// </summary>
        /// <typeparam name="T">Kiểu của đối tượng cần lưu.</typeparam>
        /// <param name="key">Khóa để xác định đối tượng trong cache.</param>
        /// <param name="obj">Đối tượng cần lưu trữ.</param>
        /// <param name="timeout">Thời gian tồn tại của mục cache (nếu có). Mặc định là không giới hạn.</param>
        /// <returns>Trả về <c>true</c> nếu lưu thành công, ngược lại trả về <c>false</c>.</returns>
        public bool Set<T>(string key, T obj, TimeSpan? timeout = null)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                return _fileCache.Add(key, bytes, timeout ?? TimeSpan.Zero);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy đối tượng từ cache và chuyển đổi thành kiểu <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Kiểu của đối tượng cần lấy.</typeparam>
        /// <param name="key">Khóa để xác định đối tượng trong cache.</param>
        /// <returns>
        /// Một tuple chứa:
        /// - <c>found</c>: <c>true</c> nếu tìm thấy và giải mã thành công, ngược lại là <c>false</c>.
        /// - <c>result</c>: Đối tượng kiểu <typeparamref name="T"/> nếu tìm thấy, ngược lại là giá trị mặc định.
        /// </returns>
        public (bool found, T result) Get<T>(string key)
        {
            var (found, bytes) = _fileCache.Find(key);
            if (!found) return (false, default);

            try
            {
                string json = Encoding.UTF8.GetString(bytes);
                T obj = JsonConvert.DeserializeObject<T>(json);
                return (true, obj);
            }
            catch
            {
                return (false, default);
            }
        }

        /// <summary>
        /// Lấy chuỗi JSON trực tiếp từ cache mà không cần giải mã thành đối tượng.
        /// </summary>
        /// <param name="key">Khóa để xác định dữ liệu trong cache.</param>
        /// <returns>
        /// Một tuple chứa:
        /// - <c>found</c>: <c>true</c> nếu tìm thấy dữ liệu, ngược lại là <c>false</c>.
        /// - <c>json</c>: Chuỗi JSON nếu tìm thấy, ngược lại là <c>null</c>.
        /// </returns>
        public (bool found, string json) GetJson(string key)
        {
            var (found, bytes) = _fileCache.Find(key);
            if (!found) return (false, null);

            try
            {
                string json = Encoding.UTF8.GetString(bytes);
                return (true, json);
            }
            catch
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Xóa một mục khỏi cache dựa trên khóa.
        /// </summary>
        /// <param name="key">Khóa của mục cần xóa.</param>
        /// <returns>Trả về <c>true</c> nếu xóa thành công, ngược lại trả về <c>false</c>.</returns>
        public bool Remove(string key)
        {
            return _fileCache.Remove(key);
        }

        /// <summary>
        /// Tải dữ liệu từ một thư mục vào cache với các tham số tùy chỉnh.
        /// </summary>
        /// <param name="path">Đường dẫn thư mục chứa dữ liệu cần tải.</param>
        /// <param name="prefix">Tiền tố cho các khóa trong cache. Mặc định là "/".</param>
        /// <param name="filter">Bộ lọc tệp (ví dụ: "*.*"). Mặc định là "*.*".</param>
        /// <param name="timeout">Thời gian tồn tại của các mục cache (nếu có). Mặc định là không giới hạn.</param>
        /// <returns>Trả về <c>true</c> nếu tải thành công, ngược lại trả về <c>false</c>.</returns>
        public bool LoadPath(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = null)
        {
            return _fileCache.InsertPath(path, prefix, filter, timeout ?? TimeSpan.Zero);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu trong cache.
        /// </summary>
        public void Clear()
        {
            _fileCache.Clear();
        }
    }
}

/*
✅ Cách sử dụng: 
📝 Lưu một đối tượng:
var user = new { Id = 1, Name = "Thuận" };
cacheManager.Set("/user/1", user);
 
📤 Gửi dưới dạng byte[]
var (found, json) = cacheManager.GetJson("/user/1");
if (found)
{
    byte[] data = Encoding.UTF8.GetBytes(json);
    Send(data); // hoặc HTTP stream.Write(data)
}
 
📥 Đọc lại và chuyển thành đối tượng
var (found, userObj) = cacheManager.Get<YourUserClass>("/user/1");
if (found)
{
    Console.WriteLine(userObj.Name);
}
   
🧹 Dọn sạch cache
cacheManager.Clear();
*/