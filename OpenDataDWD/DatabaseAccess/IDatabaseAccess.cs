using System;
using System.Collections.Generic;

namespace DatabaseAccess
{
    /// <summary>
    /// Interface used to access the Database
    /// </summary>
    public interface IDatabaseAccess
    {
        /// <summary>
        /// Load list of Station-objects from the database
        /// </summary>
        /// <returns>list of Station-objects</returns>
        List<Station> LoadStations();

        /// <summary>
        /// Load list of ClimateData-objects from the database
        /// </summary>
        /// <param name="stationId">ID of the station</param>
        /// <param name="dateFrom">Filter data by date (lower end)</param>
        /// <param name="dateTo">Filter data by date (higher end)</param>
        /// <returns>list of ClimateData-objects</returns>
        List<ClimateData> LoadClimateData(string stationId, DateTime dateFrom, DateTime dateTo);

        /// <summary>
        /// Check if the Stations and FederalStates database already exists
        /// </summary>
        /// <returns>true if it exists, false otherwise</returns>
        bool StationsDbExists();

        /// <summary>
        /// Check if the ClimateData database already exists
        /// </summary>
        /// <returns>true if it exists, false otherwise</returns>
        bool ClimateDataDbExists();

        /// <summary>
        /// Create the Stations database if it does not exist already
        /// </summary>
        void CreateStationsDb();

        /// <summary>
        /// Create the ClimateData database if it does not exist already
        /// </summary>
        void CreateClimateDataDb();

        /// <summary>
        /// Save Station data to the database
        /// </summary>
        /// <param name="station">Station object to save in the database</param>
        void SaveStation(Station station);

        /// <summary>
        /// Save ClimateData to the database
        /// </summary>
        /// <param name="climateData">ClimateData object to save in the database</param>
        void SaveClimateData(List<ClimateData> climateData);

        /// <summary>
        /// Save a federal state to the database
        /// </summary>
        /// <param name="federalState">FederalState to save in the database</param>
        void SaveFederalState(FederalState federalState);
    }
}
