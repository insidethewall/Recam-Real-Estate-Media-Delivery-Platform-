namespace RecamSystemApi.DTOs;

public class ListingCaseDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public int Postcode { get; set; }
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
    public double Price { get; set; }
    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public int Garages { get; set; }

    public double FloorArea { get; set; }
          
    public required PropertyType PropertyType { get; set; }

      
    public required SaleCategory SaleCategory { get; set; }
}