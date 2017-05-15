using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DataValidationRuleModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private DataCompareHelper _dataComparer;
        private DataTable _externalData = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataValidationRuleModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _dataComparer = new DataCompareHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new data validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The data validation rule entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataValidationRule(DataValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DataValidationRules.Any(p => p.enDataValidationProcess == validationRule.enDataValidationProcess &&
                                                         p.enDataValidationGroupName == validationRule.enDataValidationGroupName &&
                                                         p.DataValidationEntityID == validationRule.DataValidationEntityID &&
                                                         p.fkDataValidationPropertyID == validationRule.fkDataValidationPropertyID &&
                                                         p.enOperatorType == validationRule.enOperatorType &&
                                                         p.enOperator == validationRule.enOperator))
                    {
                        db.DataValidationRules.Add(validationRule);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                        "The validation rule already exist.",
                                                                        "CreateDataValidationRule",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateDataValidationRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all the data validation rules for the specified group from the database
        /// </summary>
        /// <param name="validationProcess">The data validation process linked to the rules.</param>
        /// <param name="validationEntity">The data validation entity linked to the rules.</param>
        /// <returns>Collection of DataValidationRules</returns>
        public ObservableCollection<DataValidationRule> ReadDataValidationRules(DataValidationProcess validationProcess, DataValidationGroupName validationEntity)
        {
            try
            {
                List<DataValidationRule> validationRules = new List<DataValidationRule>();
                var rules = (dynamic)null;
                short processID = validationProcess.Value();
                short entityID = validationEntity.Value();
                string propertyName = string.Empty;
                DataValidationRule newDataRule = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (validationEntity)
                    {
                        case DataValidationGroupName.Client:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     join client in db.Clients
                                     on validationRule.DataValidationEntityID equals client.pkClientID
                                     where validationRule.enDataValidationProcess == processID &&
                                           validationRule.enDataValidationGroupName == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enDataValidationProcess = validationRule.enDataValidationProcess,
                                         enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         EntityDescription = client.PrimaryCellNumber,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         enDataValidationProperty = validationProperty.enDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enOperatorType = validationRule.enOperatorType,
                                         enOperator = validationRule.enOperator,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();
                            break;
                        case DataValidationGroupName.CompanyClient:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     join company in db.Companies
                                     on validationRule.DataValidationEntityID equals company.pkCompanyID
                                     where validationRule.enDataValidationProcess == processID &&
                                           validationRule.enDataValidationGroupName == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enDataValidationProcess = validationRule.enDataValidationProcess,
                                         enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         EntityDescription = company.CompanyName,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         enDataValidationProperty = validationProperty.enDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enOperatorType = validationRule.enOperatorType,
                                         enOperator = validationRule.enOperator,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();
                            break;
                        case DataValidationGroupName.Device:
                        case DataValidationGroupName.SimCard:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     where validationRule.enDataValidationProcess == processID &&
                                           validationRule.enDataValidationGroupName == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enDataValidationProcess = validationRule.enDataValidationProcess,
                                         enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         EntityDescription = "None",
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         enDataValidationProperty = validationProperty.enDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enOperatorType = validationRule.enOperatorType,
                                         enOperator = validationRule.enOperator,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();
                            break;
                        case DataValidationGroupName.ExternalData:
                            rules = (from validationRule in db.DataValidationRules
                                     join externalData in db.ExternalBillingDatas
                                     on validationRule.DataValidationEntityID equals externalData.pkExternalBillingDataID
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     where validationRule.enDataValidationProcess == processID &&
                                           validationRule.enDataValidationGroupName == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enDataValidationProcess = validationRule.enDataValidationProcess,
                                         enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         EntityDescription = externalData.TableName,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         ExtDataValidationProperty = validationProperty.ExtDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enOperatorType = validationRule.enOperatorType,
                                         enOperator = validationRule.enOperator,
                                         CalculationParameters = validationRule.CalculationParameters,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     });
                            break;
                    }

                    // If no rules found return null
                    if (rules == null)
                        return null;

                    foreach (var rule in rules)
                    {
                        newDataRule = new DataValidationRule();
                        if ((DataValidationGroupName)rule.enDataValidationGroupName == DataValidationGroupName.ExternalData)
                        {
                            if ((RuleOperator)rule.enOperator == RuleOperator.Sum)
                            {
                                string[] properties = rule.CalculationParameters.Split(';');

                                foreach (string property in properties)
                                {
                                    propertyName = ReadExternalPropertyName(Convert.ToInt32(property));
                                    newDataRule.PropertyName += string.Format("{0};", propertyName);
                                    newDataRule.PropertyDescription += string.Format("{0};", propertyName);
                                }
                            }
                            else
                            {
                                propertyName = ReadExternalPropertyName(rule.fkDataValidationPropertyID);
                                newDataRule.PropertyName += string.Format("{0};", propertyName);
                                newDataRule.PropertyDescription += string.Format("{0};", propertyName);
                            }

                            newDataRule.PropertyName = newDataRule.PropertyName.TrimEnd(';');
                            newDataRule.PropertyDescription = newDataRule.PropertyDescription.TrimEnd(';');
                            newDataRule.CalculationParameters = rule.CalculationParameters;
                        }
                        else
                        {
                            newDataRule.PropertyName = ((DataValidationPropertyName)rule.enDataValidationProperty).ToString();
                            newDataRule.PropertyDescription = EnumHelper.GetDescriptionFromEnum((DataValidationPropertyName)rule.enDataValidationProperty);
                        }
                        newDataRule.pkDataValidationRuleID = rule.pkDataValidationRuleID;
                        newDataRule.enDataValidationProcess = rule.enDataValidationProcess;
                        newDataRule.enDataValidationGroupName = rule.enDataValidationGroupName;
                        newDataRule.GroupDescription = EnumHelper.GetDescriptionFromEnum((DataValidationGroupName)rule.enDataValidationGroupName);
                        newDataRule.DataValidationEntityID = rule.DataValidationEntityID;
                        newDataRule.EntityDescription = rule.EntityDescription;
                        newDataRule.fkDataValidationPropertyID = rule.fkDataValidationPropertyID;
                        newDataRule.DataTypeDescription = ((DataTypeName)rule.enDataType).ToString();
                        newDataRule.enOperatorType = rule.enOperatorType;
                        newDataRule.OperatorTypeDescription = EnumHelper.GetDescriptionFromEnum((OperatorType)rule.enOperatorType);
                        newDataRule.enOperator = rule.enOperator;
                        newDataRule.OperatorDescription = EnumHelper.GetOperatorFromOperatorTypeEnum((OperatorType)rule.enOperatorType, rule.enOperator);
                        newDataRule.DataValidationValue = rule.DataValidationValue;
                        newDataRule.ModifiedBy = rule.ModifiedBy;
                        newDataRule.ModifiedDate = rule.ModifiedDate;
                        validationRules.Add(newDataRule);
                    }

                    return new ObservableCollection<DataValidationRule>(validationRules.OrderBy(p => p.EntityDescription).ThenBy(p => p.EntityDescription));
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationRules",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing data validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The data validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDataValidationRule(DataValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    // Check to see if the validation rule name already exist for another entity
                    DataValidationRule existingValidationRule = db.DataValidationRules.Where(p => p.pkDataValidationRuleID != validationRule.pkDataValidationRuleID &&
                                                                                                  p.enDataValidationProcess == validationRule.enDataValidationProcess &&
                                                                                                  p.enDataValidationGroupName == validationRule.enDataValidationGroupName &&
                                                                                                  p.DataValidationEntityID == validationRule.DataValidationEntityID &&
                                                                                                  p.fkDataValidationPropertyID == validationRule.fkDataValidationPropertyID &&
                                                                                                  p.enOperatorType == validationRule.enOperatorType &&
                                                                                                  p.enOperator == validationRule.enOperator).FirstOrDefault();
        
                    if (existingValidationRule != null)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                        "The validation rule already exist.",
                                                                        "CreateDataValidationRule",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }

                    existingValidationRule = db.DataValidationRules.Where(p => p.pkDataValidationRuleID == validationRule.pkDataValidationRuleID).FirstOrDefault();

                    if (existingValidationRule != null)
                    {
                        existingValidationRule.DataValidationEntityID = validationRule.DataValidationEntityID;
                        existingValidationRule.fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID;
                        existingValidationRule.enOperatorType = validationRule.enOperatorType;
                        existingValidationRule.enOperator = validationRule.enOperator;
                        existingValidationRule.CalculationParameters = validationRule.CalculationParameters;
                        existingValidationRule.DataValidationValue = validationRule.DataValidationValue;
                        existingValidationRule.ModifiedBy = validationRule.ModifiedBy;
                        existingValidationRule.ModifiedDate = validationRule.ModifiedDate;
                        db.SaveChanges();
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateDataValidationRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Delete the data validation rule from the database
        /// </summary>
        /// <param name="validationRuleID">The data validation rule entity ID to delete.</param>
        /// <returns>True if successfull</returns>
        public bool DeleteDataValidationRule(int validationRuleID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationRule validationRule = db.DataValidationRules.Where(p => p.pkDataValidationRuleID == validationRuleID).FirstOrDefault();

                    // Check to see if the validation rule name exist
                    if (validationRule != null)
                    {
                        db.DataValidationRules.Remove(validationRule);
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "DeleteDataValidationRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The data validation rule to validate against.</param>
        /// <returns>True if successfull</returns>
        public bool ValidateDataValidationRule(DataValidationRule validationRule)
        {
            try
            {
                switch (((DataValidationGroupName)validationRule.enDataValidationGroupName))
                {
                    case DataValidationGroupName.Client:
                        break;
                    case DataValidationGroupName.CompanyClient:
                        return ValidateCompanyClientData(validationRule);
                    case DataValidationGroupName.SimCard:
                        return ValidateCompanySimCardData(validationRule);
                    case DataValidationGroupName.ExternalData:
                        return ValidateExternalBillingData(validationRule);
                }
                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateDataValidationRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update the relevant enetity based on the validation result info
        /// </summary>
        /// <param name="validationExceptionInfo">The validation exception info to apply.</param>
        /// <returns>True if successfull</returns>
        public bool ApplyDataRule(DataValidationException validationExceptionInfo)
        {
            try
            {
                bool result = false;
                string errorMessage = string.Empty;

                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationRule rule = ((DbQuery<DataValidationRule>)(from validationRule in db.DataValidationRules
                                                                             where validationRule.pkDataValidationRuleID == validationExceptionInfo.fkDataValidationPropertyID
                                                                             select validationRule)).Include("DataValidationProperty").FirstOrDefault();

                    if (rule != null)
                    {
                        switch ((DataValidationGroupName)validationExceptionInfo.enDataValidationGroupName)
                        {
                            case DataValidationGroupName.Client:
                                result = new ClientModel(_eventAggregator).UpdateClient(SearchEntity.ClientID, 
                                                                                        validationExceptionInfo.DataValidationEntityID.ToString(),
                                                                                        ((DataValidationPropertyName)rule.DataValidationProperty.enDataValidationProperty).ToString(),
                                                                                        rule.DataValidationValue,
                                                                                        null,
                                                                                        out errorMessage);

                                //Client clientToUpdate = db.Clients.Where(p => p.pkClientID == validationExceptionInfo.EntityID).FirstOrDefault();

                                //if (clientToUpdate != null)
                                //{
                                //    // Get the client table properties (Fields)
                                //    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Client));

                                //    foreach (PropertyDescriptor property in properties)
                                //    {
                                //        // Find the data column (property) to update
                                //        if (rule.DataValidationOption.ValidationDataName == property.Name)
                                //        {
                                //            // Update the property value based on the data type
                                //            switch ((ValidationDataType)rule.DataValidationOption.enValidationDataType)
                                //            {
                                //                case ValidationDataType.Date:
                                //                    property.SetValue(clientToUpdate, Convert.ToDateTime(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                case ValidationDataType.Integer:
                                //                    property.SetValue(clientToUpdate, Convert.ToInt32(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                case ValidationDataType.Decimal:
                                //                    property.SetValue(clientToUpdate, Convert.ToDecimal(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                case ValidationDataType.Bool:
                                //                    property.SetValue(clientToUpdate, Convert.ToBoolean(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                default:
                                //                    property.SetValue(clientToUpdate, rule.DataValidationOption.ValidationDataName);
                                //                    break;
                                //            }

                                //            db.SaveChanges();
                                //            result = true;
                                //        }
                                //    }
                                //}
                                break;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ApplyDataRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the company data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        private bool ValidateCompanyClientData(DataValidationRule validationRule)
        {
            try
            {
                DataValidationException validationResult = null;
                int entityID = 0;
                string entityName = string.Empty;
                string entityValue = string.Empty;
                string ruleValue = string.Empty;
                bool canApplyRule = false;
                bool result = true;

                using (var db = MobileManagerEntities.GetContext())
                {
                    // Get all the active clients link to the company
                    List<Client> companyClients = db.Clients.Where(p => p.fkCompanyID == validationRule.DataValidationEntityID && p.IsActive).ToList();

                    if (companyClients.Count > 0)
                    {
                        // Get the Client table properties (Fields)
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Client));

                        foreach (PropertyDescriptor property in properties)
                        {
                            // Only validate when the rule's data name
                            // match the property name
                            if (validationRule.PropertyName == property.Name)
                            {
                                // Initialise the progress values
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = 1,
                                    MaxValue = companyClients.Count,
                                });

                                // Validate the data rule values for each           
                                // client linked the the company
                                foreach (Client client in companyClients)
                                {
                                    entityID = client.pkClientID;
                                    entityName = string.Format("{0} ({1})", client.ClientName, client.PrimaryCellNumber);
                                    entityValue = db.Entry(client).Property(validationRule.PropertyName).CurrentValue != null ?
                                                  db.Entry(client).Property(validationRule.PropertyName).CurrentValue.ToString() : string.Empty;

                                    switch ((OperatorType)validationRule.enOperatorType)
                                    {
                                        case OperatorType.StringOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareStringValues((StringOperator)validationRule.enOperator,
                                                                                       entityValue,
                                                                                       validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the rule can be auto applied to fixed failures
                                            canApplyRule = (StringOperator)validationRule.enOperator == StringOperator.Equal ? true : false;
                                            break;
                                        case OperatorType.DateOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((DateOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            break;
                                        case OperatorType.NumericOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((NumericOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareNumericValues((NumericOperator)validationRule.enOperator,
                                                                                        entityValue,
                                                                                        validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enOperator == NumericOperator.Equal ? true : false;
                                            break;
                                        case OperatorType.BooleanOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((BooleanOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareBooleanValues((BooleanOperator)validationRule.enOperator,
                                                                                        entityValue);
                                            break;
                                        default:
                                            break;
                                    }

                                    // Update the progress values for each client
                                    _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                    {
                                        CurrentValue = companyClients.IndexOf(client),
                                        MaxValue = companyClients.Count,
                                    });

                                    // Update the validation result values
                                    validationResult = new DataValidationException();
                                    if (result)
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.InternalDataValidation.Value(),
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enDataValidationGroupName = DataValidationGroupName.Client.Value(),
                                            DataValidationEntityID = client.pkClientID,
                                            Result = true
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                    else
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.InternalDataValidation.Value(),
                                            fkDataValidationPropertyID = validationRule.pkDataValidationRuleID,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enDataValidationGroupName = DataValidationGroupName.Client.Value(),
                                            DataValidationEntityID = client.pkClientID,
                                            CanApplyRule = canApplyRule,
                                            Message = string.Format("{0} failed for {1} linked to company {2} - current value ({3}) - expected ({4}).",
                                                                    validationRule.PropertyDescription.ToUpper(), entityName, validationRule.EntityDescription,
                                                                    entityValue, ruleValue),
                                            Result = false
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                }

                                // Update the progress values for the last client
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = companyClients.Count,
                                    MaxValue = companyClients.Count,
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateCompanyClientData",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the simcard data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        private bool ValidateCompanySimCardData(DataValidationRule validationRule)
        {
            try
            {
                DataValidationException validationResult = null;
                string entityName = string.Empty;
                string entityValue = string.Empty;
                string ruleValue = string.Empty;
                bool canApplyRule = false;
                bool result = true;

                using (var db = MobileManagerEntities.GetContext())
                {
                    // Get all the active simcards
                    List<SimCard> simCards = db.SimCards.Where(p => p.IsActive).ToList();

                    if (simCards.Count > 0)
                    {
                        // Get the Client table properties (Fields)
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(SimCard));

                        foreach (PropertyDescriptor property in properties)
                        {
                            // Only validate when the rule's data name
                            // match the property name
                            if (validationRule.PropertyName == property.Name)
                            {
                                // Initialise the progress values
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = 1,
                                    MaxValue = simCards.Count,
                                });

                                // Validate the data rule values for each simcard         
                                foreach (SimCard sim in simCards)
                                {
                                    entityName = string.Format("{0}", sim.CellNumber);
                                    entityValue = db.Entry(sim).Property(validationRule.PropertyName).CurrentValue != null ?
                                                  db.Entry(sim).Property(validationRule.PropertyName).CurrentValue.ToString() : string.Empty;

                                    switch ((OperatorType)validationRule.enOperatorType)
                                    {
                                        case OperatorType.StringOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareStringValues((StringOperator)validationRule.enOperator,
                                                                                       entityValue,
                                                                                       validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (StringOperator)validationRule.enOperator == StringOperator.Equal ? true : false;
                                            break;
                                        case OperatorType.DateOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((DateOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            break;
                                        case OperatorType.NumericOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((NumericOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareNumericValues((NumericOperator)validationRule.enOperator,
                                                                                        entityValue,
                                                                                        validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enOperator == NumericOperator.Equal ? true : false;
                                            break;
                                        case OperatorType.BooleanOperator:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((BooleanOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareBooleanValues((BooleanOperator)validationRule.enOperator,
                                                                                        entityValue);
                                            break;
                                        default:
                                            break;
                                    }

                                    // Update the progress values for each simcard
                                    _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                    {
                                        CurrentValue = simCards.IndexOf(sim),
                                        MaxValue = simCards.Count,
                                    });

                                    // Update the validation result values
                                    validationResult = new DataValidationException();
                                    if (result)
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                            enDataValidationGroupName = DataValidationGroupName.SimCard.Value(),
                                            DataValidationEntityID = sim.pkSimCardID,
                                            Result = true
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                    else
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkDataValidationPropertyID = validationRule.pkDataValidationRuleID,
                                            enDataValidationGroupName = DataValidationGroupName.Client.Value(),
                                            DataValidationEntityID = sim.pkSimCardID,
                                            CanApplyRule = canApplyRule,
                                            Message = string.Format("{0} failed for {1} - current value ({2}) - expected ({3}).",
                                                                    validationRule.PropertyDescription.ToUpper(), entityName, 
                                                                    entityValue, ruleValue),
                                            Result = false
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                }

                                // Update the progress values for the last client
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = simCards.Count,
                                    MaxValue = simCards.Count,
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateSimCardData",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the external billing data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        private bool ValidateExternalBillingData(DataValidationRule validationRule)
        {
            try
            {
                DataValidationException validationResult = null;
                OperatorType operatorType = (OperatorType)validationRule.enOperatorType;
                string entityName = string.Empty;
                string entityValue = string.Empty;
                string ruleValue = string.Empty;
                int rowIdx = 0;
                bool result = true;

                using (var db = MobileManagerEntities.GetContext())
                {
                    // Get the external billing data detail
                    ExternalBillingData extBillingDetail = db.ExternalBillingDatas.Where(p => p.pkExternalBillingDataID == validationRule.DataValidationEntityID).FirstOrDefault();

                    if (extBillingDetail == null)
                    {
                        return false;
                    }

                    // Get the external data for the specified provider
                    _externalData = new ExternalBillingDataModel(_eventAggregator).ReadExternalBillingData(extBillingDetail.TableName);

                    if (_externalData.Rows.Count == 0)
                    {
                        return false;
                    }

                    // Get the external data table properties (Fields)
                    IEnumerable<DataValidationProperty> properties = new DataValidationPropertyModel(_eventAggregator).ReadExtDataValidationProperties(validationRule.enDataValidationGroupName, validationRule.DataValidationEntityID);

                    foreach (DataValidationProperty property in properties)
                    {
                        // Only validate when the rule's data name match the property name
                        if (validationRule.PropertyDescription == property.ExtDataValidationProperty)
                        {
                            // Initialise the progress values
                            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                            {
                                CurrentValue = 1,
                                MaxValue = _externalData.Rows.Count,
                            });

                            if (operatorType == OperatorType.RuleOperator && (RuleOperator)validationRule.enOperator == RuleOperator.Duplicate)
                            {
                                IEnumerable<string> duplicates = new ExternalBillingDataModel(_eventAggregator).ReadExternalDataDuplicates(extBillingDetail.TableName, property.ExtDataValidationProperty);

                                // Update the progress values for each simcard
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = rowIdx,
                                    MaxValue = _externalData.Rows.Count,
                                });

                                // Update the validation result values
                                validationResult = new DataValidationException();
                                if (duplicates.Count() == 0)
                                {
                                    validationResult = new DataValidationException()
                                    {
                                        fkBillingProcessID = BillingExecutionState.ExternalDataValidation.Value(),
                                        enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                        DataValidationEntityID = validationRule.DataValidationEntityID,
                                        fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                        Result = true
                                    };

                                    _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                }
                                else
                                {
                                    foreach (string dup in duplicates)
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.ExternalDataValidation.Value(),
                                            enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                            DataValidationEntityID = validationRule.DataValidationEntityID,
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                            CanApplyRule = false,
                                            Message = string.Format("Duplicates found for {0} {1}.",
                                                                    property.ExtDataValidationProperty.ToUpper(), dup.ToUpper()),
                                            Result = false
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                }
                            }
                            else
                            {
                                // Validate the data rule values for each simcard         
                                foreach (DataRow row in _externalData.Rows)
                                {
                                    ++rowIdx;
                                    entityName = property.ExtDataValidationProperty.ToUpper();
                                    entityValue = row[property.ExtDataValidationProperty].ToString();

                                    switch (operatorType)
                                    {
                                        case OperatorType.StringOperator:
                                            ruleValue = string.Format("{0} {1} {2}", property.ExtDataValidationProperty.ToUpper(),
                                                                                     ((StringOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareStringValues((StringOperator)validationRule.enOperator,
                                                                                       entityValue,
                                                                                       validationRule.DataValidationValue);
                                            break;
                                        case OperatorType.DateOperator:
                                            ruleValue = string.Format("{0} {1} {2}", property.ExtDataValidationProperty.ToUpper(),
                                                                                     ((DateOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);

                                            result = _dataComparer.CompareDateValues((DateOperator)validationRule.enOperator,
                                                                                     entityValue,
                                                                                     validationRule.DataValidationValue);
                                            break;
                                        case OperatorType.NumericOperator:
                                            ruleValue = string.Format("{0} {1} {2}", property.ExtDataValidationProperty.ToUpper(),
                                                                                     ((NumericOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareNumericValues((NumericOperator)validationRule.enOperator,
                                                                                        entityValue,
                                                                                        validationRule.DataValidationValue);
                                            break;
                                        case OperatorType.BooleanOperator:
                                            ruleValue = string.Format("{0} {1} {2}", property.ExtDataValidationProperty.ToUpper(),
                                                                                     ((BooleanOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareBooleanValues((BooleanOperator)validationRule.enOperator,
                                                                                        entityValue);
                                            break;
                                        case OperatorType.RuleOperator:
                                            if (((RuleOperator)validationRule.enOperator == RuleOperator.Range))
                                            {
                                                ruleValue = string.Format("{0} {1} {2}", property.ExtDataValidationProperty.ToUpper(),
                                                                                         ((RuleOperator)validationRule.enOperator).ToString(),
                                                                                         validationRule.DataValidationValue);

                                                result = _dataComparer.CompareRangeValues(entityValue,
                                                                                          validationRule.DataValidationValue);
                                            }
                                            break;
                                        default:
                                            ruleValue = string.Format("{0} {1} {2}", property.ExtDataValidationProperty.ToUpper(),
                                                                                     ((BooleanOperator)validationRule.enOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = false;
                                            break;
                                    }

                                    // Update the progress values for each simcard
                                    _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                    {
                                        CurrentValue = rowIdx,
                                        MaxValue = _externalData.Rows.Count,
                                    });

                                    // Update the validation result values
                                    validationResult = new DataValidationException();
                                    if (result)
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.ExternalDataValidation.Value(),
                                            enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                            DataValidationEntityID = validationRule.DataValidationEntityID,
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                            Result = true
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                    else
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.ExternalDataValidation.Value(),
                                            enDataValidationGroupName = validationRule.enDataValidationGroupName,
                                            DataValidationEntityID = validationRule.DataValidationEntityID,
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                            CanApplyRule = false,
                                            Message = string.Format("{0} failed for {1} - current value ({2}) - expected ({3}).",
                                                                    property.ExtDataValidationProperty.ToUpper(), validationRule.EntityDescription,
                                                                    entityValue, ruleValue),
                                            Result = false
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                }
                            }

                            // Update the progress values for the last client
                            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                            {
                                CurrentValue = _externalData.Rows.Count,
                                MaxValue = _externalData.Rows.Count,
                            });
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateExternalBillingData",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Get the external data property name
        /// </summary>
        /// <param name="propertyID">The property ID to get.</param>
        /// <returns>Property name</returns>
        private string ReadExternalPropertyName(int propertyID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationProperty property = db.DataValidationProperties.Where(p => p.pkDataValidationPropertyID == propertyID).FirstOrDefault();
                    return property != null ? property.ExtDataValidationProperty : string.Empty;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadPropertyName",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return string.Empty;
            }
        }
    }
}
