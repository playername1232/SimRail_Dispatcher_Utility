using System.ComponentModel;
using System.Runtime.CompilerServices;
using SimRailDispatcherUtility.enums.Train;

namespace SimRailDispatcherUtility.Model.Train;

public sealed class TrainRow : INotifyPropertyChanged
{
    public Train Train { get; }

    // UI fields (updated by scheduler)
    private TimeSpan _timeToDeparture;
    private TimeSpan _timeToReminder;
    private TrainScheduleState _state;

    public TimeSpan TimeToDeparture
    {
        get => _timeToDeparture;
        internal set { _timeToDeparture = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimeToDepartureString)); }
    }

    public TimeSpan TimeToReminder
    {
        get => _timeToReminder;
        internal set { _timeToReminder = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimeToReminderString)); }
    }

    public TrainScheduleState State
    {
        get => _state;
        internal set { _state = value; OnPropertyChanged(); OnPropertyChanged(nameof(StateString)); }
    }

    public bool ReminderFired { get; internal set; }

    // Passthrough properties for binding
    public int Id => Train.Id;
    public string PreviousPost => Train.PreviousPost;
    public string NextPost => Train.NextPost;
    public string TrackNumber => Train.TrackNumber;
    public DateTime DepartureTime => Train.DepartureTime;
    public bool IsDrivenByPlayer => Train.IsDrivenByPlayer;
    public TimeSpan ReminderOffset => Train.ReminderOffset;
    public DateTime RemainderTime => DepartureTime - ReminderOffset;

    public string StopTypeString => Train.StopType switch
    {
        StopType.Passenger => "Passenger",
        StopType.Technical => "Technical",
        StopType.EarlyArrival => "Early Arrival",
        _ => Train.StopType.ToString()
    };

    public string TrainTypeString => Train.TrainType switch
    {
        TrainType.Intercity => "Intercity",
        TrainType.Regional => "Regional",
        TrainType.Freight => "Freight",
        _ => Train.TrainType.ToString()
    };

    public string IsDrivenByStr => Train.IsDrivenByPlayer ? "Player" : "AI";

    public string DepartureTimeString => DepartureTime.ToString("g");

    public string TimeToDepartureString => FormatCountdown(TimeToDeparture);
    public string TimeToReminderString => FormatCountdown(TimeToReminder);

    public string StateString => State.ToString();

    public TrainRow(Train train)
    {
        Train = train;
        State = TrainScheduleState.Scheduled;
    }

    public override string ToString() => $"TrainRow: {Train.ToString()}";

    private static string FormatCountdown(TimeSpan ts)
    {
        var sign = ts < TimeSpan.Zero ? "-" : "";
        var abs = ts.Duration();
        if (abs.TotalHours >= 1) return $"{sign}{(int)abs.TotalHours:00}:{abs.Minutes:00}:{abs.Seconds:00}";
        return $"{sign}{abs.Minutes:00}:{abs.Seconds:00}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}