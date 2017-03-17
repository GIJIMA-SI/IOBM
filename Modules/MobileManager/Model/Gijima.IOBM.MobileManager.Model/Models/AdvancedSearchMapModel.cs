using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class AdvancedSearchMapModel
    {
        private IEventAggregator _eventAggregator;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="EventAggregator"></param>
        public AdvancedSearchMapModel(IEventAggregator EventAggregator)
        {
            _eventAggregator = EventAggregator;
        }

        public string GetJoinsBetween(string Entity1, string Column1, string Entity2, string Column2)
        {
            
            string joinLink = "";

            if (Column1.StartsWith("fk"))
            {
                string mainTable = Column1.Replace("fk", "").Replace("ID", "");
                joinLink = $"SELECT * FROM {mainTable} INNER JOIN {Entity1} ON {Entity1}.{Column1} = {mainTable}.pk{mainTable}ID ";
            }
            else
            { joinLink = $"SELECT * FROM {Entity1} "; }



            return joinLink;
        }

        /// <summary>
        /// Returns a list of all the AdvancedSearchMap rows
        /// </summary>
        public IEnumerable<AdvancedSearchMap> ReadAdvancedSearchMappings()
        {
            using (var db = MobileManagerEntities.GetContext())
            {
                IEnumerable<AdvancedSearchMap> AdvancedSearchMapping = ((DbQuery<AdvancedSearchMap>)(from AdvancedSearchMap in db.AdvancedSearchMaps
                                                                      select AdvancedSearchMap)).ToList();
                return AdvancedSearchMapping;
            }
        }
    }
}
