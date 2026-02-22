using System.Text.Json.Serialization;

namespace SimRailDispatcherUtility.Model.StationServiceModels;

public sealed class TrainRun
{
    [JsonPropertyName("trainNoLocal")]
    public string TrainNoLocal { get; set; } = null!;

    [JsonPropertyName("trainNoInternational")]
    public string TrainNoInternational { get; set; } = null!;

    [JsonPropertyName("trainName")]
    public string TrainName { get; set; } = null!;

    [JsonPropertyName("startStation")]
    public string StartStation { get; set; } = null!;

    [JsonPropertyName("startsAt")]
    public string StartsAt { get; set; } = null!;

    [JsonPropertyName("endStation")]
    public string EndStation { get; set; } = null!;

    [JsonPropertyName("endsAt")]
    public string EndsAt { get; set; } = null!;

    [JsonPropertyName("locoType")]
    public string LocoType { get; set; } = null!;

    [JsonPropertyName("trainLength")]
    public int TrainLength { get; set; }

    [JsonPropertyName("trainWeight")]
    public int TrainWeight { get; set; }

    [JsonPropertyName("continuesAs")]
    public string ContinuesAs { get; set; } = null!;

    [JsonPropertyName("runId")]
    public string RunId { get; set; } = null!;

    [JsonPropertyName("timetable")]
    public List<TimetablePoint> Timetable { get; set; } = new();
}