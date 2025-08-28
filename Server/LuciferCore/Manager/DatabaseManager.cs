using Microsoft.Data.SqlClient;
using LuciferCore.Core;
using LuciferCore.Helper;
using System.Data;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý kết nối cơ sở dữ liệu SQL Server và thực thi các truy vấn SQL, hỗ trợ lưu trữ cache câu lệnh SQL từ tệp.
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _basePath; // Nơi chứa thư mục gốc chứa file sql
        private readonly string database = "KontrollerDB";
        private readonly string user = "sa";
        private readonly string password = "Admin@123";
        private readonly string defaultIp = "192.168.1.25"; // IP default của máy SQL Server
        private string _connectionString;
        public event Action FailedConnectDB;

        /// <summary>
        /// Bộ nhớ cache cho các câu lệnh SQL, với khóa dạng "folder/file" hoặc "file".
        /// </summary>
        private readonly Dictionary<string, string> _sqlCache = new Dictionary<string, string>();

        /// <summary>
        /// Theo dõi thời gian sửa đổi cuối cùng của các tệp SQL để kiểm tra thay đổi.
        /// </summary>
        private readonly Dictionary<string, DateTime> _fileLastWriteTime = new Dictionary<string, DateTime>();

        private SqlConnection? _connection;   
        private string GetLocalSubnet()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            string[] parts = ip.Address.ToString().Split('.');
                            if (parts.Length == 4)
                            {
                                return $"{parts[0]}.{parts[1]}.{parts[2]}";
                            }
                        }
                    }
                }
            }
            return null;
        }
        private string GetLocalIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        return ip.Address.ToString();
                }
            }
            return null;
        }


        private async Task<string> TryAutoConnectAsync(string database, string user, string password, string defaultIp)
        {
            // Thử localhost
            string localConn = $"Server=localhost;Database={database};Integrated Security=True;TrustServerCertificate=True;";
            if (await TestConnectionAsync(localConn))
                return localConn;

            // Thử IP default
            string defaultConn = $"Server={GetLocalIPv4()};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";
            if (await TestConnectionAsync(defaultConn))
                return defaultConn;

            // Quét LAN (song song)
            string baseSubnet = GetLocalSubnet();
            if (baseSubnet != null)
            {
                Simulation.GetModel<LogManager>().Log($"🔍 Đang quét subnet {baseSubnet}.x ...", LogLevel.INFO, LogSource.SYSTEM);
                var tasks = new List<Task<(string, bool)>>();
                for (int i = 1; i <= 10; i++) // Giới hạn quét
                {
                    string ip = $"{baseSubnet}.{i}";
                    if (ip == defaultIp) continue;
                    string conn = $"Server={ip};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";
                    tasks.Add(TestConnectionWithResultAsync(conn));
                }

                var results = await Task.WhenAll(tasks);
                foreach (var result in results)
                {
                    if (result.Item2)
                        return result.Item1;
                }
            }

            return null;
        }

        private async Task<(string, bool)> TestConnectionWithResultAsync(string connectionString)
        {
            bool success = await TestConnectionAsync(connectionString);
            return (connectionString, success);
        }

        private async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    ConnectTimeout = 1
                };
                using (var conn = new SqlConnection(builder.ConnectionString))
                {
                    await conn.OpenAsync();
                }
                Simulation.GetModel<LogManager>().Log($"✅ Thành công: {connectionString}", LogLevel.INFO, LogSource.SYSTEM);
                return true;
            }
            catch
            {
                Simulation.GetModel<LogManager>().Log($"❌ Thất bại: {connectionString}", LogLevel.ERROR, LogSource.SYSTEM);
                return false;
            }
        }

        /// <summary>
        /// Khởi tạo <see cref="DatabaseManager"/> với đường dẫn thư mục SQL và chuỗi kết nối.
        /// </summary>
        /// <param name="basePath">Đường dẫn thư mục chứa các tệp SQL.</param>
        /// <param name="connectionString">Chuỗi kết nối tới cơ sở dữ liệu SQL Server.</param>
        /// <exception cref="ArgumentNullException">Ném ra nếu <paramref name="basePath"/> hoặc <paramref name="connectionString"/> là null.</exception>
        public DatabaseManager(string basePath, string connectionString)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task Init()
        {
            Simulation.GetModel<LogManager>().Log("DatabaseManager.Init() được gọi", LogLevel.DEBUG, LogSource.SYSTEM);
            try
            {
                _connectionString = await TryAutoConnectAsync(database, user, password, defaultIp);

                if (_connectionString == null)
                {
                    Simulation.GetModel<LogManager>().Log("Kết nối database thất bại.", LogLevel.ERROR, LogSource.SYSTEM);
                    FailedConnectDB?.Invoke();
                }
                else
                {
                    Simulation.GetModel<LogManager>().Log("Kết nối database thành công.", LogLevel.INFO, LogSource.SYSTEM);
                }
            }
            catch
            {
                Simulation.GetModel<LogManager>().Log("Kết nối database thất bại.", LogLevel.ERROR, LogSource.SYSTEM);
            }

        }

        /// <summary>
        /// Khởi tạo <see cref="DatabaseManager"/> với đường dẫn mặc định và chuỗi kết nối tới cơ sở dữ liệu KontrollerDB.
        /// </summary>
        public DatabaseManager()
        {
            _basePath = Path.Combine(AppContext.BaseDirectory, "extra_files", "MyServerData", "queries");
        }

        /// <summary>
        /// Lấy câu lệnh SQL từ cache hoặc đọc từ tệp, hỗ trợ thư mục con (ví dụ: "users/get_user_by_id").
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL trong cache hoặc đường dẫn tệp (ví dụ: "users/get_user_by_id").</param>
        /// <returns>Chuỗi câu lệnh SQL.</returns>
        /// <exception cref="FileNotFoundException">Ném ra nếu tệp SQL không tồn tại.</exception>
        public string GetSql(string key)
        {
            if (_sqlCache.TryGetValue(key, out var cachedSql))
            {
                // Kiểm tra file có thay đổi không
                string fullPath = GetFullPathFromKey(key);
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(fullPath);

                if (_fileLastWriteTime.TryGetValue(key, out var lastCachedWriteTime))
                {
                    if (lastWriteTime > lastCachedWriteTime)
                    {
                        // File đã thay đổi => reload
                        return ReloadSql(key, fullPath, lastWriteTime);
                    }
                    else
                    {
                        // File không thay đổi => trả cache
                        return cachedSql;
                    }
                }
                else
                {
                    // Lần đầu chưa lưu time => cập nhật
                    _fileLastWriteTime[key] = lastWriteTime;
                    return cachedSql;
                }
            }
            else
            {
                // Chưa có trong cache => đọc file
                string fullPath = GetFullPathFromKey(key);
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"File SQL không tồn tại: {fullPath}");

                return ReloadSql(key, fullPath, File.GetLastWriteTimeUtc(fullPath));
            }
        }

        /// <summary>
        /// Tải lại câu lệnh SQL từ tệp và cập nhật cache.
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL.</param>
        /// <param name="fullPath">Đường dẫn đầy đủ tới tệp SQL.</param>
        /// <param name="lastWriteTime">Thời gian sửa đổi cuối cùng của tệp.</param>
        /// <returns>Chuỗi câu lệnh SQL được tải lại.</returns>
        private string ReloadSql(string key, string fullPath, DateTime lastWriteTime)
        {
            string sql = File.ReadAllText(fullPath);
            _sqlCache[key] = sql;
            _fileLastWriteTime[key] = lastWriteTime;
            return sql;
        }

        /// <summary>
        /// Chuyển đổi khóa thành đường dẫn đầy đủ tới tệp SQL.
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL (ví dụ: "users/get_user_by_id").</param>
        /// <returns>Đường dẫn đầy đủ tới tệp SQL (ví dụ: "D:/MyServerData/users/get_user_by_id.sql").</returns>
        private string GetFullPathFromKey(string key)
        {
            // Key có thể là "users/get_user_by_id" => convert thành D:/MyServerData/users/get_user_by_id.sql
            string relativePath = key.Replace('/', Path.DirectorySeparatorChar) + ".sql";
            return Path.Combine(_basePath, relativePath);
        }

        /// <summary>
        /// Xóa toàn bộ cache câu lệnh SQL và thời gian sửa đổi tệp.
        /// </summary>
        public void ClearCache()
        {
            _sqlCache.Clear();
            _fileLastWriteTime.Clear();
        }

        /// <summary>
        /// Mở kết nối tới cơ sở dữ liệu SQL Server.
        /// </summary>
        /// <exception cref="InvalidOperationException">Ném ra nếu kết nối không thể mở.</exception>
        public void OpenConnection()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// Đóng và giải phóng kết nối cơ sở dữ liệu.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                    _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>
        /// Thực thi câu lệnh SQL không trả về dữ liệu (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL trong cache hoặc tệp.</param>
        /// <param name="parameters">Tham số SQL (nếu có).</param>
        /// <returns>Số dòng bị ảnh hưởng bởi câu lệnh.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu kết nối chưa được mở.</exception>
        public int ExecuteNonQuery(string key, Dictionary<string, object>? parameters = null)
        {
            string sql = GetSql(key);
            using var cmd = CreateCommand(sql, parameters);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Thực thi câu lệnh SQL trả về dữ liệu dưới dạng <see cref="DataTable"/> (SELECT).
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL trong cache hoặc tệp.</param>
        /// <param name="parameters">Tham số SQL (nếu có).</param>
        /// <returns>Bảng dữ liệu chứa kết quả truy vấn.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu kết nối chưa được mở.</exception>
        public DataTable ExecuteQuery(string key, Dictionary<string, object>? parameters = null)
        {
            string sql = GetSql(key);
            using var cmd = CreateCommand(sql, parameters);
            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Thực thi câu lệnh SQL trả về một giá trị duy nhất (thường dùng với COUNT, SUM, ...).
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL trong cache hoặc tệp.</param>
        /// <param name="parameters">Tham số SQL (nếu có).</param>
        /// <returns>Giá trị duy nhất từ truy vấn hoặc <c>null</c> nếu không có kết quả.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu kết nối chưa được mở.</exception>
        public object? ExecuteScalar(string key, Dictionary<string, object>? parameters = null)
        {
            string sql = GetSql(key);
            using var cmd = CreateCommand(sql, parameters);
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// Tạo đối tượng <see cref="SqlCommand"/> với câu lệnh SQL và tham số.
        /// </summary>
        /// <param name="sql">Câu lệnh SQL cần thực thi.</param>
        /// <param name="parameters">Tham số SQL (nếu có).</param>
        /// <returns>Đối tượng <see cref="SqlCommand"/> đã được cấu hình.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu kết nối chưa được mở.</exception>
        private SqlCommand CreateCommand(string sql, Dictionary<string, object>? parameters)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection chưa được mở.");

            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }
            }

            return cmd;
        }

        /// <summary>
        /// Thực thi câu lệnh SQL trả về dữ liệu dưới dạng <see cref="DataTable"/> bất đồng bộ (SELECT).
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL trong cache hoặc tệp.</param>
        /// <param name="parameters">Tham số SQL (nếu có).</param>
        /// <returns>Bảng dữ liệu chứa kết quả truy vấn.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu kết nối chưa được mở.</exception>
        public async Task<DataTable> ExecuteQueryAsync(string key, Dictionary<string, object>? parameters = null)
        {
            string sql = GetSql(key);
            using var cmd = CreateCommand(sql, parameters);
            using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        /// <summary>
        /// Thực thi câu lệnh SQL không trả về dữ liệu bất đồng bộ (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="key">Khóa của câu lệnh SQL trong cache hoặc tệp.</param>
        /// <param name="parameters">Tham số SQL (nếu có).</param>
        /// <returns>Số dòng bị ảnh hưởng bởi câu lệnh.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu kết nối chưa được mở.</exception>
        public async Task<int> ExecuteNonQueryAsync(string key, Dictionary<string, object>? parameters = null)
        {
            string sql = GetSql(key);
            using var cmd = CreateCommand(sql, parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        // Ghi chú: Không có phương thức Dispose rõ ràng trong mã gốc, có thể cần xem xét thêm IDisposable
    }
}

/*
// Ví dụ cách sử dụng:
// var connStr = "Server=localhost;Database=MyDatabase;User Id=myUsername;Password=myPassword;";
// var connStr = "Server=localhost;Database=MyDatabase;Integrated Security=True;";
// Truy cập SQL Server ở máy khác
// var connStr = "Server=192.168.1.100;Database=MyDb;User Id=sa;Password=pass;";

var connStr = "your connection string here";
using var dbManager = new DatabaseManager("D:/MyServerData", connStr);

dbManager.OpenConnection();

// Ví dụ chạy truy vấn trả về nhiều dòng
var users = dbManager.ExecuteQuery("users/get_all_users");

// Ví dụ truy vấn lấy một giá trị
var count = (int)dbManager.ExecuteScalar("users/count_users");

// Ví dụ insert với tham số
var param = new Dictionary<string, object> { ["@UserName"] = "John", ["@Age"] = 30 };
int rows = dbManager.ExecuteNonQuery("users/insert_user", param);

dbManager.CloseConnection();
*/