using System.Text.Json.Serialization;

namespace Bmw.Dashboard.Core.Models.API;

public enum MappingType
{
    PRIMARY,
    SECONDARY
}

public record VehicleMappingResponse
{
    public required string Vin { get; set; }
    public DateTime MappedSince { get; set; }
    public required string MappingType { get; set; }
}
