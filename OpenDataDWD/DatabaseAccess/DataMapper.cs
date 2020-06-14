using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    abstract class DataMapper
    {
        public static readonly DateTime UNIX_TIME = new DateTime(1970, 1, 1);

        public string GetDataFromString(string line, int dataNumber)
        {
            var mapper = GetMapperList();
            return line.Substring(mapper[dataNumber].Start, mapper[dataNumber].Length).Trim();
        }

        /// <summary>
        /// Get List of Datafield definitions
        /// </summary>
        /// <returns></returns>
        protected abstract List<DataFieldDefinition> GetMapperList();
    }

    struct DataFieldDefinition
    {
        public int Start;
        public int Length;
    }
}
