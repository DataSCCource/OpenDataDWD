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
using OxyPlot.Wpf;
using OxyPlot.Axes;

namespace OpenDataDWD
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Station> stations;
        private readonly IDatabaseAccess dataAccess;
        private string currentId;

        public MainWindow()
        {
            InitializeComponent();
            myMap.Focus();

            dataAccess = new SqliteDataAccess();

            if(!dataAccess.StationsDbExists())
            {
                dataAccess.CreateStationsDb();
            }
            stations = dataAccess.LoadStations();

            AddDataItems();
            AddDistrictItems(stations);
            AddPushPins(stations);
        }

        private void AddDataItems()
        {
            var dataTypes = Enum.GetNames(typeof(ClimateData.DataTypes)).Select(type => type.Replace('_', ' '));
            data_cb.ItemsSource = dataTypes;
            data_cb.SelectedIndex = 0;
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
                    Id = station.StationKE,
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
            string stationID = ((PushPinWithID)e.Source).Id;
            currentId = stationID;
            Station station = stations.Where(st => st.StationKE.Equals(stationID)).First();
            var climateData = dataAccess.LoadClimateData(stationID);

            PlotData(station, climateData);
        }

        private void PlotData(Station station, List<ClimateData> climateData)
        {
            myPlot.Title = station.StationName;
            myPlot.Axes.Clear();

            var selDataType = ((string)data_cb.SelectedItem).Replace(' ', '_');
            IEnumerable<double> myData;
            int nrOfDataPoints = 365;
            string unit = " C° ";

            switch (selDataType)
            {
                case nameof(ClimateData.DataTypes.Temperatur_Tagesmaximum):
                default:
                    myData = climateData.GetRange(climateData.Count - nrOfDataPoints, nrOfDataPoints).Select(dat => Math.Round(dat.TempMax, 1));
                    break;
                case nameof(ClimateData.DataTypes.Temperatur_Tagesminimum):
                    myData = climateData.GetRange(climateData.Count - nrOfDataPoints, nrOfDataPoints).Select(dat => Math.Round(dat.TempMin, 1));
                    break;
                case nameof(ClimateData.DataTypes.Luftdruck_Tagesmittel):
                    myData = climateData.GetRange(climateData.Count - nrOfDataPoints, nrOfDataPoints).Select(dat => Math.Round(dat.PressureMiddle, 1));
                    unit = " hpa ";
                    break;
                case nameof(ClimateData.DataTypes.Relative_Luftfeuchte):
                    myData = climateData.GetRange(climateData.Count - nrOfDataPoints, nrOfDataPoints).Select(dat => (double)dat.HumidityMiddle);
                    unit = " % ";
                    break;
                case nameof(ClimateData.DataTypes.Windstaerke_Tagesmittel):
                    myData = climateData.GetRange(climateData.Count - nrOfDataPoints, nrOfDataPoints).Select(dat => Math.Round(dat.WindForceMiddle, 1));
                    unit = " Bft ";
                    break;
                case nameof(ClimateData.DataTypes.Sonnenscheindauer_Tagessumme):
                    myData = climateData.GetRange(climateData.Count - nrOfDataPoints, nrOfDataPoints).Select(dat => Math.Round(dat.SunshineSum, 1));
                    unit = " h ";
                    break;
            }

            List<DataPoint> dataPoints = new List<DataPoint>();
            for (int i = -nrOfDataPoints; i < -1; i++)
            {
                dataPoints.Add(new DataPoint(i, myData.ToArray()[i+nrOfDataPoints]));
            }
            var yAxis = new OxyPlot.Wpf.LinearAxis() { Position = AxisPosition.Left, Unit = unit+" --> " };
            myPlot.Axes.Add(yAxis);

            mySeries.ItemsSource = dataPoints;
        }

        private void District_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (district_cb.SelectedIndex == 0)
                AddPushPins(stations);
            else
                AddPushPins(stations.Where(station => station.FederalState.FederalStateName.Equals(district_cb.SelectedItem.ToString())).ToList());
        }
        
        private void Data_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(currentId != null)
            {
                Station station = stations.Where(st => st.StationKE.Equals(currentId)).First();
                var climateData = dataAccess.LoadClimateData(currentId);

                PlotData(station, climateData);
            }
        }
    }
}
