using LuciferCore.Core;
using System.Collections.Concurrent;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý các thao tác database chạy nền, gom nhóm công việc (Insert/Update/Delete)
    /// để tối ưu hiệu suất, đảm bảo an toàn đa luồng.
    /// </summary>
    public class DatabaseManager : ManagerBase
    {
        /// <summary>
        /// Job database (Insert/Update/Delete).
        /// </summary>
        private readonly BlockingCollection<DbJob> _mainQueue = new();

        /// <summary>
        /// Các hàng đợi con để gom nhóm theo loại thao tác.
        /// </summary>
        private readonly List<DbJob> _insertQueue = new();
        private readonly List<DbJob> _updateQueue = new();
        private readonly List<DbJob> _deleteQueue = new();

        /// <summary>
        /// Số lượng tối đa 1 batch trước khi flush xuống DB.
        /// </summary>
        private const int BatchSize = 50;

        /// <summary>
        /// Thời gian flush định kỳ (ms) để tránh giữ job quá lâu.
        /// </summary>
        private const int FlushInterval = 5000;

        /// <summary>
        /// Thêm 1 job database vào hàng đợi chính.
        /// </summary>
        public void Enqueue(DbJob job) => _mainQueue.Add(job);

        /// <summary>
        /// Xử lý chạy nền, lấy job từ hàng đợi chính và phân loại vào subQueue.
        /// Khi subQueue đủ số lượng hoặc timeout thì flush xuống DB.
        /// </summary>
        protected override async Task Run(CancellationToken token)
        {
            var lastFlush = DateTime.Now;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Lấy job từ mainQueue
                    if (_mainQueue.TryTake(out var job, 100, token))
                    {
                        switch (job.Type)
                        {
                            case DbJobType.Insert:
                                _insertQueue.Add(job);
                                break;
                            case DbJobType.Update:
                                _updateQueue.Add(job);
                                break;
                            case DbJobType.Delete:
                                _deleteQueue.Add(job);
                                break;
                        }
                    }

                    // Flush theo batch size
                    if (_insertQueue.Count >= BatchSize) await FlushInsertAsync();
                    if (_updateQueue.Count >= BatchSize) await FlushUpdateAsync();
                    if (_deleteQueue.Count >= BatchSize) await FlushDeleteAsync();

                    // Flush định kỳ theo timeout
                    if ((DateTime.Now - lastFlush).TotalMilliseconds >= FlushInterval)
                    {
                        if (_insertQueue.Count > 0) await FlushInsertAsync();
                        if (_updateQueue.Count > 0) await FlushUpdateAsync();
                        if (_deleteQueue.Count > 0) await FlushDeleteAsync();
                        lastFlush = DateTime.Now;
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // dừng hợp lệ
                }
                catch (Exception ex)
                {
                    Simulation.GetModel<LogManager>().Log(ex);
                    await Task.Delay(500, token);
                }
            }

            // Flush nốt job còn lại trước khi shutdown
            if (_insertQueue.Count > 0) await FlushInsertAsync();
            if (_updateQueue.Count > 0) await FlushUpdateAsync();
            if (_deleteQueue.Count > 0) await FlushDeleteAsync();
        }

        private Task FlushInsertAsync()
        {
            var jobs = _insertQueue.ToList();
            _insertQueue.Clear();

            // TODO: triển khai batch insert xuống DB
            Simulation.GetModel<LogManager>().LogSystem($"[DB] Flushed {jobs.Count} INSERT jobs.");
            return Task.CompletedTask;
        }

        private Task FlushUpdateAsync()
        {
            var jobs = _updateQueue.ToList();
            _updateQueue.Clear();

            // TODO: triển khai batch update xuống DB
            Simulation.GetModel<LogManager>().LogSystem($"[DB] Flushed {jobs.Count} UPDATE jobs.");
            return Task.CompletedTask;
        }

        private Task FlushDeleteAsync()
        {
            var jobs = _deleteQueue.ToList();
            _deleteQueue.Clear();

            // TODO: triển khai batch delete xuống DB
            Simulation.GetModel<LogManager>().LogSystem($"[DB] Flushed {jobs.Count} DELETE jobs.");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Loại job database.
    /// </summary>
    public enum DbJobType
    {
        Insert,
        Update,
        Delete
    }

    /// <summary>
    /// Job database chứa dữ liệu và loại thao tác.
    /// </summary>
    public class DbJob
    {
        public DbJobType Type { get; set; }
        public object Data { get; set; }
        public string TableName { get; set; }

        public DbJob(DbJobType type, object data, string tableName)
        {
            Type = type;
            Data = data;
            TableName = tableName;
        }
    }
}
