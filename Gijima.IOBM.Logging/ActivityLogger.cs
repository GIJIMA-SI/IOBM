using Gijima.IOBM.Common.Infrastructure;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Data.Entity;

namespace Gijima.IOBM.Infrastructure.Logging
{
    public class ActivityLogger
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ActivityLogger(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create log entries for all the changes made to a specified entity after 
        /// the changes been saved to the database
        /// </summary>
        /// <typeparam name="T">The entity to log changes for.</typeparam>
        /// <param name="activityLogEntries">The activity entries to log.</param>
        /// <param name="context">The database context.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataChangeActivities<T>(IEnumerable<string> activityLogEntries)
        {
            try
            {
                //using (context)
                //{
                //    context...Entry < "ActivityLogger" > ActivityLog log = null;

                //    foreach (string activity in activityLogEntries)
                //    {

                //    }
                //    client.IsActive = true;
                //    db.Clients.Add(client);
                //    db.SaveChanges();
                    return true;
                //}
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Find all the changes made to a specified entity after the changes been saved to the database
        /// </summary>
        /// <typeparam name="T">The entity to interrigate.</typeparam>
        /// <param name="originalData">The entity prior to changes,</param>
        /// <param name="currentData">The entity post changes.</param>
        /// <param name="context">The database context.</param>
        /// <returns>A list of activity strings</returns>
        public IEnumerable<string> ReadDataChangeActivities<T>(object originalData, object currentData, DbContext context)
        {
            try
            { 
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                List<string> changedProperties = new List<string>();
                string modifiedBy = context.Entry(currentData).Property("ModifiedBy").CurrentValue.ToString();

                foreach (PropertyDescriptor property in properties)
                {
                    var originalValue = property.GetValue(originalData);
                    var currentValue = property.GetValue(currentData);

                    if (property.Name != "ModifiedBy" && property.Name != "ModifiedDate")
                        if (originalValue != null && currentValue != null && !originalValue.Equals(currentValue))
                            changedProperties.Add(string.Format("{0} changed from {1} to {2} by {3} on {4}.", property.Name.ToUpper(), 
                                                                                                              originalValue.ToString().ToUpper(), 
                                                                                                              currentValue.ToString().ToUpper(), 
                                                                                                              modifiedBy.ToUpper(), DateTime.Now.ToString()));
                }

                return changedProperties;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
}

        /// <summary>
        /// Read a list of all the entity database model entities 
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>List of Entities</returns>
        public IEnumerable<EntityType> ReadDataBaseEntites(DbContext context)
        {
            using (context)
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                var storageMetadata = ((EntityConnection)objectContext.Connection).GetMetadataWorkspace().GetItems(DataSpace.SSpace);
                return (from s in storageMetadata where s.BuiltInTypeKind == BuiltInTypeKind.EntityType select s as EntityType);
            }
        }
    }
}
