namespace Bmw.Dashboard.Core.Models;

public class VehicleDisplayModel
{
    public required string Vin { get; set; }
    public required string MappingType { get; set; }
    public string? ImagePath { get; set; }

    public bool IsPrimary =>
        string.Equals(MappingType, "PRIMARY", StringComparison.OrdinalIgnoreCase);
}
