using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
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
    public class AuditLogModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public AuditLogModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a audit log entry for the specified entity
        /// </summary>
        /// <param name="auditLog">The activity entry to log.</param>
        /// <returns>True if successfull</returns>
        public bool CreateAuditLog(AuditLog auditLog)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    db.AuditLogs.Add(auditLog);
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
        /// Create log entries for all the changes made to a specified entity after 
        /// the changes been saved to the database
        /// </summary>
        /// <typeparam name="T">The entity to log changes for.</typeparam>
        /// <param name="auditLogEntries">The activity entries to log.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataChangeAudits<T>(IEnumerable<DataActivityLog> auditLogEntries)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    AuditLog auditLog = null;

                    foreach (DataActivityLog activity in auditLogEntries)
                    {
                        auditLog = new AuditLog();
                        auditLog.AuditGroup = typeof(T).Name.ToUpper();
                        auditLog.AuditDescription = activity.ActivityDescription;
                        auditLog.EntityID = activity.EntityID;
                        auditLog.ChangedValue = activity.ChangedValue;
                        auditLog.AuditDate = DateTime.Now;
                        auditLog.ModifiedBy = SecurityHelper.LoggedInFullName;
                        auditLog.ModifiedDate = DateTime.Now;
                        db.AuditLogs.Add(auditLog);
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
        /// Read all audit logs for specified filter from the database
        /// </summary>
        /// <param name="activityFilter">The filter the activities are linked to.</param>
        /// <param name="entityID">The contractID (client) linked to the audit entries.</param>
        /// <returns>Collection of AuditLogs</returns>
        public ObservableCollection<AuditLog> ReadAuditLogs(string activityFilter, int entityID)
        {
            try
            {
                string filter = activityFilter.ToUpper();
                IEnumerable<AuditLog> logs = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    logs = ((DbQuery<AuditLog>)(from auditLog in db.AuditLogs
                                                where auditLog.EntityID == entityID
                                                select auditLog)).OrderByDescending(p => p.AuditDate).ToList();

                    if (filter != "NONE")
                        logs = logs.Where(p => p.AuditGroup == filter);

                    return new ObservableCollection<AuditLog>(logs);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing activity log entity in the database
        /// </summary>
        /// <param name="auditLog">The activity log entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateAuditLog(AuditLog auditLog)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    db.AuditLogs.Attach(auditLog);
                    db.Entry(auditLog).State = System.Data.Entity.EntityState.Modified;
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
    }
}
