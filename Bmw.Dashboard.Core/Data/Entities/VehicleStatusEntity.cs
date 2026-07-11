namespace Bmw.Dashboard.Core.Data.Entities;

public class VehicleStatusEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    
    //core telem
    public double OdometerReading { get; set; }
    public double FuelLevelPercent { get; set; }
    public double RemainingRangeKm { get; set; }

    //status
    public string? ConnectionStatus { get; set; }
    public bool IsCharging { get; set; }
}
