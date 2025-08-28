namespace LuciferCore.Interface
{
    /// <summary>
    /// Interface định nghĩa các thao tác cơ bản cho lớp quản lý cơ sở dữ liệu.
    /// Gồm truy xuất instance, lấy dữ liệu theo ID và xóa dữ liệu.
    /// </summary>
    public interface IDatabase<T>
    {
        /// <summary>
        /// Trả về instance hiện tại của lớp triển khai cơ sở dữ liệu.
        /// </summary>
        /// <returns>Đối tượng <see cref="IDatabase"/> đại diện cho instance.</returns>
        public IDatabase<T> GetInstance();

        public int Create<T>(T data);

        /// <summary>
        /// Lấy dữ liệu từ cơ sở dữ liệu theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi cần truy xuất.</param>
        /// <returns>Đối tượng dữ liệu tìm được, hoặc null nếu không tồn tại.</returns>
        public T Read<T>(string id);

        public int Update<T>(string id, T data);

        /// <summary>
        /// Xóa một đối tượng dữ liệu ra khỏi cơ sở dữ liệu.
        /// </summary>
        /// <param name="data">Đối tượng cần xóa.</param>
        /// <returns>Số lượng bản ghi bị xóa (thường là 0 hoặc 1).</returns>
        public int Delete<T>(string id);
    }
}
