﻿using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DataImportPropertyModel
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
        public DataImportPropertyModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Return required property collection
        /// </summary>
        /// <param name="Enum"></param>
        /// <returns></returns>
        public ObservableCollection<DataImportProperty> GetProperties(DataImportEntity Enum)
        {
            using (var db = MobileManagerEntities.GetContext())
            {
                int enEntity = Enum.Value();
                IEnumerable<DataImportProperty> properties = db.DataImportProperties.Where(p => p.enDataEntity == enEntity).ToList();
                return new ObservableCollection<DataImportProperty>(properties);
            }
        }

        /// <summary>
        /// Return required property description collection
        /// </summary>
        /// <param name="Enum"></param>
        /// <returns></returns>
        public ObservableCollection<string> GetPropertiesDescription(DataImportEntity Enum)
        {
            using (var db = MobileManagerEntities.GetContext())
            {
                int enEntity = Enum.Value();
                ObservableCollection<string> properties = new ObservableCollection<string>();
                properties.Add(_defaultItem);
                foreach (DataImportProperty property in db.DataImportProperties.Where(p => p.enDataEntity == enEntity).ToList())
                {
                    properties.Add(property.PropertyDescription);
                }
                return properties;
            }
        }

        /// <summary>
        /// Return required properties as a DataImportProperty collection
        /// </summary>
        /// <param name="Enum"></param>
        /// <returns></returns>
        public ObservableCollection<DataImportProperty> GetPropertiesCollection(DataImportEntity Enum)
        {
            using (var db = MobileManagerEntities.GetContext())
            {
                int enEntity = Enum.Value();
                ObservableCollection<DataImportProperty> properties = new ObservableCollection<DataImportProperty>();

                //Create the default dataImportProperty
                DataImportProperty dataImportPorperty = new DataImportProperty();
                dataImportPorperty.enDataEntity = 0;
                dataImportPorperty.PropertyDescription = _defaultItem;
                dataImportPorperty.PropertyName = "";
                dataImportPorperty.MultipleProperty = false;
                dataImportPorperty.Required = false;

                properties.Add(dataImportPorperty);
                foreach (DataImportProperty property in db.DataImportProperties.Where(p => p.enDataEntity == enEntity).ToList())
                {
                    properties.Add(property);
                }
                return properties;
            }
        }

        /// <summary>
        /// Return true if property can have multiple mappings
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="enDataEntity"></param>
        /// <returns></returns>
        public bool HasMultipleProperty(string Description, short enDataEntity)
        {
            if (Description != _defaultItem)
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.DataImportProperties.Where(p => p.PropertyDescription == Description && p.enDataEntity == enDataEntity).FirstOrDefault().MultipleProperty;
                }
            }
            else
                return false;
        }

        public string GetPropertyName(string PropertyDescription, short DataEntity)
        {
            using (var db = MobileManagerEntities.GetContext())
            {
                string name = db.DataImportProperties.Where(p => p.PropertyDescription == PropertyDescription && p.enDataEntity == DataEntity).FirstOrDefault().PropertyName;
                return name != null ? name : "";
            }
        }
    }
}
