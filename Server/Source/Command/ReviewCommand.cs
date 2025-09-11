using LuciferCore.Interface;
using Server.Source.Database;
using static LuciferCore.Core.Simulation;

namespace Server.Source.Command.ReviewCommand
{
    public record CommandCreateReview(string userId, string gameId, string content, int rating) : ICommand<int>
    {
        public int Handle() => GetModel<ReviewDatabase>().CreateReview(this);
    }

    public record CommandGetReview(string reviewId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<ReviewDatabase>().GetReview(this);
    }

    public record CommandGetReviewByUser(string userId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<ReviewDatabase>().GetReviewByUser(this);
    }

    public record CommandGetReviewByGame(string gameId) : ICommand<List<Dictionary<string, object>>>
    {
        public List<Dictionary<string, object>> Handle() => GetModel<ReviewDatabase>().GetReviewByGame(this);
    }

    public record CommandSetReview(string userId, string gameId, string reviewId,  string content, int rating) : ICommand<int>
    {
        public int Handle() => GetModel<ReviewDatabase>().SetReview(this);
    }

    public record CommandDeleteReview(string userId, string reviewId) : ICommand<int>
    {
        public int Handle() => GetModel<ReviewDatabase>().DeleteReview(this);
    }

}
