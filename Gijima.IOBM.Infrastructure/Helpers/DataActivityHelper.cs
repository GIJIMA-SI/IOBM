using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.EntityClient;
using Gijima.IOBM.Infrastructure.Events;
using System.Data.Entity.Infrastructure;
using Gijima.IOBM.Infrastructure.Structs;

namespace Gijima.IOBM.Infrastructure.Helpers
{
    public class DataActivityHelper
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataActivityHelper(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Find all the changes made to a specified entity after the changes been saved to the database
        /// </summary>
        /// <typeparam name="T">The entity to interrigate.</typeparam>
        /// <param name="originalData">The entity prior to changes,</param>
        /// <param name="currentData">The entity post changes.</param>
        /// <param name="entityID">The entity (client) linked to the data activity.</param>
        /// <param name="context">The database context.</param>
        /// <returns>A list of DataActivityLog</returns>
        public IEnumerable<DataActivityLog> GetDataChangeActivities<T>(object originalData, object currentData, int entityID, DbContext context)
        {
            try
            { 
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                List<DataActivityLog> changedProperties = new List<DataActivityLog>();
                string modifiedBy = context.Entry(currentData).Property("ModifiedBy").CurrentValue.ToString();

                foreach (PropertyDescriptor property in properties)
                {
                    var originalValue = property.GetValue(originalData);
                    var currentValue = property.GetValue(currentData);
                    DataActivityLog activity = new DataActivityLog();

                    if (originalValue == null)
                        originalValue = string.Empty;

                    if (property.Name != "ModifiedBy" && property.Name != "ModifiedDate")
                        if (currentValue != null && !originalValue.Equals(currentValue))
                        {
                            activity.EntityID = entityID;
                            activity.ChangedValue = originalValue != null ? originalValue.ToString().ToUpper() : string.Empty;
                            activity.ActivityDescription = string.Format("{0} changed from {1} to {2} by {3} on {4}.", property.Name.ToUpper(),
                                                                                                                       activity.ChangedValue,
                                                                                                                       currentValue.ToString().ToUpper(),
                                                                                                                       modifiedBy.ToUpper(), DateTime.Now.ToString());
                            // Prevent changes to Enities to be logged
                            // Only data changes must be logged
                            if (!activity.ActivityDescription.Contains("GIJIMA.IOBM"))
                                changedProperties.Add(activity);
                        }
                }

                return changedProperties;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
