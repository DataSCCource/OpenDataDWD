using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDataDWD.Model
{
    class ClimateData
    {
        public string StationKE { get; set; }
        public DateTime DataFrom { get; set; }
        public int BarometricPressure_Morning { get; set; }
        public int BarometricPressure_Midday { get; set; }
        public int BarometricPressure_Evening { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string StationName { get; set; }
        public string FederalState { get; set; }

        public override string ToString()
        {
            return StationKE + " " + DataFrom.ToString() + " " + Latitude + " " + FederalState;
        }
    }
}
