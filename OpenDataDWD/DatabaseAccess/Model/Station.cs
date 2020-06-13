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
                + " " + GetDataFromDateTime().ToString("yyyy-MM-dd mm:ss");
        }

        public DateTime GetDataFromDateTime()
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(DateFrom).ToLocalTime();
            return dtDateTime;
        }

        public DateTime GetDataToDateTime()
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(DateTo).ToLocalTime();
            return dtDateTime;
        }
    }
}
