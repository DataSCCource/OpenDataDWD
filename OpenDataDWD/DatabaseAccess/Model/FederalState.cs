using System;

namespace DatabaseAccess
{
    public class FederalState
    {
        public long Id { get; private set; }
        public string FederalStateName { get; set; }

        public FederalState(long id) : this(id, "empty") { }

        public FederalState(string federalStateName) : this(0, federalStateName){}
                
        public FederalState(long id, string federalStateName)
        {
            this.Id = id;
            this.FederalStateName = federalStateName;
        }
    }
}