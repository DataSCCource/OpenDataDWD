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

        /// <summary>
        /// This project is used for accessing the database.
        /// The Main-Method in particular imports all stations and climatedata into the database.
        /// </summary>
        static void Main()
        {
            IDatabaseAccess dataAccess = new SqliteDataAccess();
            string pathToData = "../../Daten/";

            Console.Write("Importing stations ... ");
            var stations = ReadStationsFromFile(pathToData + "KL_Standardformate_Beschreibung_Stationen.txt");
            ImportStationsIntoDatabase(dataAccess, stations);
            Console.WriteLine("Done importing stations");
            Console.WriteLine(" --- ");


            // Data files have to be in the Daten-directory of this project to be recognized
            Console.WriteLine("Importing climatedata ...");
            var fileNames = Directory.GetFiles(pathToData);
            foreach (var file in fileNames.Where(fn => fn.Contains("_bis_1999.txt") || fn.Contains("_00_akt.txt")))
            {
                var climateData = ReadClimateDataFromFile(file);
                Console.Write("Importing: " + file +" ... ");
                ImportClimateDataIntoDatabase(dataAccess, climateData);
                Console.WriteLine("Done");
            }
        }

        /// <summary>
        /// Read climatedata from given file in KL- or KX- format
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>List with climate datasets</returns>
        private static List<ClimateData> ReadClimateDataFromFile(string path)
        {
            // Extract ID from filename
            string id = path.Substring(path.IndexOf("kl_") + 3, 5);
            
            List<ClimateData> climateData = new List<ClimateData>();
            var climatedataStrings = File.ReadAllLines(path, Encoding.GetEncoding(ANSI));

            // Use each line from the file to create the dataset
            climatedataStrings.Where(x => x.StartsWith("K")).ToList()
                           .ForEach(line => climateData.Add(GetClimateDataFromString(id, line)));

            return climateData;
        }

        /// <summary>
        /// Import a given list of climate data into the database
        /// </summary>
        /// <param name="dataAccess">Database Access object (e.g. SqliteDataAccess)</param>
        /// <param name="climateData">List of ClimateData objects</param>
        private static void ImportClimateDataIntoDatabase(IDatabaseAccess dataAccess, List<ClimateData> climateData)
        {
            if(!dataAccess.ClimateDataDbExists()) 
            {
                dataAccess.CreateClimateDataDb();
            }

            dataAccess.SaveClimateData(climateData);
        }

        /// <summary>
        /// Import a given list of climate stations into the database
        /// </summary>
        /// <param name="dataAccess">Database Access object (e.g. SqliteDataAccess)</param>
        /// <param name="stations">List of Station objects</param>
        private static void ImportStationsIntoDatabase(IDatabaseAccess dataAccess, List<Station> stations)
        {
            if(!dataAccess.StationsDbExists()) 
            {
                dataAccess.CreateStationsDb();
            }

            var districtNames = stations.Select(station => station.FederalState.FederalStateName).Distinct().ToList();
            foreach (var districtName in districtNames)
            {
                dataAccess.SaveFederalState(new FederalState(districtName ));
            }

            foreach (var station in stations)
            {
                dataAccess.SaveStation(station);
            }
        }

        /// <summary>
        /// Read the available climate stations from file and parse into list
        /// </summary>
        /// <returns>List of Station objects</returns>
        private static List<Station> ReadStationsFromFile(string path)
        {
            List<Station> stations = new List<Station>();
            var stationsStrings = File.ReadAllLines(path, Encoding.GetEncoding(ANSI));

            stationsStrings.Where(x => x.StartsWith("10")).ToList()
                           .ForEach(line => stations.Add(GetStationFromString(line)));

            return stations;
        }

        /// <summary>
        /// Extract a Station-object from a given string in the correct format! 
        /// (> see; KL_Standardformate_Beschreibung_Stationen.txt)
        /// </summary>
        /// <param name="line">String with Station data</param>
        /// <returns>Station object</returns>
        private static Station GetStationFromString(string line)
        {
            // Use StationDataMapper to extract the data
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

        /// <summary>
        /// Extract a ClimateData-object from a given string in the correct format! 
        /// </summary>
        /// <param name="stationId">Id of the climatestation that measured this dataset</param>
        /// <param name="line">String with ClimateData</param>
        /// <returns></returns>
        private static ClimateData GetClimateDataFromString(string stationId, string line)
        {
            // Use ClimateDataMapper to extract the data
            DataMapper stationDataMapper = new ClimateDataMapper();

            return new ClimateData()
            {
                Id = 0,
                StationId = stationId,
                StationNumber = int.Parse(stationDataMapper.GetDataFromString(line, ClimateDataMapper.STAT)),
                Date = (long)DateTime.ParseExact(stationDataMapper.GetDataFromString(line, ClimateDataMapper.DATUM), "yyyyMMdd", CultureInfo.InvariantCulture).Subtract(DataMapper.UNIX_TIME).TotalSeconds,
                PressureMiddle = float.Parse(stationDataMapper.GetDataFromString(line, ClimateDataMapper.PM))/10,

                TempMax = float.Parse(stationDataMapper.GetDataFromString(line, ClimateDataMapper.TXK))/10,
                TempMin = float.Parse(stationDataMapper.GetDataFromString(line, ClimateDataMapper.TNK))/10,
                HumidityMiddle = int.Parse(stationDataMapper.GetDataFromString(line, ClimateDataMapper.UPM)),
                WindForceMiddle = float.Parse(stationDataMapper.GetDataFromString(line, ClimateDataMapper.FMK))/10,
                SunshineSum = float.Parse(stationDataMapper.GetDataFromString(line, ClimateDataMapper.SDK))/10
            };
        }
    }
}
