using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ActivityLogModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ActivityLogModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create log entries for all the changes made to a specified entity after 
        /// the changes been saved to the database
        /// </summary>
        /// <typeparam name="T">The entity to log changes for.</typeparam>
        /// <param name="activityLogEntries">The activity entries to log.</param>
        /// <param name="userName">The user name performing the action.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataChangeActivities<T>(IEnumerable<string> activityLogEntries, string userName)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ActivityLog activityLog = null;

                    foreach (string activity in activityLogEntries)
                    {
                        activityLog = new ActivityLog();
                        activityLog.ActivityGroup = typeof(T).Name.ToUpper();
                        activityLog.ActivityDescription = activity;
                        activityLog.ActivityDate = DateTime.Now;
                        activityLog.ModifiedBy = userName;
                        activityLog.ModifiedDate = DateTime.Now;
                        db.ActivityLogs.Add(activityLog);
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Read all activity logs for specified filter from the database
        /// </summary>
        /// <param name="activityFilter">The filter the activities are linked to.</param>
        /// <returns>Collection of ActivityLogs</returns>
        public ObservableCollection<ActivityLog> ReadActivityLogs(string activityFilter)
        {
            try
            {
                string filter = activityFilter.ToUpper();
                IEnumerable<ActivityLog> logs = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    logs = ((DbQuery<ActivityLog>)(from activityLog in db.ActivityLogs
                                                   where filter != "NONE" ? activityLog.ActivityGroup == filter : true
                                                   select activityLog)).OrderByDescending(p => p.ActivityDate).ToList();

                    return new ObservableCollection<ActivityLog>(logs);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing activity log entity in the database
        /// </summary>
        /// <param name="activityLog">The activity log entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateActivityLog(ActivityLog activityLog)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    db.ActivityLogs.Attach(activityLog);
                    db.Entry(activityLog).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
