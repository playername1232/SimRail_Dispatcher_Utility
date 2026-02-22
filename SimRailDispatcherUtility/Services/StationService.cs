using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SimRailDispatcherUtility.Model;
using SimRailDispatcherUtility.Model.StationServiceModels;

namespace SimRailDispatcherUtility.Services;

public class StationService
{
    private ILogger<StationService> _logger;
    private HttpClient _httpClient;

    public ObservableCollection<Station> Stations { get; set; } = new();

    public ObservableCollection<string> CurrentNeighborStations { get; private set; } = new();

    public StationService(ILogger<StationService> logger, HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    /// <summary>
    /// Regenerates assets/getAllTimetables.json file
    /// </summary>
    /// <param name="serverCode">Server code - "cz1" default</param>
    /// <exception cref="NullReferenceException"></exception>
    public async Task LoadTimetablesJsonAsync(string? serverCode = null)
    {
        try
        {
            string baseUrl = GetTimetablesBaseUrl();
            var rawPath = GetTimetablesFileRawPath();
            serverCode = GetServerCode(serverCode);

            string json = await _httpClient.GetStringAsync(baseUrl + $"?serverCode={serverCode}");

            var filePath = Environment.ExpandEnvironmentVariables(rawPath);

            var dir = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException($"Invalid file path: {filePath}");

            Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

            _logger.LogInformation("Updated GetAllTimetables json file.. location: {Location}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task LoadStationsAsync(string? serverCode = null)
    {
        string baseUrl = GetStationsBaseUrl();
        serverCode = GetServerCode(serverCode);

        var serverUrl = $"{baseUrl}?serverCode={serverCode}";

        string json = await _httpClient.GetStringAsync(serverUrl);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        StationsResponse? response = JsonSerializer.Deserialize<StationsResponse>(json, options);

        if (response is null)
            throw new SerializationException("Could not deserialize response from Stations endpoint");

        if (!response.Result)
            _logger.LogWarning("Stations endpoint returned result=false");

        Stations.Clear();
        response.Data.ForEach(x =>
        {
            Stations.Add(x);
        });
    }

    public void ComputeNeighborStations(string stationName)
    {
        if (string.IsNullOrWhiteSpace(stationName))
            return;

        string json = File.ReadAllText(Environment.ExpandEnvironmentVariables(GetTimetablesFileRawPath()));

        var trains = JsonSerializer.Deserialize<List<TrainRun>>(json) ?? new List<TrainRun>();

        var neighbors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var train in trains)
        {
            var timetable = train.Timetable;
            if (timetable is null || timetable.Count == 0)
                continue;

            for (int i = 0; i < timetable.Count; i++)
            {
                if (!string.Equals(timetable[i].NameOfPoint, stationName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // previous station
                if (i - 1 >= 0)
                {
                    var prev = timetable[i - 1].NameOfPoint;
                    if (!string.IsNullOrWhiteSpace(prev) && !string.Equals(prev, stationName, StringComparison.OrdinalIgnoreCase))
                        neighbors.Add(prev);
                }

                // next station
                if (i + 1 < timetable.Count)
                {
                    var next = timetable[i + 1].NameOfPoint;
                    if (!string.IsNullOrWhiteSpace(next) && !string.Equals(next, stationName, StringComparison.OrdinalIgnoreCase))
                        neighbors.Add(next);
                }
            }
        }

        CurrentNeighborStations.Clear();
        foreach (var n in neighbors.OrderBy(x => x))
            CurrentNeighborStations.Add(n);

        foreach (var currentNeighborStation in CurrentNeighborStations)
        {
            _logger.LogInformation("For station {Station} loaded Neighbor: {Neighbor}", stationName, currentNeighborStation);
        }
    }

    private string GetStationsBaseUrl()
    {
        return AppConfig.Config["SimRailApi:StationsOpenApiUrl"] ?? throw new NullReferenceException("App config does not define SimRailApi:StationsOpenApiUrl");
    }

    private string GetTimetablesBaseUrl()
    {
        return AppConfig.Config["SimRailApi:BaseTimeTableApiUrl"] ?? throw new NullReferenceException("App config does not define SimRailApi:BaseTimeTableApiUrl");
    }

    private string GetTimetablesFileRawPath()
    {
        return AppConfig.Config["GetAllTimetableDataLocation"] ?? throw new NullReferenceException("App config does not define GetAllTimetableDataLocation");
    }

    private string GetServerCode(string? errorCode = null)
    {
        return errorCode ?? AppConfig.Config["DefaultTimetableServer"] ?? throw new NullReferenceException("App config does not define DefaultTimetableServer");
    }
}