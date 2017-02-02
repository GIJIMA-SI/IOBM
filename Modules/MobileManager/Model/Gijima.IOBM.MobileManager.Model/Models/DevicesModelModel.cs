using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DevicesModelModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DevicesModelModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new device model entity in the database
        /// </summary>
        /// <param name="deviceModel">The device model entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDeviceModel(DeviceModel deviceModel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DeviceModels.Any(p => p.ModelDescription.ToUpper() == deviceModel.ModelDescription))
                    {
                        db.DeviceModels.Add(deviceModel);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} devices model already exist.", deviceModel.ModelDescription));
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
        /// Read all or active only device models from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Device models</returns>
        public ObservableCollection<DeviceModel> ReadDeviceModels(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<DeviceModel> deviceModels = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    deviceModels = ((DbQuery<DeviceModel>)(from devicesModel in db.DeviceModels
                                                           where activeOnly ? devicesModel.IsActive : true &&
                                                                 excludeDefault ? devicesModel.pkDeviceModelID > 0 : true
                                                           select devicesModel)).Include("DeviceMake").OrderBy(p => p.DeviceMake.MakeDescription)
                                                                                                      .ThenBy(p => p.ModelDescription).ToList();

                    return new ObservableCollection<DeviceModel>(deviceModels);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only device models for the specified device make from the database
        /// </summary>
        /// <param name="deviceMakeID">The specified device make primary key.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Device models</returns>
        public ObservableCollection<DeviceModel> ReadDeviceMakeModels(int deviceMakeID, bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<DeviceModel> deviceModels = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    deviceModels = ((DbQuery<DeviceModel>)(from devicesModel in db.DeviceModels
                                                           where devicesModel.fkDeviceMakeID == deviceMakeID ||
                                                                 devicesModel.pkDeviceModelID == 0
                                                           select devicesModel)).OrderBy(p => p.ModelDescription).ToList();

                    if (activeOnly)
                        deviceModels = deviceModels.Where(p => p.IsActive);

                    if (excludeDefault)
                        deviceModels = deviceModels.Where(p => p.pkDeviceModelID > 0);

                    return new ObservableCollection<DeviceModel>(deviceModels);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing device model entity in the database
        /// </summary>
        /// <param name="deviceModel">The device model entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDeviceModel(DeviceModel deviceModel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DeviceModel existingDeviceModel = db.DeviceModels.Where(p => p.ModelDescription == deviceModel.ModelDescription).FirstOrDefault();

                    // Check to see if the device model description already exist for another entity 
                    if (existingDeviceModel != null && existingDeviceModel.pkDeviceModelID != deviceModel.pkDeviceModelID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} device model already exist.", deviceModel.ModelDescription));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingDeviceModel != null)
                            db.Entry(existingDeviceModel).State = System.Data.Entity.EntityState.Detached;

                        db.DeviceModels.Attach(deviceModel);
                        db.Entry(deviceModel).State = System.Data.Entity.EntityState.Modified;
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
