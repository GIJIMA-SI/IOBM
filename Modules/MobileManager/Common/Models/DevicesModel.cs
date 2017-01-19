using Gijima.IOBM.Common.Infrastructure;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DevicesModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DevicesModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new device entity in the database
        /// </summary>
        /// <param name="device">The device entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDevice(Device device)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Devices.Any(p => p.fkDeviceMakeID == device.fkDeviceMakeID && 
                                             p.fkDeviceModelID == device.fkDeviceModelID &&
                                             p.fkContractID == device.fkContractID))
                    {
                        db.Devices.Add(device);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0}, {1} device already exist.", device.DeviceMake.MakeDescription, device.DeviceModel.ModelDescription));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Read all or active only devices from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Devices</returns>
        public ObservableCollection<Device> ReadDevices(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Device> devices = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    devices = ((DbQuery<Device>)(from device in db.Devices
                                                  where activeOnly ? device.IsActive : true &&
                                                        excludeDefault ? device.pkDeviceID > 0 : true
                                                  select device)).OrderBy(p => p.DeviceMake.MakeDescription).ToList();

                    return new ObservableCollection<Device>(devices);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only devices linked to the specified contract from the database
        /// </summary>
        /// <param name="contractID">The contract primary key linked to the devices.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Devices</returns>
        public ObservableCollection<Device> ReadDevicesForContract(int contractID, bool activeOnly = false)
        {
            try
            {
                IEnumerable<Device> devices = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    devices = ((DbQuery<Device>)(from device in db.Devices
                                                 where device.fkContractID == contractID
                                                 select device)).Include("DeviceMake")
                                                                .Include("DeviceModel")
                                                                .Include("SimmCard")
                                                                .Include("Status1")
                                                                .OrderBy(p => p.DeviceMake.MakeDescription).ToList();

                    if (activeOnly)
                        devices = devices.Where(p => p.IsActive);

                    return new ObservableCollection<Device>(devices);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing device entity in the database
        /// </summary>
        /// <param name="device">The device entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDevice(Device device)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Device existingDevice = db.Devices.Where(p => p.pkDeviceID == device.pkDeviceID).FirstOrDefault();

                    // Check to see if the device description already exist for another entity 
                    if (existingDevice != null && existingDevice.pkDeviceID != device.pkDeviceID && 
                        existingDevice.fkDeviceMakeID == device.fkDeviceMakeID && existingDevice.fkDeviceModelID == device.fkDeviceModelID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0}, {1} device already exist.", device.DeviceMake.MakeDescription, device.DeviceModel.ModelDescription));
                        return false;
                    }
                    else
                    {
                        if (existingDevice != null)
                        {
                            existingDevice.fkSimmCardID = device.fkSimmCardID;
                            existingDevice.fkDeviceMakeID = device.fkDeviceMakeID;
                            existingDevice.fkDeviceModelID = device.fkDeviceModelID;
                            existingDevice.fkStatusID = device.fkStatusID;
                            existingDevice.IMENumber = device.IMENumber;
                            existingDevice.SerialNumber = device.SerialNumber;
                            existingDevice.ReceiveDate = device.ReceiveDate;
                            existingDevice.InsuranceCost = device.InsuranceCost;
                            existingDevice.InsuranceValue = device.InsuranceValue;
                            existingDevice.ModifiedBy = device.ModifiedBy;
                            existingDevice.ModifiedDate = device.ModifiedDate;
                            existingDevice.IsActive = device.IsActive;
                            db.SaveChanges();
                            return true;
                        }

                        return false;
                    }
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
