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

                string toolTipString = $"Name: {station.StationName}\nBundesland: {station.FederalState.FederalStateName}\nKennung: {station.StationKE}\nID: {station.Id}\nZeitraum: {station.GetDataFromDateTime():dd.MM.yyyy} - {station.GetDataToDateTime():dd.MM.yyyy}";
                
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
            foreach (var child in myMap.Children)
            {
                if(child is PushPinWithID pushPin) {
                    pushPin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE56910"));
                }
            }

            var clickedPushPin = (PushPinWithID)e.Source;
            currentId = clickedPushPin.Id;
            clickedPushPin.Background = new SolidColorBrush(Colors.YellowGreen);
            Station station = GetCurrentStation();

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
            if (currentId != null)
            {
                Station station = GetCurrentStation();

                // Set filter date the last year of data, if no valid "from" / "to" dates where selected
                var dateTo = dateTo_dp.SelectedDate ?? station.GetDataToDateTime();
                var dateFrom = dateFrom_dp.SelectedDate ?? station.GetDataToDateTime().AddYears(-1);
                
                // Set displayed dates to data range and limit to selected dates
                // e.g. If "To-Date" is set to 01.01.2020, the "From-Date" after that date is not possible
                dateFrom_dp.DisplayDateStart = station.GetDataFromDateTime();
                dateFrom_dp.DisplayDateEnd = dateTo_dp.SelectedDate;
                dateTo_dp.DisplayDateEnd = station.GetDataToDateTime();
                dateTo_dp.DisplayDateStart = dateFrom_dp.SelectedDate;

                currentClimateData = dataAccess.LoadClimateData(currentId, dateFrom, dateTo).OrderBy(cd => cd.Date).ToList();
                
                myPlot.Title = station.StationName + $" ({station.GetDataFromDateTime():dd.MM.yyyy} - {station.GetDataToDateTime():dd.MM.yyyy})";
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

                IEnumerable<ClimateData> aggregatedClimateData = new List<ClimateData>();
                if (selector == null)
                {
                    aggregatedClimateData = currentClimateData;
                } 
                else
                {
                    aggregatedClimateData = currentClimateData.GroupBy(ccd1 => ccd1.GetDate().ToString(selector)).Select(ccd => new ClimateData
                        {
                            Date = ccd.First().Date,
                            TempMax = ccd.Where(c => c.TempMax > -99).Select(c => c.TempMax).DefaultIfEmpty(-9999).Average(),
                            TempMin = ccd.Where(c => c.TempMin > -99).Select(c => c.TempMin).DefaultIfEmpty(-9999).Average(),
                            PressureMiddle = ccd.Where(c => c.PressureMiddle >= 0).Select(c => c.PressureMiddle).DefaultIfEmpty(-9999).Average(),
                            HumidityMiddle = (int)ccd.Where(c => c.HumidityMiddle > -99).Select(c => c.HumidityMiddle).DefaultIfEmpty(-9999).Average(),
                            WindForceMiddle = (int)ccd.Where(c => c.WindForceMiddle > 0).Select(c => c.WindForceMiddle).DefaultIfEmpty(-9999).Average(),
                            SunshineSum = (int)ccd.Where(c => c.SunshineSum > 0).Select(c=> c.SunshineSum).DefaultIfEmpty(-9999).Average(),
                        }).ToList();
                }

                var selDataType = ((string)data_cb.SelectedItem).Replace(' ', '_');
                IEnumerable<DataPoint> myData;
                string unit = " C° ";
            
                switch (selDataType)
                {
                    case nameof(ClimateData.DataTypes.Temperatur_Tagesmaximum):
                    default:
                        myData = aggregatedClimateData.Where(c => c.TempMax > -99).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.TempMax, 1)));
                        break;
                    case nameof(ClimateData.DataTypes.Temperatur_Tagesminimum):
                        myData = aggregatedClimateData.Where(c => c.TempMin > -99).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.TempMin, 1)));
                        break;
                    case nameof(ClimateData.DataTypes.Luftdruck_Tagesmittel):
                        myData = aggregatedClimateData.Where(c => c.PressureMiddle >= 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.PressureMiddle, 1)));
                        unit = " hpa ";
                        break;
                    case nameof(ClimateData.DataTypes.Relative_Luftfeuchte):
                        myData = aggregatedClimateData.Where(c => c.HumidityMiddle >= 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), (double)dat.HumidityMiddle));
                        unit = " % ";
                        break;
                    case nameof(ClimateData.DataTypes.Windstaerke_Tagesmittel):
                        myData = aggregatedClimateData.Where(c => c.WindForceMiddle >= 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.WindForceMiddle, 1)));
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

        public Station GetCurrentStation()
        {
            return stations.Where(st => st.StationKE.Equals(currentId)).First();
        }
    }
}
