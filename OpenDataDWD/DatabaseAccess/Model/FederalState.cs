using System;

namespace DatabaseAccess
{
    /// <summary>
    /// One Dataset of a Federal state
    /// </summary>
    public class FederalState
    {
        public long Id { get; private set; }
        public string FederalStateName { get; set; }

        /// <summary>
        /// Create a FederalState object
        /// </summary>
        /// <param name="federalStateName">Name of the federal state</param>
        public FederalState(string federalStateName) : this(0, federalStateName){}
               
        /// <summary>
        /// Create a FederalState object
        /// </summary>
        /// <param name="id">Id of the federal state</param>
        /// <param name="federalStateName">Name of the federal state</param>
        public FederalState(long id, string federalStateName)
        {
            this.Id = id;
            this.FederalStateName = federalStateName;
        }
    }
}