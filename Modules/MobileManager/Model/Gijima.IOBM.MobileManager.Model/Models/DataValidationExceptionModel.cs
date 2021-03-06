﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DataValidationExceptionModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataValidationExceptionModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Save all the data validation exceptions to the database
        /// </summary>
        /// <param name="validationRuleExceptions">List of data validation exceptions to save.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataValidationExceptions(IEnumerable<DataValidationException> validationRuleExceptions)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    string billingPeriod = MobileManagerEnvironment.BillingPeriod;

                    IEnumerable<DataValidationException> exceptionsToDelete = db.DataValidationExceptions.Where(p => p.BillingPeriod == billingPeriod).ToList();

                    // First delete all the current exceptions 
                    // for this billing period
                    // There should never be exceptions for more than
                    // one billing process for the same billing period 
                    if (exceptionsToDelete.Count() > 0)
                    {
                        foreach (DataValidationException exception in exceptionsToDelete)
                        {
                            db.DataValidationExceptions.Remove(exception);
                        }
                        db.SaveChanges();
                    }

                    // Add the new exceptions for the billing period
                    foreach (DataValidationException exception in validationRuleExceptions)
                    {
                        exception.BillingPeriod = billingPeriod;
                        db.DataValidationExceptions.Add(exception);
                    }
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Read all the validation rule exceptions for the specified billing period from the database
        /// </summary>
        /// <param name="billingPeriod">The billing period to read exceptions for.</param>
        /// <returns>Collection of DataValidationExceptions</returns>
        public ObservableCollection<DataValidationException> ReadDataValidationExceptions(string billingPeriod, int validationProcess, int entityID)
        {
            try
            {
                IEnumerable<DataValidationException> validationRuleExceptions = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    validationRuleExceptions = ((DbQuery<DataValidationException>)(from ruleException in db.DataValidationExceptions
                                                                                   where ruleException.BillingPeriod == billingPeriod &&
                                                                                         ruleException.fkBillingProcessID == validationProcess &&
                                                                                         ruleException.DataValidationEntityID == entityID
                                                                                   select ruleException)).ToList();

                    return new ObservableCollection<DataValidationException>(validationRuleExceptions);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Delete all existing data validation exceptions linked to the propertyID in the database
        /// </summary>
        /// <param name="propertyID">The property ID to delete exceptions for.</param>
        /// <returns>True if successfull</returns>
        public bool DeleteDataValidationExceptions(int propertyID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    IEnumerable<DataValidationException> exceptions = db.DataValidationExceptions.Where(p => p.fkDataValidationPropertyID == propertyID).ToList();

                    if (exceptions != null && exceptions.Count() > 0)
                    {
                        foreach (DataValidationException exception in exceptions)
                        {
                            db.DataValidationExceptions.Remove(exception);
                        }
                        
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationExceptionModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "DeleteDataValidationExceptions",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Delete all the selected data validation axception in the database
        /// </summary>
        /// <param name="validationRuleExceptions">List of data validation exceptions to delete.</param>
        /// <returns>True if successfull</returns>
        public bool DeleteDataValidationExceptions(IEnumerable<DataValidationException> validationRuleExceptions)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationException exceptionToDelete = null;

                    foreach (DataValidationException exception in validationRuleExceptions)
                    {
                        exceptionToDelete = db.DataValidationExceptions.Where(p => p.pkDataValidationExceptionID == exception.pkDataValidationExceptionID).FirstOrDefault();

                        if (exceptionToDelete != null)
                            db.DataValidationExceptions.Remove(exceptionToDelete);
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationExceptionModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "DeleteDataValidationExceptions",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
