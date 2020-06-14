using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    public interface IDatabaseAccess
    {
        List<Station> LoadStations();

        List<ClimateData> LoadClimateData(string stationId);

        bool StationsDbExists();

        bool ClimateDataDbExists();

        void CreateStationsDb();

        void CreateClimateDataDb();

        void SaveStation(Station station);

        void SaveClimateData(List<ClimateData> climateData);

        void SaveFederalState(FederalState federalState);
    }
}
