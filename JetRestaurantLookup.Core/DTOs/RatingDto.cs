namespace JetRestaurantLookup.Core.Dtos
{
    public record RatingDto
    {
        public required int Count { get; init; }
        public required double StarRating { get; init; }
    }
}
