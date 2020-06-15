using Microsoft.Maps.MapControl.WPF;
using OpenDataDWD.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OxyPlot;
using DatabaseAccess;
using OxyPlot.Axes;
using System.Windows.Navigation;
using System.Windows.Media;

namespace OpenDataDWD
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PlotLogic pLogic;

        public MainWindow()
        {
            pLogic = new PlotLogic(new SqliteDataAccess());
            InitializeComponent();

            // Initialize GUI
            myMap.Focus();
            AddDataItems();
            AddDistrictItems(pLogic.Stations);
            AddPushPins(pLogic.Stations);
        }

        /// <summary>
        /// Add the possibile ClimateData types to choose from to the combobox
        /// </summary>
        private void AddDataItems()
        {
            var dataTypes = Enum.GetNames(typeof(ClimateData.DataTypes)).Select(type => type.Replace('_', ' '));
            data_cb.ItemsSource = dataTypes;
            data_cb.SelectedIndex = 0;
        }

        /// <summary>
        /// Add FederalStates to filter combobox
        /// </summary>
        /// <param name="stations">List of all Station objects</param>
        private void AddDistrictItems(List<Station> stations)
        {
            var districtNames = stations.Select(station => station.FederalState.FederalStateName).Distinct().ToList();
            districtNames.Add("--- Alle ---");
            districtNames.Sort();
            district_cb.ItemsSource = districtNames;
            district_cb.SelectedIndex = 0;
        }

        /// <summary>
        /// Add PushPins to Map based on the location of each station
        /// </summary>
        /// <param name="stations">List of all Station objects</param>
        private void AddPushPins(List<Station> stations)
        {
            myMap.Children.Clear();
            stations.ForEach(station =>
            {
                PushPinWithID pin = new PushPinWithID()
                {
                    Id = station.StationKE,
                    Location = new Location(station.Latitude, station.Longitude),
                    Content = station.StationHeight,
                };

                myMap.Children.Add(pin);
                pin.MouseRightButtonDown += Pin_MouseUp;

                string toolTipString = $"Name: {station.StationName}\nBundesland: {station.FederalState.FederalStateName}\nKennung: {station.StationKE}\nID: {station.Id}\nZeitraum: {station.GetDataFromDateTime():dd.MM.yyyy} - {station.GetDataToDateTime():dd.MM.yyyy}";
                
                ToolTipService.SetToolTip(pin, toolTipString);
            });
        }

        /// <summary>
        /// Eventhandler for clicking a Pushpin (CAREFULL, only right-click works!)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Mouse Event</param>
        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            foreach (var child in myMap.Children)
            {
                // Set all pushpin colors to default color
                if(child is PushPinWithID pushPin) {
                    pushPin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE56910"));
                }
            }

            // handle pushpin click (set pushpin id, color and get current station based on id)
            var clickedPushPin = (PushPinWithID)e.Source;
            pLogic.SetCurrentId(clickedPushPin.Id);
            clickedPushPin.Background = new SolidColorBrush(Colors.YellowGreen);
            Station station = pLogic.GetCurrentStation();

            // reset DatePicker and average radiobox
            dateTo_dp.SelectedDate = station.GetDataToDateTime();
            dateFrom_dp.SelectedDate = station.GetDataToDateTime().AddYears(-1);
            no_rb.IsChecked = true;

            PlotData();
        }

        /// <summary>
        /// Plot climate data based on settings
        /// </summary>
        private void PlotData()
        {
            if(data_cb.SelectedItem != null)
            {
                var selDataType = ((string)data_cb.SelectedItem).Replace(' ', '_');
                pLogic.PlotData(myPlot, mySeries, dateFrom_dp, dateTo_dp, selDataType);
            }
        }

        /// <summary>
        /// Eventhandler for filtering stations by federal state 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void District_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (district_cb.SelectedIndex == 0)
            {
                // Set pushpins for all stations
                AddPushPins(pLogic.Stations);
            }
            else
            {
                // Set Pushpins based on federal state filter
                AddPushPins(pLogic.Stations.Where(station => station.FederalState.FederalStateName.Equals(district_cb.SelectedItem.ToString())).ToList());
            }
        }
        
        /// <summary>
        /// Eventhandler for datatype to plot climate data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlotData();
        }

        /// <summary>
        /// Eventhandler for radiobuttons that allow averagiing of climatedata by month or year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Group_rb_Checked(object sender, RoutedEventArgs e)
        {
            if (no_rb.IsChecked ?? false)
                pLogic.SetAggregateData(AverageData.None);
            else if (mon_rb.IsChecked ?? false)
                pLogic.SetAggregateData(AverageData.Monthly);
            else
                pLogic.SetAggregateData(AverageData.Yearly);

            PlotData();
        }
    }
}
