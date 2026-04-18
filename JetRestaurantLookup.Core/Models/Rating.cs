namespace JetRestaurantLookup.Core.Models
{
    public record Rating
    {
        public required int Count { get; init; }
        public required double StarRating { get; init; }
    }
}
