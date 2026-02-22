using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimRailDispatcherUtility.Model.StationServiceModels;

public sealed class Station
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = null!;

    [JsonPropertyName("DifficultyLevel")]
    public int DifficultyLevel { get; set; }

    // API key is misspelled
    [JsonPropertyName("Latititude")]
    public double Latitude { get; set; }

    [JsonPropertyName("Longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("MainImageURL")]
    public string MainImageUrl { get; set; } = null!;

    [JsonPropertyName("AdditionalImage1URL")]
    public string AdditionalImage1Url { get; set; } = null!;

    [JsonPropertyName("AdditionalImage2URL")]
    public string AdditionalImage2Url { get; set; } = null!;

    [JsonPropertyName("DispatchedBy")]
    public JsonElement DispatchedBy { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
}