using LuciferCore.NetCoreServer;
using System.Text;

namespace LuciferCore.Extra
{
    /// <summary>
    /// Lớp mở rộng từ <see cref="HttpRequest"/> cho phép tạo bản sao bất biến (immutable copy) từ một đối tượng HttpRequest gốc.
    /// Giúp lưu lại trạng thái của một request tại thời điểm cụ thể, tránh ảnh hưởng bởi các thay đổi sau đó.
    /// </summary>
    public class HttpRequestCopy : HttpRequest
    {
        private readonly string _urlCopy;
        private readonly string _methodCopy;
        private readonly string _protocolCopy;
        private readonly string _bodyCopy;
        private readonly long _bodyLengthCopy;
        private readonly List<(string, string)> _headersCopy;
        private readonly List<(string, string)> _cookiesCopy;

        /// <summary>
        /// Khởi tạo một bản sao bất biến từ một đối tượng <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="source">Đối tượng HttpRequest nguồn để sao chép.</param>
        /// <exception cref="ArgumentNullException">Ném ra nếu source là null.</exception>
        public HttpRequestCopy(HttpRequest source) : base()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _urlCopy = source.Url ?? string.Empty;
            _methodCopy = source.Method ?? string.Empty;
            _protocolCopy = source.Protocol ?? string.Empty;
            _bodyCopy = source.Body ?? string.Empty;
            _bodyLengthCopy = source.BodyLength;
            _headersCopy = new List<(string, string)>();
            _cookiesCopy = new List<(string, string)>();

            for (int i = 0; i < source.Headers; i++)
            {
                _headersCopy.Add(source.Header(i));
            }

            for (int i = 0; i < source.Cookies; i++)
            {
                _cookiesCopy.Add(source.Cookie(i));
            }

            SetBegin(_methodCopy, _urlCopy, _protocolCopy);
            foreach (var header in _headersCopy)
            {
                SetHeader(header.Item1, header.Item2);
            }
            foreach (var cookie in _cookiesCopy)
            {
                SetCookie(cookie.Item1, cookie.Item2);
            }
            SetBody(_bodyCopy);
        }

        /// <summary>
        /// Đường dẫn URL gốc được sao chép.
        /// </summary>
        public new string Url => _urlCopy;

        /// <summary>
        /// Phương thức HTTP gốc (GET, POST, v.v.) được sao chép.
        /// </summary>
        public new string Method => _methodCopy;

        /// <summary>
        /// Phiên bản giao thức HTTP được sao chép (ví dụ: HTTP/1.1).
        /// </summary>
        public new string Protocol => _protocolCopy;

        /// <summary>
        /// Dữ liệu body gốc được sao chép.
        /// </summary>
        public new string Body => _bodyCopy;

        /// <summary>
        /// Độ dài của body request.
        /// </summary>
        public new long BodyLength => _bodyLengthCopy;

        /// <summary>
        /// Tổng số header đã sao chép.
        /// </summary>
        public new long Headers => _headersCopy.Count;

        /// <summary>
        /// Tổng số cookie đã sao chép.
        /// </summary>
        public new long Cookies => _cookiesCopy.Count;

        /// <summary>
        /// Trả về header tại vị trí chỉ định.
        /// </summary>
        /// <param name="i">Chỉ số của header.</param>
        /// <returns>Cặp (key, value) nếu hợp lệ, hoặc ("", "") nếu vượt giới hạn.</returns>
        public new (string, string) Header(int i)
        {
            if (i < 0 || i >= _headersCopy.Count)
            {
                return ("", "");
            }
            return _headersCopy[i];
        }

        /// <summary>
        /// Trả về cookie tại vị trí chỉ định.
        /// </summary>
        /// <param name="i">Chỉ số của cookie.</param>
        /// <returns>Cặp (key, value) nếu hợp lệ, hoặc ("", "") nếu vượt giới hạn.</returns>
        public new (string, string) Cookie(int i)
        {
            if (i < 0 || i >= _cookiesCopy.Count)
            {
                return ("", "");
            }
            return _cookiesCopy[i];
        }

        /// <summary>
        /// Xuất thông tin đầy đủ của request copy ra chuỗi dạng dễ đọc (debug/log).
        /// </summary>
        /// <returns>Chuỗi mô tả nội dung request.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Request method: {Method}");
            sb.AppendLine($"Request URL: {Url}");
            sb.AppendLine($"Request protocol: {Protocol}");
            sb.AppendLine($"Request headers: {Headers}");
            foreach (var header in _headersCopy)
            {
                sb.AppendLine($"{header.Item1} : {header.Item2}");
            }
            sb.AppendLine($"Request body: {BodyLength}");
            sb.AppendLine(Body);
            return sb.ToString();
        }
    }
}
