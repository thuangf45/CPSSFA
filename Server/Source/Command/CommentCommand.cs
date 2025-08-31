using LuciferCore.Interface;
using Server.Source.Database;
using static LuciferCore.Core.Simulation;

namespace Server.Source.Command.CommentCommand
{
    public record CommandCreateComment(string userId, string reviewId, string content) : ICommand<int>
    {
        public int Handle() => GetModel<CommentDatabase>().CreateComment(this);
    }
    public record CommandGetComment(string commentId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<CommentDatabase>().GetComment(this);
    }
    public record CommandGetCommentByUser(string userId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<CommentDatabase>().GetCommentByUser(this);
    }

    public record CommandGetCommentByReview(string reviewId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<CommentDatabase>().GetCommentByReview(this);
    }

    public record CommandSetComment(string userId, string reviewId, string commentId, string content) : ICommand<int>
    {
        public int Handle() => GetModel<CommentDatabase>().SetComment(this);
    }

    public record CommandDeleteComment(string userId, string commentId, string reviewId) : ICommand<int>
    {
        public int Handle() => GetModel<CommentDatabase>().DeleteComment(this);
    }
}
