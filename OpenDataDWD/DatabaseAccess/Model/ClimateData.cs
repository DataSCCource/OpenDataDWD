using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    public class ClimateData
    {
        public int Id { get; set; }
        public string StationId { get; set; }
        public int StationNumber { get; set; }
        public long Date { get; set; }
        public float PressureMiddle { get; set; }
        public float TempMax { get; set; }
        public float TempMin { get; set; }
        public int HumidityMiddle { get; set; }
        public float WindForceMiddle { get; set; }
        public float SunshineSum { get; set; }

        public override string ToString()
        {
            return GetDate().ToString("yyyy-MM-dd mm:ss") + " | " + StationId + " " + StationNumber + " " 
                + PressureMiddle + " " + TempMax + " " + TempMin + " " + HumidityMiddle + " " + SunshineSum;
        }

        public DateTime GetDate()
        {
            return DataMapper.UNIX_TIME.AddSeconds(Date).ToLocalTime();
        }

        public enum DataTypes
        {
            Temperatur_Tagesmaximum, Temperatur_Tagesminimum, Luftdruck_Tagesmittel, Relative_Luftfeuchte, Windstaerke_Tagesmittel, Sonnenscheindauer_Tagessumme
        }
    }
}
