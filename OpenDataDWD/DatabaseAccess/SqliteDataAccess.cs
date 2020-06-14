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
                    "(Id, StationKE, StationName, DateFrom, DateTo, StationHeight, Latitude, Longitude, fk_FederalState) " +
                    $"SELECT @Id, @StationKE, @StationName, @DateFrom, @DateTo, @StationHeight, @Latitude, @Longitude, Id FROM FederalStates WHERE FederalStateName='{station.FederalState.FederalStateName}'";

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
                                'Id'    INTEGER NOT NULL UNIQUE,
                                'StationKE' TEXT NOT NULL UNIQUE,
                                'StationName'   TEXT NOT NULL,
                                'DateFrom'  INTEGER NOT NULL,
                                'DateTo'    INTEGER NOT NULL,
                                'StationHeight' INTEGER DEFAULT 0,
                                'Latitude'  REAL NOT NULL,
                                'Longitude' REAL NOT NULL,
                                'fk_FederalState'   INTEGER NOT NULL,
                                PRIMARY KEY('Id'),
                                FOREIGN KEY('fk_FederalState') REFERENCES 'FederalStates'('Id') ON DELETE SET DEFAULT ON UPDATE CASCADE
                            ); ");


            }
        }
    }
}
