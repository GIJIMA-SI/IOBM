using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class AdvancedSearchFieldModel
    {
        #region Properties & Attributes
        
        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public AdvancedSearchFieldModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Get all the rows in the advanced search entity
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<AdvancedSearchField> ReadAdvancedSearchColumns()
        {
            using (var db = MobileManagerEntities.GetContext())
            {
                IEnumerable<AdvancedSearchField> advancedSearchFields = db.AdvancedSearchFields.ToList();
                return new ObservableCollection<AdvancedSearchField>(advancedSearchFields);
            }
        }
        
        #endregion
    }
}
