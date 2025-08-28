namespace Server.Source.Core
{
    /// <summary>
    /// Lớp Simulation triển khai mẫu mô phỏng sự kiện rời rạc (discrete event simulator pattern).
    /// Các sự kiện được sử dụng lại thông qua pool, với dung lượng mặc định là 4 phần tử.
    /// </summary>
    public static partial class Simulation
    {

        static HeapQueue<Event> eventQueue = new HeapQueue<Event>(); // Hàng đợi ưu tiên (heap) chứa các sự kiện
        static Dictionary<System.Type, Stack<Event>> eventPools = new Dictionary<System.Type, Stack<Event>>(); // Thư mục chứa pool cho mỗi loại sự kiện
        static readonly object queueLock = new object();
        /// <summary>
        /// Tạo một sự kiện mới của kiểu T và trả về nó mà không lên lịch cho sự kiện.
        /// </summary>
        /// <typeparam name="T">Kiểu của sự kiện cần tạo.</typeparam>
        /// <returns>Phần tử sự kiện mới tạo.</returns>
        static public T New<T>() where T : Event, new()
        {
            Stack<Event> pool;
            // Kiểm tra xem đã có pool cho kiểu sự kiện này chưa
            if (!eventPools.TryGetValue(typeof(T), out pool))
            {
                pool = new Stack<Event>(4); // Tạo pool với dung lượng tối thiểu là 4
                pool.Push(new T()); // Đẩy một phần tử sự kiện mới vào pool
                eventPools[typeof(T)] = pool; // Lưu pool vào dictionary
            }
            if (pool.Count > 0)
                return (T)pool.Pop(); // Nếu pool không rỗng, lấy một phần tử sự kiện từ pool
            else
                return new T(); // Nếu pool rỗng, tạo mới sự kiện
        }

        /// <summary>
        /// Xóa tất cả các sự kiện đang chờ và đặt lại giá trị tick về 0.
        /// </summary>
        public static void Clear()
        {
            eventQueue.Clear(); // Xóa tất cả các sự kiện trong hàng đợi
        }

        /// <summary>
        /// Lên lịch cho một sự kiện vào một tick trong tương lai và trả về sự kiện đó.
        /// </summary>
        /// <returns>Sự kiện đã được lên lịch.</returns>
        /// <param name="tick">Thời gian của tick trong tương lai.</param>
        /// <typeparam name="T">Kiểu sự kiện cần lên lịch.</typeparam>
        static public T Schedule<T>(float tick = 0) where T : Event, new()
        {
            var ev = New<T>(); // Tạo một sự kiện mới
            ev.tick = Time.time + tick; // Đặt thời gian tick cho sự kiện
            eventQueue.Push(ev);// Đẩy sự kiện vào hàng đợi
            return ev; // Trả về sự kiện đã lên lịch
        }

        /// <summary>
        /// Lên lịch lại một sự kiện đã tồn tại vào một tick trong tương lai và trả về sự kiện đó.
        /// </summary>
        /// <returns>Sự kiện đã được lên lịch lại.</returns>
        /// <param name="tick">Thời gian của tick trong tương lai.</param>
        /// <typeparam name="T">Kiểu sự kiện cần lên lịch lại.</typeparam>
        static public T Reschedule<T>(T ev, float tick) where T : Event, new()
        {
            ev.tick = Time.time + tick; // Đặt lại thời gian tick cho sự kiện
            eventQueue.Push(ev);// Đẩy sự kiện vào hàng đợi
            return ev; // Trả về sự kiện đã được lên lịch lại
        }

        /// <summary>
        /// Trả về thể hiện (instance) của mô hình mô phỏng cho một lớp cụ thể.
        /// </summary>
        /// <typeparam name="T">Kiểu lớp mô hình.</typeparam>
        static public T GetModel<T>() where T : class, new()
        {
            return InstanceRegister<T>.instance; // Trả về thể hiện của lớp đã đăng ký
        }

        /// <summary>
        /// Cài đặt thể hiện mô hình cho một lớp cụ thể.
        /// </summary>
        /// <typeparam name="T">Kiểu lớp mô hình.</typeparam>
        static public void SetModel<T>(T instance) where T : class, new()
        {
            InstanceRegister<T>.instance = instance; // Cài đặt thể hiện cho lớp
        }

        /// <summary>
        /// Hủy thể hiện mô hình cho một lớp cụ thể.
        /// </summary>
        /// <typeparam name="T">Kiểu lớp mô hình.</typeparam>
        static public void DestroyModel<T>() where T : class, new()
        {
            InstanceRegister<T>.instance = null; // Hủy thể hiện của lớp
        }

        /// <summary>
        /// Tiến hành mô phỏng (tick) và trả về số sự kiện còn lại.
        /// Nếu không còn sự kiện nào, mô phỏng sẽ kết thúc trừ khi có sự kiện mới được lên lịch.
        /// </summary>
        /// <returns>Số lượng sự kiện còn lại trong hàng đợi.</returns>
        static public int Tick()
        {
            var time = Time.time; // Lấy thời gian hiện tại
            var executedEventCount = 0; // Đếm số sự kiện đã được thực thi
            while (eventQueue.Count > 0 && eventQueue.Peek().tick <= time) // Kiểm tra và thực thi các sự kiện đã đến thời gian
            {
                var ev = eventQueue.Pop(); // Lấy sự kiện đầu tiên trong hàng đợi
                var tick = ev.tick; // Lưu thời gian của sự kiện
                ev.ExecuteEvent(); // Thực thi sự kiện
                if (ev.tick > tick)
                {
                    // Nếu sự kiện đã được lên lịch lại, không trả lại vào pool
                }
                else
                {
                    ev.Cleanup(); // Dọn dẹp tài nguyên của sự kiện
                    try
                    {
                        eventPools[ev.GetType()].Push(ev); // Đẩy sự kiện vào pool
                    }
                    catch (KeyNotFoundException)
                    {
                        // Nếu không tìm thấy pool cho kiểu sự kiện này
                        //Debug.LogError($"No Pool for: {ev.GetType()}");
                    }
                }
                executedEventCount++; // Tăng số sự kiện đã thực thi
            }
            return eventQueue.Count; // Trả về số lượng sự kiện còn lại trong hàng đợi
        }
    }
}
