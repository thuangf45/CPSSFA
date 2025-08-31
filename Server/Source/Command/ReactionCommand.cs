using LuciferCore.Interface;
using Server.Source.Database;
using static LuciferCore.Core.Simulation;

namespace Server.Source.Command.ReactionCommand
{
    public record CommandCreateReactionForComment(string userId, string commentId, int reactionType) : ICommand<int>
    {
        public int Handle() => GetModel<ReactionDatabase>().CreateReactionForComment(this);
    }

    public record CommandCreateReactionForReview(string userId, string reviewId, int reactionType) : ICommand<int>
    {
        public int Handle() => GetModel<ReactionDatabase>().CreateReactionForReview(this);
    }

    public record CommandDeleteReactionComment(string userId, string commentId, string reactionId) : ICommand<int>
    {
        public int Handle() => GetModel<ReactionDatabase>().DeleteReactionComment(this);
    }

    public record CommandDeleteReactionReview(string userId, string reviewId, string reactionId) : ICommand<int>
    {
        public int Handle() => GetModel<ReactionDatabase>().DeleteReactionReview(this);
    }

    public record CommandSetReactionComment(string userId, string commentId, string reactionId, int reactionType) : ICommand<int>
    {
        public int Handle() => GetModel<ReactionDatabase>().SetReactionComment(this);
    }

    public record CommandSetReactionReview(string userId, string reviewId, string reactionId, int reactionType) : ICommand<int>
    {
        public int Handle() => GetModel<ReactionDatabase>().SetReactionReview(this);
    }

    public record CommandGetReactionByComment(string commentId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<ReactionDatabase>().GetReactionByComment(this);
    }

    public record CommandGetReactionByReview(string reviewId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<ReactionDatabase>().GetReactionByReview(this);
    }

    public record CommandGetReactionByUser(string userId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<ReactionDatabase>().GetReactionByUser(this);
    }
}
