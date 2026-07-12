using Bmw.Dashboard.Core.Models.API;

namespace Bmw.Dashboard.Core.Data.Entities;

public class ContainerEntity
{
    public string ContainerID { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string ContainerPurpose { get; set; } = string.Empty;
    public ContainerState State { get; set; }
    public DateTime Created { get; set; }
}
