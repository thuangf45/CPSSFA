using LuciferCore.Database;
using LuciferCore.Helper;
using static LuciferCore.Core.Simulation;

namespace Server.Source.Database
{
    /// <summary>
    /// Lớp quản lý truy xuất dữ liệu bảng <c>account</c> trong cơ sở dữ liệu.
    /// Cung cấp các chức năng tạo tài khoản, đăng nhập, thay đổi thông tin và khôi phục mật khẩu.
    /// </summary>
    public class AccountDatabase : BaseDatabase
    {
        /// <summary>
        /// Tạo tài khoản mới trong hệ thống.
        /// </summary>
        /// <param name="data">Dữ liệu tài khoản cần tạo (kiểu Account).</param>
        /// <returns>Chuỗi kết quả trả về từ truy vấn (có thể là ID, trạng thái, v.v.).</returns>
        public virtual string CreateAccount(object data)
        {
            string sql = "Placeholder";
            var result = Create(data, sql);
            return DataMapper.GetScalarValue<string>(result);
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập.
        /// </summary>
        /// <param name="data">Thông tin đăng nhập (username/email và password).</param>
        /// <returns>Chuỗi ID người dùng nếu thành công, null hoặc chuỗi rỗng nếu thất bại.</returns>
        public virtual string CheckLoginAccount(object data)
        {
            string sql = "Placeholder";
            var result = Read(data, sql);
            return DataMapper.GetScalarValue<string>(result);
        }

        /// <summary>
        /// Thay đổi tên người dùng (username).
        /// </summary>
        /// <param name="data">Thông tin thay đổi (kiểu ParamsChangeUsername).</param>
        /// <returns>1 nếu thành công, 0 nếu thất bại.</returns>
        public virtual int ChangeUsername(object data)
        {
            string sql = "Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        /// <summary>
        /// Thay đổi mật khẩu tài khoản.
        /// </summary>
        /// <param name="data">Thông tin thay đổi (kiểu ParamsChangePassword).</param>
        /// <returns>1 nếu thành công, 0 nếu thất bại.</returns>
        public virtual int ChangePassword(object data)
        {
            string sql = "Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<int>(result);
        }

        /// <summary>
        /// Xử lý yêu cầu quên mật khẩu.
        /// </summary>
        /// <param name="data">Thông tin người dùng để xác minh (ParamsForgetPassword).</param>
        /// <returns>1 nếu gửi thành công, 0 nếu không hợp lệ.</returns>
        public virtual string ForgetPassword(object data)
        {
            string sql = "Placeholder";
            var result = Update(data, sql);
            return DataMapper.GetScalarValue<string>(result);
        }
    }
}
