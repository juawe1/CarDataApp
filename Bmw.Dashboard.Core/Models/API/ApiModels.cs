using System.Text.Json.Serialization;

namespace Bmw.Dashboard.Core.Models.API;

public enum MappingType
{
    PRIMARY = 0,
    SECONDARY = 1
}

public enum ContainerState
{
    ACTIVE = 0,
    DELETED = 1
}

public record VehicleMappingResponse
{
    [JsonPropertyName("vin")] public required string Vin { get; set; }
    [JsonPropertyName("mappedSince")] public DateTime MappedSince { get; set; }
    [JsonPropertyName("mappingType")] public required string MappingType { get; set; }
}

public record ContainerResponse
{
    [JsonPropertyName("containerId")] public required string ContainerID { get; set; }
    [JsonPropertyName("name")] public required string ContainerName { get; set; }
    [JsonPropertyName("purpose")] public required string ContainerPurpose { get; set; }
    [JsonPropertyName("state")] public ContainerState State { get; set; }
    [JsonPropertyName("created")] public DateTime Created { get; set; }
}

public record ContainerListResponse
{
    [JsonPropertyName("containers")] public List<ContainerResponse> Containers { get; set; } = [];
}