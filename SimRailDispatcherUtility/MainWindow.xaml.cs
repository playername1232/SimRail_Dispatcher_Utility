using SimRailDispatcherUtility.enums.Train;
using SimRailDispatcherUtility.Model.Train;
using SimRailDispatcherUtility.Services;
using SimRailDispatcherUtility.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace SimRailDispatcherUtility;

public partial class MainWindow : Window
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable

    #region Dependency Injection var
    
    private readonly ILogger<MainWindow> _logger;
    private readonly StationService _stationService;
    private readonly Func<AddTrainWindow> _addTrainFactory;

    #endregion
    
    private readonly ObservableCollection<TrainRow> _trains = null!;
    private readonly TrainReminderScheduler _scheduler = null!;

    private readonly DispatcherTimer _gcTimer = new();
    private int _gcOldTrainsDelay = 100;
    private ICollectionView _trainsView;

    public MainWindow(ILogger<MainWindow> logger, StationService stationService, Func<AddTrainWindow> addTrainFactory)
    {
        InitializeComponent();

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stationService = stationService ?? throw new ArgumentNullException(nameof(stationService));
        _addTrainFactory = addTrainFactory ?? throw new ArgumentNullException(nameof(addTrainFactory));

        this.IsEnabled = false;

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Title = $"SimRail Dispatcher Utility v{version}";

        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        try
        {
            _trains = new ObservableCollection<TrainRow>();
            
            _trainsView = CollectionViewSource.GetDefaultView(_trains);
            _trainsView.SortDescriptions.Add(
                new SortDescription(nameof(TrainRow.DepartureTime), ListSortDirection.Ascending)
            );

            TrainTimers_ListView.ItemsSource = _trainsView;

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
                "H4",
                DateTime.Parse("22.2.2026 01:29:00", new CultureInfo("cs-CZ")),
                StopType.Passenger,
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
        if (sender is AddTrainWindow)
        {
            _trains.Add(new TrainRow(train));
        }
    }

    private void OnReminderFired(object? sender, TrainRow row)
    {
        // TODO: lepší je toast / tray icon; tohle je jen demo
        MessageBox.Show(
            this,
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
        if (_stationService.CurrentNeighborStations.Count == 0)
        {
            MessageBox.Show(
                this,
                "No neighbor stations were loaded yet. Try selecting current station",
                "No neighbor stations",
                MessageBoxButton.OK);
            return;
        }

        var addWindow = _addTrainFactory();
        addWindow.Owner = this;
        addWindow.Show();
        IsEnabled = false;
    }

    private async void TimeTable_Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _stationService.LoadTimetablesJsonAsync();
            await _stationService.LoadStationsAsync();

            SelectedStation_ComboBox.ItemsSource =
                _stationService.Stations.Select(x => x.Name).ToList();

            this.IsEnabled = true;
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Error has occured", MessageBoxButton.OK);
            Application.Current.Shutdown();
        }
    }

    private void SelectedStation_ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        string? selectedStation = ((ComboBox)sender).SelectedItem.ToString();

        if (selectedStation is null)
            return;

        this.IsEnabled = false;

        for (int i = 0; i < _trains.Count; i++)
        {
            _trains.Clear();
        }

        _stationService.ComputeNeighborStations(selectedStation);

        this.IsEnabled = true;
    }

    private void EraseDepartedTrains_Button_Click(object sender, RoutedEventArgs e)
    {
        for (int i = _trains.Count - 1; i >= 0; i--)
        {
            if (_trains[i].TimeToDeparture <= TimeSpan.Zero)
                _trains.RemoveAt(i);
        }
    }
}