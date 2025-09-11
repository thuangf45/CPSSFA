namespace Server.LuciferCore.Model
{
    /// <summary>
    /// Lớp cơ sở cho các yêu cầu xóa, chứa UserId của người gửi yêu cầu.
    /// </summary>
    public class DeleteBaseParams
    {
        public string UserId { get; set; }
    }

    /// <summary>
    /// Yêu cầu xóa tài khoản, bao gồm UserId và mật khẩu xác minh.
    /// </summary>
    public class DeleteAccountParams : DeleteBaseParams
    {
        public string Password { get; set; }
    }

    /// <summary>
    /// Yêu cầu xóa một mục tiêu cụ thể (ví dụ bài viết, bình luận,...), bao gồm UserId và TargetId.
    /// </summary>
    public class DeleteReviewParams : DeleteBaseParams
    {
        public string GameId { get; set; }
        public string ReviewId { get; set; }
    }
    public class DeleteCommentParams : DeleteBaseParams
    {
        public string ReviewId { get; set; }
        public string CommentId { get; set; }
    }
    public class DeleteReactionCParams : DeleteBaseParams
    {
        public string CommentId { get; set; }
        public string ReactionId { get; set; }
    }
    public class DeleteReactionRParams : DeleteBaseParams
    {
        public string ReviewId { get; set; }
        public string ReactionId { get; set; }
    }

    /// <summary>
    /// Đối tượng mang theo một giá trị Id đơn giản.
    /// </summary>
    public class IdParams { }

    public class UserIdParams : IdParams
    {
        public UserIdParams(string userId)
        {
            UserId = userId;
        }
        public string UserId { get; set; }
    }

    public class ReviewIdParams : IdParams
    {
        public ReviewIdParams(string reviewId)
        {
            ReviewId = reviewId;
        }
        public string ReviewId { get; set; }
    }

    public class GameIdParams : IdParams
    {
        public GameIdParams(string gameId)
        {
            GameId = gameId;
        }
        public string GameId { get; set; }
    }

    public class CommentIdParams : IdParams
    {
        public CommentIdParams(string commentId)
        {
            CommentId = commentId;
        }
        public string CommentId { get; set; }
    }

    public class ReactionIdParams : IdParams
    {
        public ReactionIdParams(string reactionId)
        {
            ReactionId = reactionId;
        }
        public string ReactionId { get; set; }
    }


    /// <summary>
    /// Yêu cầu thay đổi mật khẩu, bao gồm mật khẩu cũ và mật khẩu mới.
    /// </summary>
    public class ChangePasswordParams
    {
        public string UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UsernameParams
    {
        public string Username { get; set; }
    }
    /// <summary>
    /// Yêu cầu thay đổi tên người dùng (username).
    /// </summary>
    public class ChangeUsernameParams
    {
        public string UserId { get; set; }
        public string Username { get; set; }
    }

    /// <summary>
    /// Yêu cầu khôi phục mật khẩu chỉ dựa trên email.
    /// </summary>
    public class ForgetPasswordParams
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// Yêu cầu thay đổi địa chỉ email.
    /// </summary>
    public class ChangeEmailParams
    {
        public string UserId { get; set; }
        public string NewEmail { get; set; }
    }

    /// <summary>
    /// Yêu cầu thay đổi ảnh đại diện (avatar).
    /// </summary>
    public class ChangeAvatarParams
    {
        public string UserId { get; set; }
        public string Avatar { get; set; }
    }

    public class FollowParam
    {
        public string UserId { get; set; }
        public string Target { get; set; }
    }

    public class PaginateParams
    {
        public int Page { get; set; }
        public int Limit { get; set; }
    }

    public class UserPaginateParams : PaginateParams
    {
        public string UserId { get; set; }
    }


    public class UploadedFile
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }

        public string Name { get; set; } // key của input file
    }

    /// <summary>
    /// Yêu cầu gửi email, chứa đầy đủ thông tin người gửi, người nhận và nội dung.
    /// </summary>
    public class EmailSendRequest
    {
        /// <summary>
        /// Email dùng để đăng nhập SMTP (sẽ kết hợp với mật khẩu từ cấu hình NotifyManager).
        /// </summary>
        public string SmtpUser { get; set; } = default!;

        /// <summary>
        /// Email hiển thị là người gửi.
        /// </summary>
        public string FromEmail { get; set; } = default!;

        /// <summary>
        /// Email người nhận.
        /// </summary>
        public string ToEmail { get; set; } = default!;

        /// <summary>
        /// Tiêu đề email.
        /// </summary>
        public string Subject { get; set; } = default!;

        /// <summary>
        /// Nội dung email.
        /// </summary>
        public string Body { get; set; } = default!;

        /// <summary>
        /// Cho biết nội dung email có phải là HTML hay không.
        /// </summary>
        public bool IsHtml { get; set; } = false;

        /// <summary>
        /// Danh sách đường dẫn tuyệt đối tới các tệp đính kèm.
        /// </summary>
        public List<string> Attachments { get; set; } = new();
    }

}
