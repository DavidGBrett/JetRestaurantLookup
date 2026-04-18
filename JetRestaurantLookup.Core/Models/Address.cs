
namespace JetRestaurantLookup.Core.Models
{
    public record Address
    {
        public required string City { get; init; }
        public required string FirstLine { get; init; }
        public required string PostalCode { get; init; }
    }
}
