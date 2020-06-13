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

        void SaveStation(Station station);

        void SaveFederalState(FederalState federalState);
    }
}
