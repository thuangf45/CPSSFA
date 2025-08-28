namespace Server.Source.Core
{
    public static partial class Simulation
    {
        /// <summary>
        /// Sự kiện là một hành động xảy ra tại một thời điểm nhất định trong mô phỏng.
        /// Phương thức Precondition được sử dụng để kiểm tra xem sự kiện có nên được thực thi hay không,
        /// vì các điều kiện có thể đã thay đổi trong mô phỏng kể từ khi sự kiện được lên lịch ban đầu.
        /// </summary>
        /// <typeparam name="Event">Loại sự kiện.</typeparam>
        public abstract class Event : System.IComparable<Event>
        {
            internal float tick; // Thời gian (tick) của sự kiện

            /// <summary>
            /// So sánh hai sự kiện dựa trên thời gian (tick).
            /// </summary>
            /// <param name="other">Sự kiện khác cần so sánh.</param>
            /// <returns>Kết quả so sánh thời gian giữa hai sự kiện.</returns>
            public int CompareTo(Event other)
            {
                return tick.CompareTo(other.tick); // So sánh thời gian (tick) giữa hai sự kiện
            }

            /// <summary>
            /// Thực thi sự kiện. Phương thức này sẽ được cụ thể hóa trong lớp con.
            /// </summary>
            public abstract void Execute();

            /// <summary>
            /// Phương thức kiểm tra điều kiện tiền đề cho sự kiện.
            /// Mặc định là true, có thể được ghi đè trong các lớp con để kiểm tra điều kiện cụ thể.
            /// </summary>
            public virtual bool Precondition() => true;

            /// <summary>
            /// Phương thức thực thi sự kiện nếu điều kiện tiền đề đúng.
            /// </summary>
            internal virtual void ExecuteEvent()
            {
                if (Precondition()) // Nếu điều kiện tiền đề thỏa mãn
                    Execute(); // Thực thi sự kiện
            }

            /// <summary>
            /// Phương thức dọn dẹp tài nguyên khi sự kiện đã hoàn thành.
            /// Được gọi tự động bởi Simulation sau khi sự kiện đã thực thi xong.
            /// </summary>
            internal virtual void Cleanup()
            {
                // Tùy thuộc vào sự kiện, có thể ghi đè để dọn dẹp tài nguyên.
            }
        }

        /// <summary>
        /// Event<T> bổ sung khả năng gọi callback OnExecute mỗi khi sự kiện được thực thi.
        /// Lớp này cho phép bạn thêm chức năng vào ứng dụng của mình mà không cần cấu hình phức tạp.
        /// </summary>
        /// <typeparam name="T">Kiểu sự kiện con kế thừa Event<T>.</typeparam>
        public abstract class Event<T> : Event where T : Event<T>
        {
            public static System.Action<T> OnExecute; // Callback khi sự kiện được thực thi

            /// <summary>
            /// Phương thức thực thi sự kiện và gọi callback OnExecute nếu điều kiện tiền đề thỏa mãn.
            /// </summary>
            internal override void ExecuteEvent()
            {
                if (Precondition()) // Nếu điều kiện tiền đề thỏa mãn
                {
                    Execute(); // Thực thi sự kiện
                    OnExecute?.Invoke((T)this); // Gọi callback OnExecute, nếu có
                }
            }
        }
    }
}
