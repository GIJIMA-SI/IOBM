﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class StatusModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public StatusModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new status entity in the database
        /// </summary>
        /// <param name="status">The status entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateStatus(Status status)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Status.Any(p => p.StatusDescription.ToUpper() == status.StatusDescription))
                    {
                        db.Status.Add(status);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} status already exist.", status.StatusDescription));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Read all or active only statuses from the database
        /// </summary>
        /// <param name="enLinkedTo">The entities the status is linked to.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Statuses</returns>
        public ObservableCollection<Status> ReadStatuses(StatusLink enLinkedTo, bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                short allID = StatusLink.All.Value();
                short contractID = StatusLink.Contract.Value();
                short deviceID = StatusLink.Device.Value();
                short simID = StatusLink.Sim.Value();
                short contractDeviceID = StatusLink.ContractDevice.Value();
                short contractSimID = StatusLink.ContractSim.Value();
                short deviceSimID = StatusLink.DeviceSim.Value();
                IEnumerable<Status> statuses = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    statuses = ((DbQuery<Status>)(from status in db.Status
                                                  where activeOnly ? status.IsActive : true &&
                                                        excludeDefault ? status.pkStatusID > 0 : true
                                                  select status)).OrderBy(p => p.StatusDescription).ToList();

                    if (enLinkedTo == StatusLink.Contract)
                        statuses = statuses.Where(p => p.enStatusLink == allID || p.enStatusLink == contractID || 
                                                       p.enStatusLink == contractDeviceID || p.enStatusLink == contractSimID);

                    if (enLinkedTo == StatusLink.Device)
                        statuses = statuses.Where(p => p.enStatusLink == allID || p.enStatusLink == deviceID || 
                                                       p.enStatusLink == contractDeviceID || p.enStatusLink == deviceSimID);

                    if (enLinkedTo == StatusLink.Sim)
                        statuses = statuses.Where(p => p.enStatusLink == allID || p.enStatusLink == simID || 
                                                       p.enStatusLink == contractSimID || p.enStatusLink == deviceSimID);

                    return new ObservableCollection<Status>(statuses);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all status names from the database
        /// </summary>
        /// <param name="enLinkedTo">The entities the status is linked to.</param>
        /// <returns>Collection of Statuses</returns>
        public ObservableCollection<string> ReadStatuseOptions(StatusLink enLinkedTo)
        {
            try
            {
                short allID = StatusLink.All.Value();
                short contractID = StatusLink.Contract.Value();
                short deviceID = StatusLink.Device.Value();
                short simID = StatusLink.Sim.Value();
                short contractDeviceID = StatusLink.ContractDevice.Value();
                short contractSimID = StatusLink.ContractSim.Value();
                short deviceSimID = StatusLink.DeviceSim.Value();
                IEnumerable<Status> statuses = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    statuses = ((DbQuery<Status>)(from status in db.Status
                                                  select status)).OrderBy(p => p.StatusDescription).ToList();

                    if (enLinkedTo == StatusLink.Contract)
                        statuses = statuses.Where(p => p.enStatusLink == allID || p.enStatusLink == contractID ||
                                                       p.enStatusLink == contractDeviceID || p.enStatusLink == contractSimID);

                    if (enLinkedTo == StatusLink.Device)
                        statuses = statuses.Where(p => p.enStatusLink == allID || p.enStatusLink == deviceID ||
                                                       p.enStatusLink == contractDeviceID || p.enStatusLink == deviceSimID);

                    if (enLinkedTo == StatusLink.Sim)
                        statuses = statuses.Where(p => p.enStatusLink == allID || p.enStatusLink == simID ||
                                                       p.enStatusLink == contractSimID || p.enStatusLink == deviceSimID);
                }

                //Converto to observabile collection of string
                ObservableCollection<string> statusOptions = new ObservableCollection<string>();
                foreach (Status status in statuses)
                {
                    statusOptions.Add(status.StatusDescription);
                }
                return statusOptions;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing status entity in the database
        /// </summary>
        /// <param name="status">The status entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateStatus(Status status)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Status existingStatus = db.Status.Where(p => p.StatusDescription == status.StatusDescription).FirstOrDefault();

                    // Check to see if the status description already exist for another entity 
                    if (existingStatus != null && existingStatus.pkStatusID != status.pkStatusID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} status already exist.", status.StatusDescription));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingStatus != null)
                            db.Entry(existingStatus).State = System.Data.Entity.EntityState.Detached;

                        db.Status.Attach(status);
                        db.Entry(status).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
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
