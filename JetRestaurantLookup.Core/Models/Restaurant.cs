namespace JetRestaurantLookup.Core.Models
{
    public record Restaurant
    {
        public required string Id {get; init;}
        public required string Name {get; init;}
        public required Address Address {get; init;}
        public required Rating Rating {get; init;}
        public required List<string> Cuisines { get; init; }
    }
        
}