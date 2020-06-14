using System.Collections.Generic;

namespace DatabaseAccess
{
    class ClimateDataDataMapper : DataMapper
    {
        public static readonly int STAT = 0;  // Stationsnummer
        public static readonly int DATUM = 1; // Datum der Messung
        public static readonly int PM = 2;    // Tagesmittel des Luftdruckes in Stationshoehe 
        public static readonly int TXK = 3;   // Tagesmaximum der Temperatur der Luft in 2m Hoehe 
        public static readonly int TNK = 4;   // Tagesminimum der Temperatur der Luft in 2m Hoehe 
        public static readonly int UPM = 5;   // Tagesmittel der relativen Feuchte 
        public static readonly int FMK = 6;   // Tagesmittel der Windstaerke 
        public static readonly int SDK = 7;   // Tagessumme der Sonnenscheindauer 

        /// <summary>
        /// Get List of DataFieldDefinitions for climatedata in KL- and KX-format
        /// </summary>
        /// <returns></returns>
        protected override List<DataFieldDefinition> GetMapperList()
        {
            return new List<DataFieldDefinition>
                {
                    new DataFieldDefinition { Start=2,   Length=5 }, // STAT
                    new DataFieldDefinition { Start=7,   Length=8 }, // DATUM
                    new DataFieldDefinition { Start=37,  Length=5 }, // PM
                    new DataFieldDefinition { Start=43,  Length=4 }, // TXK
                    new DataFieldDefinition { Start=48,  Length=4 }, // TNK
                    new DataFieldDefinition { Start=125, Length=3 }, // UPM
                    new DataFieldDefinition { Start=160, Length=3 }, // FMK
                    new DataFieldDefinition { Start=195, Length=3 }, // SDK
                };
        }
    }
}
