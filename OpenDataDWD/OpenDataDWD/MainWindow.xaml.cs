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

            stations = ReadStations();
            AddDistrictItems(stations);
            AddPushPins(stations);
        }

        private List<Station> ReadStations()
        {
            List<Station> stations = new List<Station>();
            var stationsStrings = File.ReadAllLines("../../Daten/KL_Standardformate_Beschreibung_Stationen.txt", Encoding.GetEncoding(ANSI));

            stationsStrings.Where(x => x.StartsWith("10")).ToList()
                           .ForEach(line => stations.Add(GetStationFromString(line)) );

            return stations;
        }

        private void AddDistrictItems(List<Station> stations)
        {
            var districtNames = stations.Select(station => station.FederalState).Distinct().ToList();
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
                    Id = station.StationID,
                    Location = new Location(station.Latitude, station.Longitude),
                    ToolTip = station.StationName,
                    Content = station.StationHeight,
                };

                myMap.Children.Add(pin);
                pin.MouseRightButtonDown += Pin_MouseUp;

                string toolTipString = $"Kennung: {station.StationKE}\nID: {station.StationID}\nName: {station.StationName}\nBundesland: {station.FederalState}";
                ToolTipService.SetToolTip(pin, toolTipString);
            });
        }

        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Random r = new Random();
            string stationID = ((PushPinWithID)e.Source).Id;
            Station station = stations.Where(st => st.StationID.Equals(stationID)).First();

            myPlot.Title = station.StationName;
            List<DataPoint> ldp = new List<DataPoint>();
            for (int i = 0; i < 100; i++)
            {
                ldp.Add(new DataPoint(i, r.Next(0, 20)));
            }

            mySeries.ItemsSource = ldp;
        }

        private Station GetStationFromString(string line)
        {
            var sMapper = GetStationMappers();
            return new Station()
            {
                StationKE = line.Substring(sMapper[0].Start, sMapper[0].Length).Trim(),
                StationID = line.Substring(sMapper[1].Start, sMapper[1].Length).Trim(),
                DataFrom = DateTime.ParseExact(line.Substring(sMapper[2].Start, sMapper[2].Length).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture),
                DataTo = DateTime.ParseExact(line.Substring(sMapper[3].Start, sMapper[3].Length).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture),
                StationHeight = int.Parse(line.Substring(sMapper[4].Start, sMapper[4].Length).Trim()),
                Latitude = float.Parse(line.Substring(sMapper[5].Start, sMapper[5].Length).Trim(), CultureInfo.InvariantCulture),
                Longitude = float.Parse(line.Substring(sMapper[6].Start, sMapper[6].Length).Trim(), CultureInfo.InvariantCulture),
                StationName = line.Substring(sMapper[7].Start, sMapper[7].Length).Trim(),
                FederalState = line.Substring(sMapper[8].Start, sMapper[8].Length).Trim(),
            };
        }

        private List<DataField> GetStationMappers()
        {
            return new List<DataField>
            {
                new DataField { Start=0, Length=5 },
                new DataField { Start=6, Length=5 },
                new DataField { Start=12, Length=8 },
                new DataField { Start=21, Length=8 },
                new DataField { Start=31, Length=8 },
                new DataField { Start=40, Length=10 },
                new DataField { Start=51, Length=10 },
                new DataField { Start=62, Length=25 },
                new DataField { Start=88, Length=25 },
            };
        }

        private void District_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (district_cb.SelectedIndex == 0)
                AddPushPins(stations);
            else
                AddPushPins(stations.Where(station => station.FederalState == district_cb.SelectedItem.ToString()).ToList());
            
        }
    }

    struct DataField
    {
        public int Start;
        public int Length;
    }
}
