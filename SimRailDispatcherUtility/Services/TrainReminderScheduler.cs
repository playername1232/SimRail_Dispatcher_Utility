using System.Collections.ObjectModel;
using System.Windows.Threading;
using SimRailDispatcherUtility.enums.Train;
using SimRailDispatcherUtility.Model.Train;

namespace SimRailDispatcherUtility.Services;

public sealed class TrainReminderScheduler : IDisposable
{
    private readonly ObservableCollection<TrainRow> _rows;
    private readonly DispatcherTimer _timer;

    public event EventHandler<TrainRow>? ReminderFired;

    public TrainReminderScheduler(ObservableCollection<TrainRow> rows, Dispatcher dispatcher)
    {
        _rows = rows;

        _timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTick;
    }

    private void OnTick(object? sender, EventArgs e) => Tick();

    public void Start()
    {
        Tick();
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    private void Tick()
    {
        var now = DateTime.Now;

        List<TrainRow> snapshot;
        lock (_rows)
        {
            snapshot = _rows.ToList();
        }

        foreach (var row in snapshot)
        {
            var dep = row.DepartureTime;
            var offset = row.ReminderOffset;

            row.TimeToDeparture = dep - now;
            row.TimeToReminder  = (dep - offset) - now;

            if (row.TimeToDeparture <= TimeSpan.Zero)
                row.State = TrainScheduleState.Departed;
            else if (row.TimeToDeparture <= TimeSpan.FromSeconds(30))
                row.State = TrainScheduleState.Departing;
            else if (row.TimeToReminder <= TimeSpan.Zero)
                row.State = TrainScheduleState.ReminderDue;
            else
                row.State = TrainScheduleState.Scheduled;

            if (!row.ReminderFired && now >= dep - offset && now < dep)
            {
                row.ReminderFired = true;
                ReminderFired?.Invoke(this, row);
            }
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}