using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DatabaseAccess
{
    class Program
    {
        private const int ANSI = 1252; // Windows-1252 or CP1252 Encoding

        static void Main()
        {
            IDatabaseAccess dataAccess = new SqliteDataAccess();

            var stations = ReadStationsFromFile();
            ImportStationsIntoDatabase(dataAccess, stations);
        }

        private static void ImportStationsIntoDatabase(IDatabaseAccess dataAccess, List<Station> stations)
        {
            if(!dataAccess.StationsDbExists()) 
            {
                dataAccess.CreateStationsDb();
            }

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
            DataMapper stationDataMapper = new StationDataMapper();

            return new Station()
            {
                StationKE = stationDataMapper.GetDataFromString(line, StationDataMapper.ST_KE),
                Id = int.Parse(stationDataMapper.GetDataFromString(line, StationDataMapper.ST_ID)),
                DateFrom = (long)DateTime.ParseExact(stationDataMapper.GetDataFromString(line, StationDataMapper.VON), "yyyyMMdd", CultureInfo.InvariantCulture).Subtract(DataMapper.UNIX_TIME).TotalSeconds,
                DateTo = (long)DateTime.ParseExact(stationDataMapper.GetDataFromString(line, StationDataMapper.BIS), "yyyyMMdd", CultureInfo.InvariantCulture).Subtract(DataMapper.UNIX_TIME).TotalSeconds,
                StationHeight = int.Parse(stationDataMapper.GetDataFromString(line, StationDataMapper.ST_HOEHE)),
                Latitude = float.Parse(stationDataMapper.GetDataFromString(line, StationDataMapper.GEO_BREITE), CultureInfo.InvariantCulture),
                Longitude = float.Parse(stationDataMapper.GetDataFromString(line, StationDataMapper.GEO_LAENGE), CultureInfo.InvariantCulture),
                StationName = stationDataMapper.GetDataFromString(line, StationDataMapper.STATIONSNAME),
                FederalState = new FederalState(stationDataMapper.GetDataFromString(line, StationDataMapper.BUNDESLANDNAME))                                
            };
        }

    }
}
