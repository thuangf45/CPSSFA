namespace Server.Source.Core
{
    /// <summary>
    /// Fuzzy cung cấp các phương thức để sử dụng giá trị với độ lệch ngẫu nhiên (fuzz), 
    /// cho phép tạo sự không chính xác trong các phép so sánh hoặc tính toán.
    /// </summary>
    static class Fuzzy
    {
        static readonly Random rnd = new Random();

        /// <summary>
        /// Kiểm tra xem giá trị 'value' có nhỏ hơn 'test' không, với độ lệch ngẫu nhiên 'fuzz'.
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="test">Giá trị so sánh</param>
        /// <param name="fuzz">Độ lệch ngẫu nhiên (mặc định là 0.1)</param>
        /// <returns>Trả về true nếu giá trị nhỏ hơn 'test' sau khi áp dụng fuzz, ngược lại trả về false.</returns>
        public static bool ValueLessThan(float value, float test, float fuzz = 0.1f)
        {
            var delta = value - test; // Tính sự chênh lệch giữa 'value' và 'test'
            return delta < 0 ? true : rnd.NextDouble() > delta / (fuzz * test); // Nếu delta < 0 trả về true, nếu không thì so sánh ngẫu nhiên
        }

        /// <summary>
        /// Kiểm tra xem giá trị 'value' có lớn hơn 'test' không, với độ lệch ngẫu nhiên 'fuzz'.
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="test">Giá trị so sánh</param>
        /// <param name="fuzz">Độ lệch ngẫu nhiên (mặc định là 0.1)</param>
        /// <returns>Trả về true nếu giá trị lớn hơn 'test' sau khi áp dụng fuzz, ngược lại trả về false.</returns>
        public static bool ValueGreaterThan(float value, float test, float fuzz = 0.1f)
        {
            var delta = value - test; // Tính sự chênh lệch giữa 'value' và 'test'
            return delta < 0 ? rnd.NextDouble() > -1 * delta / (fuzz * test) : true; // Nếu delta < 0 thì so sánh ngẫu nhiên, nếu không trả về true
        }

        /// <summary>
        /// Kiểm tra xem giá trị 'value' có gần bằng giá trị 'test' trong phạm vi độ lệch ngẫu nhiên 'fuzz' hay không.
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="test">Giá trị so sánh</param>
        /// <param name="fuzz">Độ lệch ngẫu nhiên (mặc định là 0.1)</param>
        /// <returns>Trả về true nếu tỷ lệ giữa 'value' và 'test' gần bằng 1 trong phạm vi 'fuzz', ngược lại trả về false.</returns>
        public static bool ValueNear(float value, float test, float fuzz = 0.1f)
        {
            return Math.Abs(1f - value / test) < fuzz; // So sánh tỷ lệ giữa 'value' và 'test' với phạm vi fuzz
        }

        /// <summary>
        /// Trả về giá trị 'value' cộng thêm một độ lệch ngẫu nhiên trong phạm vi 'fuzz'.
        /// </summary>
        /// <param name="value">Giá trị cần thêm độ lệch ngẫu nhiên</param>
        /// <param name="fuzz">Độ lệch ngẫu nhiên (mặc định là 0.1)</param>
        /// <returns>Trả về giá trị với độ lệch ngẫu nhiên.</returns>
        public static float Value(float value, float fuzz = 0.1f)
        {
            return value + value * RandomRange(-fuzz, +fuzz); // Sinh ra một độ lệch ngẫu nhiên và cộng vào giá trị 'value'
        }
        static float RandomRange(float min, float max)
        {
            return (float)(rnd.NextDouble() * (max - min) + min);
        }

    }
}
