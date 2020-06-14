using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    public class Station
    {
        public int Id { get; set; }
        public string StationKE { get; set; }
        public string StationName { get; set; }
        public int StationHeight { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public long DateFrom { get; set; }
        public long DateTo { get; set; }
        public FederalState FederalState { get; set; }

        public override string ToString()
        {
            return StationKE + " " + DateFrom.ToString() + " " + Latitude
                + " " + GetDataFromDateTime().ToString("yyyy-MM-dd mm:ss") + " | " + DateFrom;
        }

        public DateTime GetDataFromDateTime()
        {
            return DataMapper.UNIX_TIME.AddSeconds(DateFrom).ToLocalTime();
        }

        public DateTime GetDataToDateTime()
        {
            return DataMapper.UNIX_TIME.AddSeconds(DateTo).ToLocalTime();
        }
    }
}
