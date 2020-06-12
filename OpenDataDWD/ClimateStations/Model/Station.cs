using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDataDWD
{
    class Station
    {
        public string StationKE { get; set; }
        public int StationID { get; set; }
        public DateTime DataFrom { get; set; }
        public DateTime DataTo { get; set; }
        public int StationHeight { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string StationName { get; set; }
        public string FederalState { get; set; }


        public override string ToString()
        {
            return StationKE + " " + DataFrom.ToString("yyyy-MM-dd") + " " + Latitude + " " + FederalState;
        }
    }
}
