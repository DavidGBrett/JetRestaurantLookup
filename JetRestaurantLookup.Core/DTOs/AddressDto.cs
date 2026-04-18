namespace JetRestaurantLookup.Core.Dtos
{
    public record AddressDto
    {
        public required string City { get; init; }
        public required string FirstLine { get; init; }
        public required string PostalCode { get; init; }
    }
}
