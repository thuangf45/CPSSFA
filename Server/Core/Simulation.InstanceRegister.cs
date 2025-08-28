namespace Server.Source.Core
{
    public static partial class Simulation
    {
        /// <summary>
        /// Lớp này cung cấp một container để tạo các đối tượng singleton cho bất kỳ lớp nào,
        /// trong phạm vi của mô phỏng. Nó thường được sử dụng để chứa các mô hình mô phỏng
        /// và các lớp cấu hình.
        /// </summary>
        /// <typeparam name="T">Kiểu lớp muốn tạo singleton.</typeparam>
        static class InstanceRegister<T> where T : class, new()
        {
            // Đối tượng singleton duy nhất của kiểu T.
            public static T instance = new T();
        }
    }
}
