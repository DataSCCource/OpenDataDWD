using Microsoft.Maps.MapControl.WPF;
using OpenDataDWD.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using DatabaseAccess;

namespace OpenDataDWD
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ANSI = 1252;
        private readonly List<Station> stations;

        public MainWindow()
        {
            InitializeComponent();
            myMap.Focus();

            IDatabaseAccess dataAccess = new SqliteDataAccess();

            if(!dataAccess.StationsDbExists())
            {
                dataAccess.CreateStationsDb();
            }
            stations = dataAccess.LoadStations();

            AddDistrictItems(stations);
            AddPushPins(stations);
        }

        private void AddDistrictItems(List<Station> stations)
        {
            var districtNames = stations.Select(station => station.FederalState.FederalStateName).Distinct().ToList();
            districtNames.Add("--- Alle ---");
            districtNames.Sort();
            district_cb.ItemsSource = districtNames;
            district_cb.SelectedIndex = 0;
        }

        private void AddPushPins(List<Station> stations)
        {
            myMap.Children.Clear();
            stations.ForEach(station =>
            {
                PushPinWithID pin = new PushPinWithID()
                {
                    Id = station.Id,
                    Location = new Location(station.Latitude, station.Longitude),
                    ToolTip = station.StationName,
                    Content = station.StationHeight,
                };

                myMap.Children.Add(pin);
                pin.MouseRightButtonDown += Pin_MouseUp;

                string toolTipString = $"Kennung: {station.StationKE}\nID: {station.Id}\nName: {station.StationName}\nBundesland: {station.FederalState.FederalStateName}";
                ToolTipService.SetToolTip(pin, toolTipString);
            });
        }

        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Random r = new Random();
            int stationID = ((PushPinWithID)e.Source).Id;
            Station station = stations.Where(st => st.Id == stationID).First();

            myPlot.Title = station.StationName;
            List<DataPoint> dataPoints = new List<DataPoint>();
            for (int i = 0; i < 100; i++)
            {
                dataPoints.Add(new DataPoint(i, r.Next(0, 20)));
            }
            mySeries.ItemsSource = dataPoints;
        }

        private void District_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (district_cb.SelectedIndex == 0)
                AddPushPins(stations);
            else
                AddPushPins(stations.Where(station => station.FederalState.FederalStateName.Equals(district_cb.SelectedItem.ToString())).ToList());
        }
    }
}
