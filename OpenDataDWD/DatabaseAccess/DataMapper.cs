using System;
using System.Collections.Generic;

namespace DatabaseAccess
{
    /// <summary>
    /// Mapper Class to extract the datafields from a given string
    /// String has to be in the correct Format!
    /// </summary>
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

    /// <summary>
    /// Defines start and length of a datafield in a string. 
    /// </summary>
    struct DataFieldDefinition
    {
        public int Start;
        public int Length;
    }
}
