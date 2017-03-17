using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
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

        /// <summary>
        /// Delete an existing data validation property in the database
        /// </summary>
        /// <param name="validationProperty">The data validation property to delete.</param>
        /// <returns>True if successfull</returns>
        public bool DeleteDataValidationProperty(DataValidationProperty validationProperty)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    // First delete all the exceptions link to the property
                    if (new DataValidationExceptionModel(_eventAggregator).DeleteDataValidationExceptions(validationProperty.pkDataValidationPropertyID))
                    {
                        // Get the property to delete
                        DataValidationProperty propertyToDelete = db.DataValidationProperties.Where(p => p.pkDataValidationPropertyID == validationProperty.pkDataValidationPropertyID).FirstOrDefault();

                        if (propertyToDelete != null)
                        {
                            db.DataValidationProperties.Remove(propertyToDelete);
                            db.SaveChanges();
                        }
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
                                                                "DeleteDataValidationProperty",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the external data validation rules based on the specified data file properties
        /// </summary>
        /// <param name="externalBillingData">The external billing data entity to validate against.</param>
        /// <returns>True if successfull</returns>
        public bool ValidateExtDataValidationRules(ExternalBillingData externalBillingData)
        {
            try
            {
                DataValidationException validationResult = null;
                bool result = true;
                bool invalidRule = false;

                using (var db = MobileManagerEntities.GetContext())
                {
                    // Get all the properties of the external data file
                    IEnumerable<string> externalFileProperties = new ExternalBillingDataModel(_eventAggregator).ReadExternalBillingDataProperties(externalBillingData.TableName);

                    if (externalFileProperties.Count() > 0)
                    {
                        // Get all the data validation properties for the external data file
                        IEnumerable<DataValidationProperty> dataValidationProperties = new DataValidationPropertyModel(_eventAggregator).ReadExtDataValidationProperties(DataValidationGroupName.ExternalData.Value(), externalBillingData.pkExternalBillingDataID);

                        foreach (DataValidationProperty property in dataValidationProperties)
                        {
                            result = true;

                            // Initialise the progress values
                            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                            {
                                CurrentValue = 1,
                                MaxValue = dataValidationProperties.Count(),
                            });

                            // Validate if an existing data validation property
                            // exists in the external data file, If it does not exist
                            // then check if there is a rule linked to it. If there is a
                            // rule linked to it then show rule exception error else delete the 
                            // data validation property 
                            if (!externalFileProperties.Contains(property.ExtDataValidationProperty))
                            {
                                // Check to see if any rules are linked to the property
                                if (db.DataValidationRules.Any(p => p.enDataValidationGroupName == property.enDataValidationGroupName &&
                                                                    p.DataValidationEntityID == property.enDataValidationEntity &&
                                                                    p.fkDataValidationPropertyID == property.pkDataValidationPropertyID))
                                {
                                    result = false;
                                    invalidRule = true;
                                }

                                // If no rules are linked
                                // then delete the property
                                if (result)
                                    DeleteDataValidationProperty(property);
                            }

                            // Update the validation result values
                            validationResult = new DataValidationException();
                            if (result)
                            {
                                validationResult = new DataValidationException()
                                {
                                    fkBillingProcessID = BillingExecutionState.ExternalDataRuleValidation.Value(),
                                    fkDataValidationPropertyID = property.pkDataValidationPropertyID,
                                    BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                    enDataValidationGroupName = DataValidationGroupName.ExternalData.Value(),
                                    DataValidationEntityID = (int)property.enDataValidationEntity,
                                    Result = true
                                };

                                _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                            }
                            else
                            {
                                validationResult = new DataValidationException()
                                {
                                    fkBillingProcessID = BillingExecutionState.ExternalDataRuleValidation.Value(),
                                    fkDataValidationPropertyID = property.pkDataValidationPropertyID,
                                    BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                    enDataValidationGroupName = DataValidationGroupName.ExternalData.Value(),
                                    DataValidationEntityID = (int)property.enDataValidationEntity,
                                    CanApplyRule = false,
                                    Message = string.Format("The data validation rule based on the {0} property in the {1} file is invalid. Please update or delete the rule.",
                                                            property.ExtDataValidationProperty.ToUpper(), externalBillingData.TableName),
                                    Result = false
                                };

                                _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                            }

                            // Update the progress values for the last property
                            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                            {
                                CurrentValue = dataValidationProperties.Count(),
                                MaxValue = dataValidationProperties.Count(),
                            });
                        }
                    }
                }

                return invalidRule ? false : true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationPropertyModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateExtDataValidationRules",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
