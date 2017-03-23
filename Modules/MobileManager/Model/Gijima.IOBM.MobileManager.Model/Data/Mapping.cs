using Gijima.IOBM.MobileManager.Model.Data;
using System.Collections.Generic;


namespace Gijima.IOBM.MobileManager.Common.Structs
{
    public class Mapping
    {
        /// <summary>
        /// The AdvancedSearch object name
        /// </summary>
        public string Me
        {
            get { return _me; }
            set { _me = value; }
        }
        private string _me;

        /// <summary>
        /// All the joins to me
        /// </summary>
        public List<AdvancedSearchMap> Connections
        {
            get { return _connections; }
            set { _connections = value; }
        }
        private List<AdvancedSearchMap> _connections;

        /// <summary>
        /// Constructor
        /// </summary>
        public Mapping()
        {
            Connections = new List<AdvancedSearchMap>();
        }
    }
}
