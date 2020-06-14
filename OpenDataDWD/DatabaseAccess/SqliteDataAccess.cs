using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace DatabaseAccess
{
    public class SqliteDataAccess : IDatabaseAccess
    {
        /// <summary>
        /// Load Connection String for SQLite from the Settings
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        /// <inheritdoc/>
        public List<Station> LoadStations()
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                var sql = @"SELECT * FROM Stations s INNER JOIN FederalStates fs ON fs.Id = s.fk_FederalState";
                var output = conn.Query<Station, FederalState, Station>(sql, (station, federalstate) => { station.FederalState = federalstate; return station; });

                return output.ToList();
            }
        }

        /// <inheritdoc/>
        public void SaveStation(Station station)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                string sql = "INSERT OR IGNORE INTO Stations " +
                    "(Id, StationKE, StationName, DateFrom, DateTo, StationHeight, Latitude, Longitude, fk_FederalState) " 
                    + "SELECT @Id, @StationKE, @StationName, @DateFrom, @DateTo, @StationHeight, @Latitude, @Longitude, Id "
                    + $"FROM FederalStates WHERE FederalStateName='{station.FederalState.FederalStateName}'";

                conn.Execute(sql, station);
            }
        }

        /// <inheritdoc/>
        public void SaveFederalState(FederalState federalState)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Execute("INSERT OR IGNORE INTO FederalStates (FederalStateName) VALUES (@FederalStateName)", federalState);
            }
        }

        /// <inheritdoc/>
        public bool StationsDbExists()
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                var resFederalStates = conn.Query("SELECT name FROM sqlite_master WHERE type='table' and name='FederalStates'");
                var resStations = conn.Query("SELECT name FROM sqlite_master WHERE type='table' and name='Stations'");

                return resFederalStates.Count() > 0 && resStations.Count() > 0;
            }
        }

        /// <inheritdoc/>
        public void CreateStationsDb()
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                // Create Table FederalStates if not exists
                conn.Execute(@"CREATE TABLE IF NOT EXISTS 'FederalStates' (
                                'Id'    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                'FederalStateName'  TEXT NOT NULL UNIQUE
                            );");

                // Create Table Stations if not exists
                conn.Execute(@"CREATE TABLE IF NOT EXISTS 'Stations' (
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

        /// <inheritdoc/>
        public List<ClimateData> LoadClimateData(string stationId, DateTime dateFrom, DateTime dateTo)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                long secondsFrom = (long)dateFrom.Subtract(DataMapper.UNIX_TIME).TotalSeconds;
                long secondsTo = (long)dateTo.Subtract(DataMapper.UNIX_TIME).TotalSeconds;

                var sql = $"SELECT * FROM ClimateData WHERE StationId='{stationId}' AND Date>={secondsFrom} AND Date<={secondsTo}";
                var output = conn.Query<ClimateData>(sql);

                return output.ToList();
            }
        }

        /// <inheritdoc/>
        public void CreateClimateDataDb()
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                // Create Table ClimateData if not exists
                conn.Execute(@"CREATE TABLE IF NOT EXISTS 'ClimateData' (
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

        /// <inheritdoc/>
        public void SaveClimateData(List<ClimateData> climateData)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                foreach (var cData in climateData)
                {
                    conn.Execute(@"INSERT INTO ClimateData 
                                (StationId, StationNumber, Date, PressureMiddle, TempMax, TempMin, HumidityMiddle, WindForceMiddle, SunshineSum) VALUES 
                                (@StationId, @StationNumber, @Date, @PressureMiddle, @TempMax, @TempMin, @HumidityMiddle, @WindForceMiddle, @SunshineSum)", cData);
                }
            }
        }

        /// <inheritdoc/>
        public bool ClimateDataDbExists()
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                var resClimateData = conn.Query("SELECT name FROM sqlite_master WHERE type='table' and name='ClimateData'");
                return resClimateData.Count() > 0;
            }
        }
    }
}
