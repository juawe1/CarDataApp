namespace Bmw.Dashboard.Core.Data.Entities;

public record UserConfigEntity
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string VehicleVin { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
