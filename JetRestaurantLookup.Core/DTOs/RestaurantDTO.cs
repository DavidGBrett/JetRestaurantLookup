namespace JetRestaurantLookup.Core.Dtos
{
    public record RestaurantDto
    {
        public required string Id { get; init; }
        public required string UniqueName { get; init; }
        public required string Name { get; init; }
        public required string LogoUrl { get; init; }
        public required AddressDto Address { get; init; }
        public required RatingDto Rating { get; init; }
        public required List<CuisineDto> Cuisines { get; init; }
    }
}