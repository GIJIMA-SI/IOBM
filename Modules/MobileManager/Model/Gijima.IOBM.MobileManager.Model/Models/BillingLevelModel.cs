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
    public class BillingLevelModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public BillingLevelModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new billing level entity in the database
        /// </summary>
        /// <param name="billingLevel">The billing level entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateBillingLevel(BillingLevel billingLevel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.BillingLevels.Any(p => p.LevelDescription.ToUpper() == billingLevel.LevelDescription))
                    {
                        db.BillingLevels.Add(billingLevel);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("BillingLevelModel",
                                                                        string.Format("The {0} billing level already exist.", billingLevel.LevelDescription),
                                                                        "CreateBillingLevel",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("BillingLevelModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateBillingLevel",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all or active only billing level from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of BillingLevels</returns>
        public ObservableCollection<BillingLevel> ReadBillingLevels(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<BillingLevel> billingLevels = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    billingLevels = ((DbQuery<BillingLevel>)(from billingLevel in db.BillingLevels
                                                             where activeOnly ? billingLevel.IsActive : true &&
                                                                   excludeDefault ? billingLevel.pkBillingLevelID > 0 : true
                                                             select billingLevel)).OrderBy(p => p.LevelDescription).ToList();

                    return new ObservableCollection<BillingLevel>(billingLevels);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("BillingLevelModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadBillingLevels",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing billing level entity in the database
        /// </summary>
        /// <param name="billingLevel">The billing level entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateBillingLevel(BillingLevel billingLevel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    BillingLevel existingLocation = db.BillingLevels.Where(p => p.LevelDescription == billingLevel.LevelDescription).FirstOrDefault();

                    // Check to see if the location description already exist for another entity 
                    if (existingLocation != null && existingLocation.pkBillingLevelID != billingLevel.pkBillingLevelID)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("BillingLevelModel",
                                                                        string.Format("The {0} billing level already exist.", billingLevel.LevelDescription),
                                                                        "UpdateBillingLevel",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingLocation != null)
                            db.Entry(existingLocation).State = System.Data.Entity.EntityState.Detached;

                        db.BillingLevels.Attach(billingLevel);
                        db.Entry(billingLevel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("BillingLevelModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateBillingLevel",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
