namespace Bmw.Dashboard.Core.Data.Entities;

public record UserConfigEntity
{
    public int Id { get; set; }
    public string ClientId { get; set; } = "Example Client ID";
    public string VehicleVin { get; set; } = "Example VIN";
    public DateTime LastUpdated { get; set; }
}
