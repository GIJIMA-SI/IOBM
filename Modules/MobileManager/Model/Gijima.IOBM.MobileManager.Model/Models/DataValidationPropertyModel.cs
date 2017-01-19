using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Helpers;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DataValidationPropertyModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataValidationPropertyModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new data validation property in the database
        /// </summary>
        /// <param name="validationProperty">The data validation property to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataValidationProperty(DataValidationProperty validationProperty)
        {
            try
            {
                bool result = false;

                using (var db = MobileManagerEntities.GetContext())
                {
                    // Check for duplicate internal or external data properties
                    if ((DataValidationGroupName)validationProperty.enDataValidationGroupName == DataValidationGroupName.ExternalData)
                    {
                        result = db.DataValidationProperties.Any(p => p.enDataValidationGroupName == validationProperty.enDataValidationGroupName &&
                                                                      p.enDataValidationEntity == validationProperty.enDataValidationEntity &&
                                                                      p.ExtDataValidationProperty == validationProperty.ExtDataValidationProperty);
                    }
                    else
                    {
                        result = db.DataValidationProperties.Any(p => p.enDataValidationGroupName == validationProperty.enDataValidationGroupName &&
                                                                      p.enDataValidationProperty == validationProperty.enDataValidationProperty);
                    }


                    if (!result)
                    {
                        db.DataValidationProperties.Add(validationProperty);
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationPropertyModel",
                                         string.Format("Error! {0}, {1}.",
                                         ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                         "CreateDataValidationProperty",
                                         ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all or active only data validation properties from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of DataValidationProperties</returns>
        public ObservableCollection<DataValidationProperty> ReadDataValidationProperties(DataValidationGroupName dataValidationGroup, bool activeOnly)
        {
            try
            {
                IEnumerable<DataValidationProperty> validationRulesData = null;
                short dataValidationGroupID = dataValidationGroup.Value();

                using (var db = MobileManagerEntities.GetContext())
                {
                    validationRulesData = ((DbQuery<DataValidationProperty>)(from validationProperty in db.DataValidationProperties
                                                                             where validationProperty.enDataValidationGroupName == 0 ||
                                                                                   validationProperty.enDataValidationGroupName == dataValidationGroupID
                                                                             select validationProperty)).ToList();

                    if (activeOnly)
                        validationRulesData = validationRulesData.Where(p => p.IsActive);

                    return new ObservableCollection<DataValidationProperty>(validationRulesData);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationPropertyModel",
                                         string.Format("Error! {0}, {1}.",
                                         ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                         "ReadDataValidationProperties",
                                         ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read the external data validation properties from the database
        /// </summary>
        /// <param name="externalDataName">The external data table name.</param>
        /// <returns>DataTable</returns>
        public IEnumerable<DataValidationProperty> ReadExtDataValidationProperties(int validationGroupID, int validationEntityID)
        {
            try
            {
                IEnumerable<DataValidationProperty> properties = null;
                
                using (var db = MobileManagerEntities.GetContext())
                {
                    properties = ((DbQuery<DataValidationProperty>)(from validationProperty in db.DataValidationProperties
                                                                    where validationProperty.enDataValidationGroupName == 0 ||
                                                                          (validationProperty.enDataValidationGroupName == validationGroupID &&
                                                                           validationProperty.enDataValidationEntity == validationEntityID)
                                                                    select validationProperty)).ToList();

                    return new ObservableCollection<DataValidationProperty>(properties);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationPropertyModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadExternalDataPropertiesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing data validation property in the database
        /// </summary>
        /// <param name="validationProperty">The data validation property to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDataValidationProperty(DataValidationProperty validationProperty)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationProperty existingDataValidationProperty = db.DataValidationProperties.Where(p => p.enDataValidationEntity == validationProperty.enDataValidationEntity &&
                                                                                                                   p.enDataValidationProperty == validationProperty.enDataValidationProperty).FirstOrDefault();

                    // Check to see if the data validation rule property already exist for another entity 
                    if (existingDataValidationProperty != null && existingDataValidationProperty.pkDataValidationPropertyID != validationProperty.pkDataValidationPropertyID)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DataValidationPropertyModel",
                                                 string.Format("The data validation property {0} already exist.",
                                                 ((DataValidationPropertyName)validationProperty.enDataValidationProperty).ToString()),
                                                 "UpdateDataValidationProperty",
                                                 ApplicationMessage.MessageTypes.SystemError));
                        return false;
                    }
                    else
                    {
                        if (existingDataValidationProperty != null)
                        {
                            existingDataValidationProperty.enDataValidationEntity = validationProperty.enDataValidationEntity;
                            existingDataValidationProperty.enDataValidationProperty = validationProperty.enDataValidationProperty;
                            existingDataValidationProperty.ModifiedBy = validationProperty.ModifiedBy;
                            existingDataValidationProperty.ModifiedDate = validationProperty.ModifiedDate;
                            db.SaveChanges();
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationPropertyModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadExternalDataPropertiesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
