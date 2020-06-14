﻿using DatabaseAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{

    class StationDataMapper : DataMapper
    {
        public static readonly int ST_KE = 0;
        public static readonly int ST_ID = 1;
        public static readonly int VON = 2;
        public static readonly int BIS = 3;
        public static readonly int ST_HOEHE = 4;
        public static readonly int GEO_BREITE = 5;
        public static readonly int GEO_LAENGE = 6;
        public static readonly int STATIONSNAME = 7;
        public static readonly int BUNDESLANDNAME = 8;

        protected override List<DataFieldDefinition> GetMapperList()
        {
            return new List<DataFieldDefinition>
                {
                    new DataFieldDefinition { Start=0,  Length=5  }, // ST_KE
                    new DataFieldDefinition { Start=6,  Length=5  }, // ST_ID
                    new DataFieldDefinition { Start=12, Length=8  }, // von
                    new DataFieldDefinition { Start=21, Length=8  }, // bis
                    new DataFieldDefinition { Start=31, Length=8  }, // ST_Hoehe
                    new DataFieldDefinition { Start=40, Length=10 }, // geo_Breite
                    new DataFieldDefinition { Start=51, Length=10 }, // geo_Laenge
                    new DataFieldDefinition { Start=62, Length=25 }, // Stationsname
                    new DataFieldDefinition { Start=88, Length=25 }, // Bundeslandname
                };
        }

    }
}
