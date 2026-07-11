namespace Bmw.Dashboard.Core.Data.Entities;

public class TripRecordEntity
{
    public int Id { get; set; }
    public string? ExternalTripId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DistanceTraveled { get; set; }
    public double AverageFuelConsumption { get; set; }
}
