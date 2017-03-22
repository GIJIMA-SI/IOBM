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
