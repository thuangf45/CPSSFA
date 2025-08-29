using Server.LuciferCore.Model;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using static LuciferCore.Core.Simulation;

namespace LuciferCore.Manager
{
    /// <summary>
    /// Quản lý hệ thống gửi email nền bằng SMTP, sử dụng hàng đợi và đa luồng an toàn.
    /// Cho phép cấu hình linh hoạt người gửi (smtpUser) cho từng email.
    /// </summary>
    public class NotifyManager : ManagerBase
    {
        /// <summary>
        /// Hàng đợi các yêu cầu gửi email đang chờ xử lý.
        /// </summary>
        private readonly BlockingCollection<EmailSendRequest> _queue = new();

        /// <summary>
        /// Máy chủ SMTP (ví dụ: smtp.gmail.com cho gmail).
        /// </summary>
        private readonly string _smtpHost;

        /// <summary>
        /// Cổng SMTP (ví dụ: 587 cho TLS).
        /// </summary>
        private readonly int _smtpPort;

        /// <summary>
        /// Mật khẩu ứng dụng (app password) dùng chung để xác thực SMTP. (ví dụ: aysd pgdv lfib ldll)
        /// </summary>
        private readonly string _smtpPass;

        /// <summary>
        /// Có sử dụng SSL/TLS cho kết nối SMTP hay không.
        /// </summary>
        private readonly bool _useSsl;

        /// <summary>
        /// Sự kiện được kích hoạt khi một email được gửi thành công.
        /// </summary>
        public event Action<EmailSendRequest>? OnEmailSent;

        /// <summary>
        /// Sự kiện được kích hoạt khi gửi email thất bại.
        /// </summary>
        public event Action<EmailSendRequest, Exception>? OnEmailFailed;

        /// <summary>
        /// Khởi tạo đối tượng NotifyManager với thông tin cấu hình SMTP.
        /// </summary>
        /// <param name="smtpHost">Địa chỉ máy chủ SMTP.</param>
        /// <param name="smtpPort">Cổng SMTP.</param>
        /// <param name="smtpPass">Mật khẩu ứng dụng (SMTP password).</param>
        /// <param name="useSsl">Sử dụng SSL/TLS hay không.</param>
        public NotifyManager(string smtpHost, int smtpPort, string smtpPass, bool useSsl = true)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpPass = smtpPass;
            _useSsl = useSsl;
        }

        public NotifyManager()
        {
            _smtpHost = "smtp.gmail.com";
            _smtpPort = 587;
            _smtpPass = "aysd pgdv lfib ldll";
            _useSsl = true;
        }
        private string QuickFormatResetPassword(string password)
        {
            // Mã hóa HTML để tránh special chars phá cấu trúc HTML
            var safePassword = WebUtility.HtmlEncode(password);

            // Dùng verbatim + interpolation để dễ đọc; TrimStart() loại indent thừa
            var body = $@"
            <html>
              <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <h2 style='color: #2c3e50;'>Xin chào,</h2>
                <p>Yêu cầu reset mật khẩu của bạn đã được xử lý thành công.</p>
                <p>
                  <strong>Mật khẩu mới của bạn: 
                    <span style='color: #e74c3c;'>{safePassword}</span>
                  </strong>
                </p>
                <p>Hãy đăng nhập ngay và đổi mật khẩu để đảm bảo an toàn tài khoản.</p>
                <p style='color: #999; font-size: 12px;'>
                  Nếu bạn không yêu cầu reset mật khẩu, vui lòng bỏ qua email này.
                </p>
                <hr />
                <p style='font-size: 12px; color: #999;'>
                  &copy; 2025 Hệ thống Kontroller - Email tự động, vui lòng không trả lời.
                </p>
              </body>
            </html>".Trim(); // Trim để loại khoảng trắng ở đầu/cuối

            return body;
        }

        /// <summary>
        /// Thêm một yêu cầu gửi email vào hàng đợi.
        /// </summary>
        /// <param name="request">Thông tin email cần gửi.</param>
        public void Send(EmailSendRequest request)
        {
            _queue.Add(request);
        }
        public void SendMailResetPassword(string toEmail, string password)
        {

            var mail = new EmailSendRequest
            {
                SmtpUser = "kingnemacc@gmail.com",
                FromEmail = "kingnemacc@gmail.com",
                ToEmail = toEmail,
                Subject = "Phản hồi reset mật khẩu",
                IsHtml = true,
                Body = QuickFormatResetPassword(password)
            };
            Send(mail);
        }
        /// <summary>
        /// Vòng lặp chính xử lý gửi email nền.
        /// Lấy email từ hàng đợi, gửi qua SMTP và phát sự kiện tương ứng.
        /// </summary>
        /// <param name="token">Token để hủy tác vụ một cách an toàn.</param>
        protected override async Task Run(CancellationToken token)
        {
            foreach (var req in _queue.GetConsumingEnumerable(token))
            {
                if (token.IsCancellationRequested) break;

                using var message = new MailMessage();
                message.From = new MailAddress(req.FromEmail);
                message.To.Add(req.ToEmail);
                message.Subject = req.Subject;
                message.Body = req.Body;
                message.IsBodyHtml = req.IsHtml;

                foreach (var filePath in req.Attachments)
                {
                    if (File.Exists(filePath))
                    {
                        message.Attachments.Add(new Attachment(filePath));
                    }
                }

                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = _useSsl,
                    Credentials = new NetworkCredential(req.SmtpUser, _smtpPass)
                };

                try
                {
                    await client.SendMailAsync(message);
                    OnEmailSent?.Invoke(req);
                }
                catch (Exception ex)
                {
                    OnEmailFailed?.Invoke(req, ex);
                }

                await Task.Delay(10_000, token); // Tránh spam server
            }
        }

        protected override void OnStarted()
        {
            GetModel<LogManager>().LogSystem("⚙️ NotifyManager started");
        }

        protected override void OnStopping()
        {
            _queue.CompleteAdding();
        }

        protected override void OnStopped()
        {
            GetModel<LogManager>().LogSystem("⚙️ NotifyManager stopped");
        }
    }
}

/*
 notifyManager.Send(new EmailSendRequest
{
    SmtpUser = "your_gmail@gmail.com",
    FromEmail = "your_gmail@gmail.com",
    ToEmail = "friend@example.com",
    Subject = "Gửi email có file đính kèm",
    Body = "Đây là email có file đính kèm.",
    IsHtml = false,
    Attachments = new List<string>
    {
        @"C:\Users\thuận\Documents\bao_cao.pdf",
        @"C:\Users\thuận\Pictures\anh.png"
    }
});
 
 */