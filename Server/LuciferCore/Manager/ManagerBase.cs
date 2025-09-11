namespace LuciferCore.Manager
{
    /// <summary>
    /// Base class chuẩn cho các Manager chạy background task.
    /// </summary>
    public abstract class ManagerBase
    {
        private CancellationTokenSource _cts = new();
        private Task? _task;

        /// <summary>
        /// Khởi động tác vụ nền.
        /// </summary>
        public virtual void Start()
        {
            if (_task != null && !_task.IsCompleted) return;

            if (_cts.IsCancellationRequested)
                _cts = new CancellationTokenSource();

            _task = Task.Run(() => Run(_cts.Token));
            OnStarted();
        }

        /// <summary>
        /// Dừng tác vụ nền.
        /// </summary>
        public virtual void Stop()
        {
            _cts.Cancel();
            OnStopping();
            try
            {
                _task?.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => e is OperationCanceledException);
            }

            OnStopped();
        }

        /// <summary>
        /// Khởi động lại tác vụ nền.
        /// </summary>
        public virtual void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Override để implement vòng lặp nền.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Task chạy vòng lặp nền</returns>
        protected abstract Task Run(CancellationToken token);

        /// <summary>
        /// Override nếu muốn thực hiện hành động khi bắt đầu.
        /// </summary>
        protected virtual void OnStarted() { }


        /// <summary>
        /// Override nếu muốn thực hiện hành động chuẩn bị dừng.
        /// </summary>
        protected virtual void OnStopping() { }

        /// <summary>
        /// Override nếu muốn thực hiện hành động khi dừng.
        /// </summary>
        protected virtual void OnStopped() { }
    }
}
