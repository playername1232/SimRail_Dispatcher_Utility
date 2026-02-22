using Serilog;
using SimRailDispatcherUtility.enums.Train;
using SimRailDispatcherUtility.Model.Train;
using SimRailDispatcherUtility.Services;
using SimRailDispatcherUtility.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace SimRailDispatcherUtility;

public partial class MainWindow : Window
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ObservableCollection<TrainRow> _trains = null!;
    private readonly ILogger _logger = null!;
    private readonly TrainReminderScheduler _scheduler = null!;
    
    private readonly DispatcherTimer _gcTimer = new();
    private int _gcOldTrainsDelay = 100;

    public MainWindow()
    {
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        try
        {
            _trains = new ObservableCollection<TrainRow>();
            TrainTimers_ListView.ItemsSource = _trains;
        
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();


            _scheduler = new TrainReminderScheduler(_trains, Dispatcher);
            _scheduler.ReminderFired += OnReminderFired;
            _scheduler.Start();

            _gcOldTrainsDelay = int.Parse(AppConfig.Config["UserSettings:OldTrainsCleanupInHours"] ?? throw new ArgumentNullException($"AppConfig.Config[\"UserSettings:OldTrainsCleanupInHours\"]"));
            int gcOldTrainsCycleDelayMinutes = int.Parse(AppConfig.Config["UserSettings:OldTrainsGarbageCollectorCycleDelayInMinutes"] ?? throw new ArgumentNullException($"AppConfig.Config[\"UserSettings:OldTrainsGarbageCollectorCycleDelayInMinutes\"]"));

            _gcTimer.Interval = TimeSpan.FromMinutes(gcOldTrainsCycleDelayMinutes);
            _gcTimer.Tick += TrainsGarbageCollector;
            _gcTimer.Start();

            // test item
            _trains.Add(new TrainRow(new Train(
                11111,
                "Bohumín",
                "Ostrava-Svinov",
                4,
                DateTime.Parse("22.2.2026 01:29:00", new CultureInfo("cs-CZ")),
                StopType.PassengerStop,
                TrainType.Intercity,
                true,
                TimeSpan.FromMinutes(1))));

            TrainsGarbageCollector(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    public void AddTrain(object sender, Train train)
    {
        if (sender is AddTrain)
        {
            _trains.Add(new TrainRow(train));
        }
    }

    private void OnReminderFired(object? sender, TrainRow row)
    {
        _logger.Information("REMINDER: Train {Id} departs in ~1 minute ({Dep:g})", row.Id, row.DepartureTime);

        // TODO: lepší je toast / tray icon; tohle je jen demo
        MessageBox.Show(
            $"Train {row.Id} departs in ~1 minute!\n{row.PreviousPost} → {row.NextPost}\nTrack {row.TrackNumber}\nDeparture: {row.DepartureTime:g}",
            "SimRail Reminder",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void TrainsGarbageCollector(object? sender, EventArgs _)
    {
        for (int i = 0; i < _trains.Count; i++)
        {
            if (DateTime.Now - _trains[i].DepartureTime > TimeSpan.FromHours(_gcOldTrainsDelay))
            {
                _trains.RemoveAt(i);
                i--;
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _scheduler.ReminderFired -= OnReminderFired;
        _scheduler.Stop();

        (_logger as IDisposable)?.Dispose();
        base.OnClosed(e);
    }

    private void DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        if (TrainTimers_ListView.SelectedItem is TrainRow row)
        {
            _trains.Remove(row);
        }
    }

    private void AddTrain_Button_Click(object sender, RoutedEventArgs e)
    {
        AddTrain addWindow = new();
        addWindow.Owner = this;

        addWindow.Show();
        this.IsEnabled = false;
    }
}