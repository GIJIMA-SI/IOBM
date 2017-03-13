using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DataUpdateSearchPropertyModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private DataActivityHelper _dataActivityHelper = null;
        private string _defaultItem = "-- Please Select --";

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataUpdateSearchPropertyModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Get all the search properties as an observablecollection of string
        /// </summary>
        /// <param name="Enum"></param>
        /// <returns></returns>
        public ObservableCollection<string> GetSearchProperties(DataUpdateEntity Enum)
        {
            using (var db = MobileManagerEntities.GetContext())
            {
                int enEntity = Enum.Value();
                DataUpdateSearchProperty searchProperty = db.DataUpdateSearchProperties.Where(p => p.enDataEntity == enEntity).FirstOrDefault();
                IEnumerable<string> searchProperties = searchProperty.SearchProperties.Split(';').ToList();
                ObservableCollection<string> searchPropertiesCollection = new ObservableCollection<string>(searchProperties);
                searchPropertiesCollection.Insert(0, _defaultItem);
                return searchPropertiesCollection;
            }
        }
    }
}
