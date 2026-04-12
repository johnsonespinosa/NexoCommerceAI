namespace NexoCommerceAI.Domain.ValueObjects;

public record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }
    
    public Address(string street, string city, string? state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode is required", nameof(zipCode));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));
        
        Street = street;
        City = city;
        State = state ?? string.Empty;
        ZipCode = zipCode;
        Country = country;
    }
    
    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}