using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DatabaseAccess
{
    class Program
    {
        private const int ANSI = 1252;

        static void Main()
        {
            IDatabaseAccess dataAccess = new SqliteDataAccess();
            var stations = ReadStationsFromFile();


            ImportStationsIntoDatabase(dataAccess, stations);

        }

        private static void ImportStationsIntoDatabase(IDatabaseAccess dataAccess, List<Station> stations)
        {
            var districtNames = stations.Select(station => station.FederalState.FederalStateName).Distinct().ToList();
            foreach (var districtName in districtNames)
            {
                dataAccess.SaveFederalState(new FederalState( 1, districtName ));
            }

            foreach (var station in stations)
            {
                dataAccess.SaveStation(station);
            }
            
        }

        private static List<Station> ReadStationsFromFile()
        {
            List<Station> stations = new List<Station>();
            var stationsStrings = File.ReadAllLines("../../Daten/KL_Standardformate_Beschreibung_Stationen.txt", Encoding.GetEncoding(ANSI));

            stationsStrings.Where(x => x.StartsWith("10")).ToList()
                           .ForEach(line => stations.Add(GetStationFromString(line)));

            return stations;
        }

        private static Station GetStationFromString(string line)
        {
            var sMapper = GetStationMappers();


            return new Station()
        {
            StationKE = line.Substring(sMapper[0].Start, sMapper[0].Length).Trim(),
                Id = int.Parse(line.Substring(sMapper[1].Start, sMapper[1].Length).Trim()),
                DateFrom = (long)DateTime.UtcNow.Subtract(DateTime.ParseExact(line.Substring(sMapper[2].Start, sMapper[2].Length).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture)).TotalSeconds,
                DateTo = (long)DateTime.UtcNow.Subtract(DateTime.ParseExact(line.Substring(sMapper[3].Start, sMapper[3].Length).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture)).TotalSeconds,
                StationHeight = int.Parse(line.Substring(sMapper[4].Start, sMapper[4].Length).Trim()),
                Latitude = float.Parse(line.Substring(sMapper[5].Start, sMapper[5].Length).Trim(), CultureInfo.InvariantCulture),
                Longitude = float.Parse(line.Substring(sMapper[6].Start, sMapper[6].Length).Trim(), CultureInfo.InvariantCulture),
                StationName = line.Substring(sMapper[7].Start, sMapper[7].Length).Trim(),
                FederalState = new FederalState(line.Substring(sMapper[8].Start, sMapper[8].Length).Trim()),
            };
    }

    private static List<DataField> GetStationMappers()
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

        struct DataField
        {
            public int Start;
            public int Length;
        }
    }

}
