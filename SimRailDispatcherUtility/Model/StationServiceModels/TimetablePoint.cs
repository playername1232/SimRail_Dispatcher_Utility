using System.Text.Json.Serialization;

namespace SimRailDispatcherUtility.Model.StationServiceModels;

public sealed class TimetablePoint
{
    [JsonPropertyName("nameOfPoint")]
    public string NameOfPoint { get; set; } = null!;

    [JsonPropertyName("nameForPerson")]
    public string NameForPerson { get; set; } = null!;

    [JsonPropertyName("pointId")]
    public string PointId { get; set; } = null!;

    [JsonPropertyName("supervisedBy")]
    public string SupervisedBy { get; set; } = null!;

    [JsonPropertyName("radioChanels")]
    public string RadioChanels { get; set; } = null!;

    [JsonPropertyName("displayedTrainNumber")]
    public string DisplayedTrainNumber { get; set; } = null!;

    [JsonPropertyName("arrivalTime")]
    [JsonConverter(typeof(SimRailDateTimeConverter))]
    public DateTime ArrivalTime { get; set; }

    [JsonPropertyName("departureTime")]
    [JsonConverter(typeof(SimRailDateTimeConverter))]
    public DateTime DepartureTime { get; set; }

    [JsonPropertyName("stopType")]
    public string StopType { get; set; } = null!;

    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("track")]
    public int? Track { get; set; }

    [JsonPropertyName("trainType")]
    public string TrainType { get; set; } = null!;

    [JsonPropertyName("mileage")]
    public double Mileage { get; set; }

    [JsonPropertyName("maxSpeed")]
    public int MaxSpeed { get; set; }

    [JsonPropertyName("stationCategory")]
    public string? StationCategory { get; set; }
}