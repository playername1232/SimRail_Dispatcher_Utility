using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Extensions.Configuration;
using SimRailDispatcherUtility.enums.Train;
using SimRailDispatcherUtility.Model.Train;
using SimRailDispatcherUtility.Services;

namespace SimRailDispatcherUtility.Windows
{
    /// <summary>
    /// Interaction logic for AddTrain.xaml
    /// </summary>
    public partial class AddTrainWindow : Window
    {
        public AddTrainWindow(StationService stationService)
        {
            InitializeComponent();

            DepartureDate_DatePicker.SelectedDate = DateTime.Today;
            DepartureTime_TextBox.Text = DateTime.Now.ToString("HH:mm");

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var stations = stationService?.CurrentNeighborStations.ToList() ?? throw new ArgumentNullException(nameof(stationService));

            PreviousPost_ComboBox.ItemsSource = stations;
            NextPost_ComboBox.ItemsSource = stations;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Owner.IsEnabled = true;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show("Do you want to cancel adding new train?", "Cancel action", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (response == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            int id          = 0,
                minRemind   = 0;

            string track = "";

            string? errors = null;

            Action<string> handleAppendError = (err) =>
            {
                errors = (errors == null ? "" : errors + "\n") + err;
            };

            if (string.IsNullOrWhiteSpace(TrainId_TextBox.Text) || !int.TryParse(TrainId_TextBox.Text, out id))
            {
                handleAppendError("Train Id must be numeric!");
            }

            string previous = PreviousPost_ComboBox.Text;
            string next = NextPost_ComboBox.Text;

            bool bInvalidPrevious = false;
            bool bInvalidNext = false;

            if (string.IsNullOrWhiteSpace(previous))
            {
                handleAppendError("Previous post must be entered!");
                bInvalidPrevious = true;
            }

            if (string.IsNullOrWhiteSpace(next))
            {
                handleAppendError("Next post must be entered!");
                bInvalidNext = true;
            }

            if (bInvalidPrevious is false && bInvalidNext is false)
            {
                if (previous.Equals(next, StringComparison.CurrentCultureIgnoreCase))
                {
                    handleAppendError("Previous and next post cannot be the same!");
                }
            }

            if (string.IsNullOrWhiteSpace(Track_TextBox.Text))
            {
                handleAppendError("Track number must be entered!");
            }

            var date = DepartureDate_DatePicker.SelectedDate?.Date;

            DateTime? departure = null;

            if (date is null || !TimeSpan.TryParse(DepartureTime_TextBox.Text, out var time))
            {
                handleAppendError("Invalid time format (use HH:mm or HH:mm:ss)");
            }
            else
            {
                departure = date + time;

                if (DateTime.Now - departure >= TimeSpan.Zero)
                {
                    handleAppendError("Invalid time: Departure must be in the future");
                }
            }

            if (string.IsNullOrWhiteSpace(ReminderMinutes_TextBox.Text) ||
                !int.TryParse(ReminderMinutes_TextBox.Text, out minRemind) ||
                minRemind < 0)
            {
                handleAppendError("Reminder must be zero or a positive number");
            }

            // todo lepčí ošetření enumů - chat
            StopType? stopType = (StopType?)StopType_ComboBox.SelectedItem;
            if (stopType is null)
            {
                handleAppendError("Stop type must be selected");
            }

            TrainType? trainType = (TrainType?)TrainType_ComboBox.SelectedItem;
            if (trainType is null)
            {
                handleAppendError("Train type must be selected");
            }

            if (errors != null)
            {
                MessageBox.Show(errors, "Missing required data", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            Train train = new(
                id,
                PreviousPost_ComboBox.Text,
                NextPost_ComboBox.Text,
                track,
                departure!.Value,
                stopType!.Value,
                trainType!.Value,
                DrivenBy_CheckBox.IsChecked ?? false,
                TimeSpan.FromMinutes(minRemind));

            ((MainWindow)this.Owner).AddTrain(this, train);
            this.Close();
        }
    }
}
