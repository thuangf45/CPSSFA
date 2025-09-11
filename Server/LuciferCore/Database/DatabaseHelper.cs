using LuciferCore.Manager;
using Microsoft.Data.SqlClient;
using System.Data;
using static LuciferCore.Core.Simulation;
using static LuciferCore.Database.DbAutoConnector;

namespace LuciferCore.Database
{
    public class DatabaseHelper
    {
        private readonly string database = "KontrollerDB";
        private readonly string user = "sa";
        private readonly string password = "svcntt";
        private readonly string defaultIp = "192.168.1.25";
        private string _connectionString;

        public DatabaseHelper()
        {
            _connectionString = $"Server=localhost;" +
                    $"Database={database};" +
                    $"User Id={user};Password={password};" +
                    "Pooling=true;" +
                    "Min Pool Size=5;" +
                    "Max Pool Size=50;" +
                    "TrustServerCertificate=True;";
        }

        public async Task<bool> AutoConnectString()
        {
            GetModel<LogManager>().LogSystem("DatabaseManager.Init() được gọi", LogLevel.DEBUG);
            try
            {
                var connStr = await TryAutoConnectAsync(database, user, password, defaultIp);
                if (connStr == null) return false;
                _connectionString = connStr;
                return true;
            }
            catch
            {
                GetModel<LogManager>().LogSystem("Kết nối database thất bại.", LogLevel.ERROR);
                return false;
            }
        }

        private SqlCommand CreateCommand(SqlConnection conn, string sql, Dictionary<string, object>? parameters)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }
            return cmd;
        }

        public object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = CreateCommand(conn, sql, parameters);
            return cmd.ExecuteScalar();
        }

        public async Task<object?> ExecuteScalarAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = CreateCommand(conn, sql, parameters);
            return await cmd.ExecuteScalarAsync();
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = CreateCommand(conn, sql, parameters);
            return cmd.ExecuteNonQuery();
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = CreateCommand(conn, sql, parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public DataTable ExecuteQuery(string sql, Dictionary<string, object>? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = CreateCommand(conn, sql, parameters);
            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            conn.Open();
            adapter.Fill(dt);
            return dt;
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = CreateCommand(conn, sql, parameters);
            await using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
    }
}
