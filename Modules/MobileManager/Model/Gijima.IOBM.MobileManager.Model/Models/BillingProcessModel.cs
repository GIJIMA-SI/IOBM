using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.Security;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class BillingProcessModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public BillingProcessModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new billing history entity for the specified process in the database
        /// </summary>
        /// <param name="billingProcess">The billing process to create history for.</param>
        /// <returns>True if successfull</returns>
        public bool CreateBillingProcessHistory(BillingExecutionState billingProcess)
        {
            try
            {
                string billingPeriod = MobileManagerEnvironment.BillingPeriod;
                int processID = billingProcess.Value();

                using (var db = MobileManagerEntities.GetContext())
                {
                    BillingProcessHistory currentHistory = db.BillingProcessHistories.Where(p => p.BillingPeriod == billingPeriod &&    
                                                                                                 p.ProcessCurrent == true).FirstOrDefault();

                    // If a current process history entity
                    // found make it not current
                    if (currentHistory != null && currentHistory.fkBillingProcessID < processID &&
                        currentHistory.ProcessEndDate != null && currentHistory.ProcessResult != null)
                    {
                        currentHistory.ProcessCurrent = false;
                    }

                    // Add the new process history entity only if if its the next process
                    if (currentHistory == null || currentHistory.fkBillingProcessID < processID)
                    {
                        BillingProcessHistory processHistory = new BillingProcessHistory();
                        processHistory.fkBillingProcessID = billingProcess.Value();
                        processHistory.BillingPeriod = billingPeriod;
                        processHistory.ProcessStartDate = DateTime.Now;
                        processHistory.ProcessCurrent = true;
                        processHistory.ModifiedBy = SecurityHelper.LoggedInDomainName;
                        processHistory.DateModified = DateTime.Now;

                        db.BillingProcessHistories.Add(processHistory);
                        db.SaveChanges();
                    }

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
        /// Read billing processes from the database
        /// </summary>
        /// <returns>Collection of BillingProcesses</returns>
        public ObservableCollection<BillingProcess> ReadBillingProcesses()
        {
            try
            {
                IEnumerable<BillingProcess> billingProcesses = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    billingProcesses = ((DbQuery<BillingProcess>)(from billingProcess in db.BillingProcesses
                                                                  select billingProcess));

                    return new ObservableCollection<BillingProcess>(billingProcesses);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read billing process history for all or for the specified billing period from the database
        /// </summary>
        /// <param name="billingPeriod">The billing period, default to current.</param>
        /// <returns>Collection of BillingProcessHistory</returns>
        public ObservableCollection<BillingProcessHistory> ReadBillingProcessHistory(string billingPeriod = null)
        {
            try
            {
                billingPeriod = billingPeriod != null ? billingPeriod : MobileManagerEnvironment.BillingPeriod;
                IEnumerable<BillingProcessHistory> billingProcessHistory = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    billingProcessHistory = ((DbQuery<BillingProcessHistory>)(from processHistory in db.BillingProcessHistories
                                                                              where processHistory.BillingPeriod == billingPeriod
                                                                              select processHistory)).Include("BillingProcess")
                                                                                                     .OrderBy(p => p.ProcessStartDate);

                    return new ObservableCollection<BillingProcessHistory>(billingProcessHistory);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read the current billing process history from the database
        /// </summary>
        /// <returns>BillingProcessHistory</returns>
        public BillingProcessHistory ReadBillingProcessCurrentHistory()
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.BillingProcessHistories.Where(p => p.ProcessCurrent == true).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Set a billing history entity for the specified process as completed in the database
        /// </summary>
        /// <param name="billingProcess">The billing process to complete.</param>
        /// <param name="processResult">The process result.</param>
        /// <returns>True if successfull</returns>
        public bool CompleteBillingProcessHistory(BillingExecutionState billingProcess, bool processResult)
        {
            try
            {
                string billingPeriod = MobileManagerEnvironment.BillingPeriod;
                int processID = billingProcess.Value();

                using (var db = MobileManagerEntities.GetContext())
                {
                    BillingProcessHistory processHistory = db.BillingProcessHistories.Where(p => p.BillingPeriod == billingPeriod &&
                                                                                                 p.fkBillingProcessID == processID).FirstOrDefault();

                    // If a process history entity was found and
                    // it is current then set it as complete
                    if (processHistory != null && processHistory.ProcessCurrent)
                    {
                        processHistory.ProcessEndDate = DateTime.Now;
                        TimeSpan duration = processHistory.ProcessEndDate.Value - processHistory.ProcessStartDate;
                        processHistory.ProcessDuration = Math.Round(duration.TotalMinutes, 2);
                        processHistory.ProcessResult = processResult;
                        db.SaveChanges();
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }
    }
}
