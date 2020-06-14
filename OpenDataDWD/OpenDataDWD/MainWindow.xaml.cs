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

namespace OpenDataDWD
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Station> stations;
        private readonly IDatabaseAccess dataAccess;
        private List<ClimateData> currentClimateData;
        private string currentId;
        private AggregateData aggregateData;

        public MainWindow()
        {
            InitializeComponent();
            myMap.Focus();
            aggregateData = AggregateData.None;

            // Initialize Database 
            dataAccess = new SqliteDataAccess();
            if(!dataAccess.StationsDbExists())
            {
                dataAccess.CreateStationsDb();
            }
            stations = dataAccess.LoadStations();

            // Initialize GUI
            AddDataItems();
            AddDistrictItems(stations);
            AddPushPins(stations);
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
                    ToolTip = station.StationName,
                    Content = station.StationHeight,
                };

                myMap.Children.Add(pin);
                pin.MouseRightButtonDown += Pin_MouseUp;

                string toolTipString = $"Kennung: {station.StationKE}\nID: {station.Id}\nName: {station.StationName}\nBundesland: {station.FederalState.FederalStateName}";
                ToolTipService.SetToolTip(pin, toolTipString);
            });
        }

        /// <summary>
        /// Eventhandler for clicking a Pushpin (CAREFULL, only middle- and right-click work!)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Mouse Event</param>
        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string stationID = ((PushPinWithID)e.Source).Id;
            currentId = stationID;

            PlotData();
        }

        /// <summary>
        /// Plot climate data based on settings
        /// </summary>
        private void PlotData()
        {
            if (currentId != null)
            {
                var dateFrom = dateFrom_dp.SelectedDate ?? DateTime.Now.AddYears(-1);
                var dateTo = dateTo_dp.SelectedDate ?? DateTime.Now;

                currentClimateData = dataAccess.LoadClimateData(currentId, dateFrom, dateTo).OrderBy(cd => cd.Date).ToList();
                Station station = stations.Where(st => st.StationKE.Equals(currentId)).First();
                
                myPlot.Title = station.StationName;
                myPlot.Axes.Clear();

                string selector;
                switch (aggregateData)
                {
                    case AggregateData.None:
                    default:
                        selector = null;
                        break;
                    case AggregateData.Monthly:
                        selector = "MMM yyyy";
                        break;
                    case AggregateData.Yearly:
                        selector = "yyyy";
                        break;
                }

                IEnumerable<ClimateData> aggregatedClimateData;
                if (selector == null)
                {
                    aggregatedClimateData = currentClimateData;
                } 
                else
                {
                     aggregatedClimateData = currentClimateData.GroupBy(ccd => ccd.GetDate().ToString(selector)).Select(ccd => new ClimateData
                    {
                        Date = ccd.First().Date,
                        PressureMiddle = ccd.Where(c => c.HumidityMiddle>=-999).Average(c => c.PressureMiddle),
                        TempMax = ccd.Where(c => c.TempMax > -99).Average(c => c.TempMax),
                        TempMin = ccd.Where(c => c.TempMin > -99).Average(c => c.TempMin),
                        HumidityMiddle = (int)ccd.Where(c => c.HumidityMiddle > -99).Average(c => c.HumidityMiddle),
                        WindForceMiddle = (int)ccd.Where(c => c.WindForceMiddle > 0).Average(c => c.WindForceMiddle),
                        SunshineSum = (int)ccd.Where(c => c.SunshineSum > 0).Average(c => c.SunshineSum),
                    }).ToList();
                }



                var selDataType = ((string)data_cb.SelectedItem).Replace(' ', '_');
                IEnumerable<DataPoint> myData;
                string unit = " C° ";
            
                switch (selDataType)
                {
                    case nameof(ClimateData.DataTypes.Temperatur_Tagesmaximum):
                    default:
                        myData = aggregatedClimateData.Where(c => c.TempMin > -99).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.TempMax, 1)));
                        break;
                    case nameof(ClimateData.DataTypes.Temperatur_Tagesminimum):
                        myData = aggregatedClimateData.Where(c => c.TempMin > -99).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.TempMin, 1)));
                        break;
                    case nameof(ClimateData.DataTypes.Luftdruck_Tagesmittel):
                        myData = aggregatedClimateData.Where(c => c.HumidityMiddle >= -999).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.PressureMiddle, 1)));
                        unit = " hpa ";
                        break;
                    case nameof(ClimateData.DataTypes.Relative_Luftfeuchte):
                        myData = aggregatedClimateData.Where(c => c.HumidityMiddle > -99).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), (double)dat.HumidityMiddle));
                        unit = " % ";
                        break;
                    case nameof(ClimateData.DataTypes.Windstaerke_Tagesmittel):
                        myData = aggregatedClimateData.Where(c => c.WindForceMiddle > 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.WindForceMiddle, 1)));
                        unit = " Bft ";
                        break;
                    case nameof(ClimateData.DataTypes.Sonnenscheindauer_Tagessumme):
                        myData = aggregatedClimateData.Where(c => c.SunshineSum > 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.SunshineSum, 1)));
                        unit = " h ";
                        break;
                }

                int count = myData.Count();
                int step = count / 365;
                var yAxis = new OxyPlot.Wpf.LinearAxis() { Position = AxisPosition.Left, Unit = unit+" --> " };
                var xAxis = new OxyPlot.Wpf.DateTimeAxis() { Position = AxisPosition.Bottom };
                myPlot.Axes.Add(yAxis);
                myPlot.Axes.Add(xAxis);

                mySeries.ItemsSource = myData;
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
                AddPushPins(stations);
            else
                AddPushPins(stations.Where(station => station.FederalState.FederalStateName.Equals(district_cb.SelectedItem.ToString())).ToList());
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
        /// Eventhandler for radiobuttons that allow aggregation of climatedata by month or year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Group_rb_Checked(object sender, RoutedEventArgs e)
        {
            if (no_rb.IsChecked ?? false)
            {
                aggregateData = AggregateData.None;
            }
            else if (mon_rb.IsChecked ?? false)
            {
                aggregateData = AggregateData.Monthly;
            }
            else
            {
                aggregateData = AggregateData.Yearly;
            }

            PlotData();
        }
        enum AggregateData { None, Monthly, Yearly }

    }
}
