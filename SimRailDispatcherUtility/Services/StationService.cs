using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

        // Source of truth for "valid posts"
        // Timetable contains posts that are not visible / available to dispatcher
        // eg.
        // Skierniewice should have: Belchów, Zyrardow, Plycwia, Puszcza Marianska
        // but Train Timetable contains minor posts between these posts. + Some Major posts are not in Stations API e.g. Belchów
        var stationsByCanonical = Stations
            .Select(s => s.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .GroupBy(CanonicalizePointName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        if (stationsByCanonical.Count == 0)
            return;

        string json = File.ReadAllText(Environment.ExpandEnvironmentVariables(GetTimetablesFileRawPath()));
        var trains = JsonSerializer.Deserialize<List<TrainRun>>(json) ?? new List<TrainRun>();

        // store canonical to avoid duplicates from different spellings
        var neighborByCanonical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var targetCanonical = CanonicalizePointName(stationName);

        foreach (var train in trains)
        {
            var tt = train.Timetable;
            if (tt is null || tt.Count == 0)
                continue;

            for (int i = 0; i < tt.Count; i++)
            {
                var curRaw = tt[i].NameOfPoint;
                if (string.IsNullOrWhiteSpace(curRaw))
                    continue;

                if (!string.Equals(CanonicalizePointName(curRaw), targetCanonical, StringComparison.OrdinalIgnoreCase))
                    continue;

                var prev = FindNearestNeighbor(tt, i, -1, targetCanonical, stationsByCanonical);
                if (prev is not null)
                    neighborByCanonical[prev.Value.Canonical] = prev.Value.Display;

                var next = FindNearestNeighbor(tt, i, +1, targetCanonical, stationsByCanonical);
                if (next is not null)
                    neighborByCanonical[next.Value.Canonical] = next.Value.Display;
            }
        }

        CurrentNeighborStations.Clear();
        foreach (var display in neighborByCanonical.Values.OrderBy(x => x))
            CurrentNeighborStations.Add(display);
    }

    private static (string Canonical, string Display)? FindNearestNeighbor(
        IList<TimetablePoint> timetable,
        int fromIndex,
        int step,
        string targetCanonical,
        Dictionary<string, string> stationsByCanonical)
    {
        // PASS 1: Prefer official stations from Stations API
        for (int j = fromIndex + step; j >= 0 && j < timetable.Count; j += step)
        {
            var raw = timetable[j].NameOfPoint;
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            if (LooksLikeSubPoint(raw))
                continue;

            var canon = CanonicalizePointName(raw);
            if (string.IsNullOrWhiteSpace(canon))
                continue;

            // 🚫 never return self
            if (string.Equals(canon, targetCanonical, StringComparison.OrdinalIgnoreCase))
                continue;

            if (stationsByCanonical.TryGetValue(canon, out var display))
                return (canon, display);
        }

        // PASS 2: Fallback to timetable points (cleaned), e.g. Biechów
        for (int j = fromIndex + step; j >= 0 && j < timetable.Count; j += step)
        {
            var raw = timetable[j].NameOfPoint;
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            if (LooksLikeSubPoint(raw))
                continue;

            var canon = CanonicalizePointName(raw);
            if (string.IsNullOrWhiteSpace(canon))
                continue;

            // never return self (important: same check here!)
            if (string.Equals(canon, targetCanonical, StringComparison.OrdinalIgnoreCase))
                continue;

            // display: if exists in Stations, use it; else use canonical (fallback)
            if (stationsByCanonical.TryGetValue(canon, out var display))
                return (canon, display);

            return (canon, canon);
        }

        return null;
    }

    private static string CanonicalizePointName(string name)
    {
        name = name.Trim();

        // remove common timetable suffix variants
        name = Regex.Replace(name, @"\s+R\d+\b.*$", "", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\s+GT\b.*$", "", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\s+(M|S)?\s*PZS\b.*$", "", RegexOptions.IgnoreCase);

        name = Regex.Replace(name, @"\s{2,}", " ").Trim();
        return name;
    }

    private static bool LooksLikeSubPoint(string rawName)
    {
        rawName = rawName.Trim();

        return Regex.IsMatch(rawName, @"\bGT\b", RegexOptions.IgnoreCase) ||
               Regex.IsMatch(rawName, @"\bPZS\b", RegexOptions.IgnoreCase) ||
               Regex.IsMatch(rawName, @"\bR\d+\b", RegexOptions.IgnoreCase);
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