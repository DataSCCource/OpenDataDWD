using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    public class SqliteDataAccess : IDatabaseAccess
    {
        public List<Station> LoadStations()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var sql = @"SELECT * FROM Stations s INNER JOIN FederalStates fs ON fs.Id = s.fk_FederalState";
                var output = cnn.Query<Station, FederalState, Station>(sql, (station, federalstate) => { station.FederalState = federalstate; return station; });

                return output.ToList();
            }
        }

        public void SaveStation(Station station)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "INSERT OR IGNORE INTO Stations " +
                    "(Id, StationKE, StationName, DateFrom, DateTo, StationHeight, Latitude, Longitude, fk_FederalState) " 
                    + "SELECT @Id, @StationKE, @StationName, @DateFrom, @DateTo, @StationHeight, @Latitude, @Longitude, Id "
                    + $"FROM FederalStates WHERE FederalStateName='{station.FederalState.FederalStateName}'";

                cnn.Execute(sql, station);
            }
        }

        public void SaveFederalState(FederalState federalState)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT OR IGNORE INTO FederalStates (FederalStateName) VALUES (@FederalStateName)", federalState);
            }
        }

        private string LoadConnectionString(string id = "Default") 
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        public bool StationsDbExists()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var resFederalStates = cnn.Query("SELECT name FROM sqlite_master WHERE type='table' and name='FederalStates'");
                var resStations = cnn.Query("SELECT name FROM sqlite_master WHERE type='table' and name='Stations'");

                return resFederalStates.Count() > 0 && resStations.Count() > 0;
            }
        }

        public void CreateStationsDb()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                // Create Table FederalStates if not exists
                cnn.Execute(@"CREATE TABLE IF NOT EXISTS 'FederalStates' (
                                'Id'    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                'FederalStateName'  TEXT NOT NULL UNIQUE
                            );");

                // Create Table Stations if not exists
                cnn.Execute(@"CREATE TABLE IF NOT EXISTS 'Stations' (
                                'StationKE' TEXT NOT NULL UNIQUE,
                                'Id'    INTEGER NOT NULL UNIQUE,
                                'StationName'   TEXT NOT NULL,
                                'DateFrom'  INTEGER NOT NULL,
                                'DateTo'    INTEGER NOT NULL,
                                'StationHeight' INTEGER DEFAULT 0,
                                'Latitude'  REAL NOT NULL,
                                'Longitude' REAL NOT NULL,
                                'fk_FederalState'   INTEGER NOT NULL,
                                PRIMARY KEY('StationKE'),
                                FOREIGN KEY('fk_FederalState') REFERENCES 'FederalStates'('Id') ON DELETE SET DEFAULT ON UPDATE CASCADE
                            ); ");


            }
        }


        public List<ClimateData> LoadClimateData(string stationId, DateTime dateFrom, DateTime dateTo)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                long secondsFrom = (long)dateFrom.Subtract(DataMapper.UNIX_TIME).TotalSeconds;
                long secondsTo = (long)dateTo.Subtract(DataMapper.UNIX_TIME).TotalSeconds;

                var sql = $"SELECT * FROM ClimateData WHERE StationId='{stationId}' AND Date>={secondsFrom} AND Date<={secondsTo}";
                var output = cnn.Query<ClimateData>(sql);

                return output.ToList();
            }
        }

        public void CreateClimateDataDb()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                // Create Table ClimateData if not exists
                cnn.Execute(@"CREATE TABLE IF NOT EXISTS 'ClimateData' (
                                'Id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                'StationId' TEXT NOT NULL,
                                'StationNumber' INTEGER NOT NULL,
                                'Date'  INTEGER NOT NULL,
                                'PressureMiddle'    REAL,
                                'TempMax'   REAL,
                                'TempMin'   REAL,
                                'HumidityMiddle'    INTEGER,
                                'WindForceMiddle'   REAL,
                                'SunshineSum'   REAL,
                            	FOREIGN KEY('StationId') REFERENCES 'Stations'('StationKE') ON DELETE SET DEFAULT ON UPDATE CASCADE
                            ); ");
            }
        }

        public void SaveClimateData(List<ClimateData> climateData)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                foreach (var cd in climateData)
                {
                    cnn.Execute(@"INSERT INTO ClimateData 
                                (StationId, StationNumber, Date, PressureMiddle, TempMax, TempMin, HumidityMiddle, WindForceMiddle, SunshineSum) VALUES 
                                (@StationId, @StationNumber, @Date, @PressureMiddle, @TempMax, @TempMin, @HumidityMiddle, @WindForceMiddle, @SunshineSum)", cd);
                }
            }
        }

        public bool ClimateDataDbExists()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var resClimateData = cnn.Query("SELECT name FROM sqlite_master WHERE type='table' and name='ClimateData'");
                return resClimateData.Count() > 0;
            }
        }
    }
}
