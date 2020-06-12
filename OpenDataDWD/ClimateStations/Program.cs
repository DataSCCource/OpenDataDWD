using OpenDataDWD;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimateStations
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] stationsStrings = File.ReadAllLines("../../Daten/KL_Standardformate_Beschreibung_Stationen.txt");

            foreach (var line in stationsStrings)
            {
                if(line.StartsWith("10"))
                {
                    Station station = GetStationFromString(line);
                    Console.WriteLine(station);

                }
            }

            string[] data = File.ReadAllLines("../../Daten/kl_10004_00_akt.txt");

            foreach (var line in data)
            {
                if (line.StartsWith("KL"))
                {

                } 
                else if(line.StartsWith("KX"))
                {

                }
            }
        }

        static Station GetStationFromString(string line)
        {
            var sMapper = GetStationMappers();
            return new Station()
            {
                StationKE = line.Substring(sMapper[0].Start, sMapper[0].Length).Trim(),
                StationID = int.Parse(line.Substring(sMapper[1].Start, sMapper[1].Length).Trim()),
                DataFrom = DateTime.ParseExact(line.Substring(sMapper[2].Start, sMapper[2].Length).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture),
                DataTo = DateTime.ParseExact(line.Substring(sMapper[3].Start, sMapper[3].Length).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture),
                StationHeight = int.Parse(line.Substring(sMapper[4].Start, sMapper[4].Length).Trim()),
                Latitude = float.Parse(line.Substring(sMapper[5].Start, sMapper[5].Length).Trim(), CultureInfo.InvariantCulture),
                Longitude = float.Parse(line.Substring(sMapper[6].Start, sMapper[6].Length).Trim(), CultureInfo.InvariantCulture),
                StationName = line.Substring(sMapper[7].Start, sMapper[7].Length).Trim(),
                FederalState = line.Substring(sMapper[8].Start, sMapper[8].Length).Trim(),
            };
        }

        static List<Mapper> GetStationMappers()
        {
            return new List<Mapper>
            {
                new Mapper { Start=0, Length=5 },
                new Mapper { Start=6, Length=5 },
                new Mapper { Start=12, Length=8 },
                new Mapper { Start=21, Length=8 },
                new Mapper { Start=31, Length=8 },
                new Mapper { Start=40, Length=10 },
                new Mapper { Start=51, Length=10 },
                new Mapper { Start=62, Length=25 },
                new Mapper { Start=88, Length=25 },
            };
        }

    }

    struct Mapper
    {
        public int Start;
        public int Length;
    }
}
