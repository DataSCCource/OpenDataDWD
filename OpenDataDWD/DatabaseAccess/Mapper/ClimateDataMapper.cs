using System.Collections.Generic;

namespace DatabaseAccess
{
    /// <summary>
    /// Mapper class to define where to find which datafields in a ClimateData string
    /// </summary>
    class ClimateDataMapper : DataMapper
    {
        public static readonly int STAT = 0;  // Stationnumber
        public static readonly int DATUM = 1; // Date of measurement
        public static readonly int PM = 2;    // Daily average of air preasure
        public static readonly int TXK = 3;   // Daily maximum temperature
        public static readonly int TNK = 4;   // Daily minimum temperature
        public static readonly int UPM = 5;   // Daily average of humidity
        public static readonly int FMK = 6;   // Daily average of wind strength
        public static readonly int SDK = 7;   // Amount of sunsine

        /// <summary>
        /// Get List of DataFieldDefinitions for climatedata in KL- and KX-format
        /// </summary>
        /// <returns>List of DataFieldDefinition of a Climatedata String</returns>
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
