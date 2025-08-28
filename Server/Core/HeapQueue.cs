using System;
using System.Collections.Generic;

namespace Server.Source.Core
{
    /// <summary>
    /// HeapQueue cung cấp một hàng đợi (queue) luôn được sắp xếp theo một thứ tự nhất định, 
    /// sử dụng cấu trúc heap (cây nhị phân) để duy trì tính chất của hàng đợi.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu được so sánh trong hàng đợi (phải implements IComparable)</typeparam>
    public class HeapQueue<T> where T : IComparable<T>
    {
        List<T> items; // Danh sách lưu trữ các phần tử trong heap

        public int Count { get { return items.Count; } } // Số lượng phần tử trong heap

        public bool IsEmpty { get { return items.Count == 0; } } // Kiểm tra xem heap có rỗng không

        public T First { get { return items[0]; } } // Lấy phần tử đầu tiên trong heap (min heap)

        public void Clear() => items.Clear(); // Xóa tất cả các phần tử trong heap

        public bool Contains(T item) => items.Contains(item); // Kiểm tra xem phần tử có trong heap hay không

        public void Remove(T item) => items.Remove(item); // Xóa một phần tử khỏi heap

        public T Peek() => items[0]; // Xem phần tử đầu tiên trong heap mà không lấy ra

        public HeapQueue()
        {
            items = new List<T>(); // Khởi tạo danh sách chứa phần tử
        }

        /// <summary>
        /// Thêm phần tử vào heap và duy trì cấu trúc heap.
        /// </summary>
        /// <param name="item">Phần tử cần thêm vào heap</param>
        public void Push(T item)
        {
            items.Add(item); // Thêm phần tử vào cuối danh sách
            SiftDown(0, items.Count - 1); // Tìm vị trí đúng của phần tử trong heap
        }

        /// <summary>
        /// Lấy và loại bỏ phần tử đầu tiên (min heap).
        /// </summary>
        /// <returns>Phần tử đầu tiên trong heap</returns>
        public T Pop()
        {
            T item;
            var last = items[items.Count - 1]; // Lấy phần tử cuối cùng trong heap
            items.RemoveAt(items.Count - 1); // Loại bỏ phần tử cuối cùng
            if (items.Count > 0)
            {
                item = items[0];
                items[0] = last; // Đưa phần tử cuối cùng lên vị trí đầu tiên
                SiftUp(); // Duy trì cấu trúc heap
            }
            else
            {
                item = last;
            }
            return item; // Trả về phần tử đầu tiên đã lấy
        }

        // Hàm so sánh giữa hai phần tử trong heap
        int Compare(T A, T B) => A.CompareTo(B);

        /// <summary>
        /// Duy trì cấu trúc heap bằng cách "sàng xuống" (sift down) từ một vị trí cụ thể.
        /// </summary>
        /// <param name="startpos">Vị trí bắt đầu sàng xuống</param>
        /// <param name="pos">Vị trí cần sàng xuống</param>
        void SiftDown(int startpos, int pos)
        {
            var newitem = items[pos]; // Lưu lại phần tử cần di chuyển
            while (pos > startpos)
            {
                var parentpos = pos - 1 >> 1; // Tính toán vị trí của parent trong cây nhị phân
                var parent = items[parentpos];
                // Nếu phần tử mới đã đúng vị trí so với parent thì thoát vòng lặp
                if (Compare(parent, newitem) <= 0)
                    break;
                // Di chuyển phần tử cha xuống vị trí hiện tại
                items[pos] = parent;
                pos = parentpos; // Cập nhật lại vị trí cho tiếp tục sàng xuống
            }
            items[pos] = newitem; // Đặt phần tử vào vị trí đúng
        }

        /// <summary>
        /// Duy trì cấu trúc heap bằng cách "sàng lên" (sift up) phần tử tại đầu heap.
        /// </summary>
        void SiftUp()
        {
            var endpos = items.Count;
            var startpos = 0;
            var newitem = items[0]; // Lưu lại phần tử đầu tiên cần di chuyển
            var childpos = 1;
            var pos = 0;
            while (childpos < endpos)
            {
                var rightpos = childpos + 1; // Tính vị trí nhánh phải của node hiện tại
                if (rightpos < endpos && Compare(items[rightpos], items[childpos]) <= 0)
                    childpos = rightpos; // Nếu nhánh phải nhỏ hơn hoặc bằng nhánh trái thì chọn nhánh phải
                items[pos] = items[childpos]; // Di chuyển phần tử con lên vị trí của phần tử hiện tại
                pos = childpos; // Cập nhật lại vị trí hiện tại
                childpos = 2 * pos + 1; // Tính vị trí con
            }
            items[pos] = newitem; // Đặt phần tử vào vị trí đúng
            SiftDown(startpos, pos); // Duy trì cấu trúc heap bằng cách sàng xuống
        }
    }
}
