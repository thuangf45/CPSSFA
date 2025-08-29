using LuciferCore.Core;
using LuciferCore.Manager;
using Microsoft.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LuciferCore.Databasse
{
    public static class DbAutoConnector
    {

        public static async Task<string> TryAutoConnectAsync(
            string database, string user, string password, string defaultIp)
        {
            // 1. Localhost
            string localConn = $"Server=localhost;Database={database};Integrated Security=True;" +
                               "Pooling=true;Min Pool Size=5;Max Pool Size=50;TrustServerCertificate=True;";
            if (await TestConnectionAsync(localConn))
                return localConn;

            // 2. IP mặc định hoặc local IPv4
            string defaultConn = $"Server={defaultIp};Database={database};User Id={user};Password={password};" +
                                 "Pooling=true;Min Pool Size=5;Max Pool Size=50;TrustServerCertificate=True;";
            if (await TestConnectionAsync(defaultConn))
                return defaultConn;

            // 3. Quét LAN (nếu cần)
            string baseSubnet = GetLocalSubnet();
            if (baseSubnet != null)
            {
                Simulation.GetModel<LogManager>()
                    .Log($"🔍 Đang quét subnet {baseSubnet}.x ...", LogLevel.INFO, LogSource.SYSTEM);

                var tasks = new List<Task<(string, bool)>>();
                for (int i = 1; i <= 10; i++)
                {
                    string ip = $"{baseSubnet}.{i}";
                    if (ip == defaultIp) continue;

                    string conn = $"Server={ip};Database={database};User Id={user};Password={password};" +
                                  "Pooling=true;Min Pool Size=5;Max Pool Size=50;TrustServerCertificate=True;";
                    tasks.Add(TestConnectionWithResultAsync(conn));
                }

                var results = await Task.WhenAll(tasks);
                foreach (var result in results)
                    if (result.Item2) return result.Item1;
            }

            return null;
        }

        private static string GetLocalSubnet()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
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

        private static string GetLocalIPv4()
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

        private static async Task<(string, bool)> TestConnectionWithResultAsync(string connectionString)
        {
            bool success = await TestConnectionAsync(connectionString);
            return (connectionString, success);
        }

        private static async Task<bool> TestConnectionAsync(string connectionString)
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
                return true;
            }
            catch
            { 
                return false;
            }
        }
    }
}
