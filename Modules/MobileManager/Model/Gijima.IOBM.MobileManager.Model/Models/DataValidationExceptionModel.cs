using Gijima.IOBM.Infrastructure.Events;
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
        public ObservableCollection<DataValidationException> ReadDataValidationExceptions(string billingPeriod, int validationProcess)
        {
            try
            {
                IEnumerable<DataValidationException> validationRuleExceptions = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    validationRuleExceptions = ((DbQuery<DataValidationException>)(from ruleException in db.DataValidationExceptions
                                                                                   where ruleException.BillingPeriod == billingPeriod &&
                                                                                         ruleException.fkBillingProcessID == validationProcess
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
    }
}
