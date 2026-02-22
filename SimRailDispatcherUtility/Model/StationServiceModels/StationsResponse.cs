using System.Text.Json.Serialization;

namespace SimRailDispatcherUtility.Model.StationServiceModels;

public sealed class StationsResponse
{
    [JsonPropertyName("result")]
    public bool Result { get; set; }

    [JsonPropertyName("data")]
    public List<Station> Data { get; set; } = new();
}