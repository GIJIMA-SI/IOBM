using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DevicesModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DevicesModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
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
                    // Get the re-allacted status ID to be used in re-allaction valdation
                    int reAllocatedStatusID = db.Status.Where(p => p.StatusDescription == "REALLOCATED").First().pkStatusID;

                    // If a device gets re-allocated ensure that all the required properties 
                    // is valid to allow re-alloaction
                    if (db.Devices.Any(p => p.IMENumber.ToUpper().Trim() == device.IMENumber.ToUpper().Trim() &&
                                            p.fkStatusID != reAllocatedStatusID &&
                                            p.IsActive == true))
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DevicesModel",
                                                                        "The device is still allocated to another client.",
                                                                        "CreateDevice",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }

                    if (!db.Devices.Any(p => p.fkDeviceMakeID == device.fkDeviceMakeID &&
                                             p.fkDeviceModelID == device.fkDeviceModelID &&
                                             p.fkContractID == device.fkContractID &&
                                             p.fkStatusID != reAllocatedStatusID))
                    {
                        db.Devices.Add(device);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DevicesModel",
                                                                        "The device already exist.",
                                                                        "CreateDevice",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DevicesModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateDevice",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DevicesModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDevices",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                                                                .Include("SimCard")
                                                                .Include("Status")
                                                                .OrderByDescending(p => p.IsActive)
                                                                .ThenBy(p => p.Status.StatusDescription).ToList();

                    if (activeOnly)
                        devices = devices.Where(p => p.IsActive);

                    return new ObservableCollection<Device>(devices);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DevicesModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDevicesForContract",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DevicesModel",
                                                                        string.Format("The {0}, {1} device already exist.",
                                                                        device.DeviceMake.MakeDescription, device.DeviceModel.ModelDescription),
                                                                        "UpdateDevice",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                    else if (existingDevice != null)
                    {
                        // Log the data values that changed
                        _activityLogger.CreateDataChangeAudits<Device>(_dataActivityHelper.GetDataChangeActivities<Device>(existingDevice, device, device.fkContractID, db));

                        // Save the new values
                        existingDevice.fkDeviceMakeID = device.fkDeviceMakeID;
                        existingDevice.fkDeviceModelID = device.fkDeviceModelID;
                        existingDevice.fkSimCardID = device.fkSimCardID;
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
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DevicesModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateDevice",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Sets the linked devices inactive
        /// </summary>
        /// <param name="contractID">The contract linked to the devices</param>
        /// <param name="context">The context inside the transaction</param>
        public void DeleteDevicesForClient(int contractID, MobileManagerEntities context)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    IEnumerable<Device> devices = context.Devices.Where(p => p.fkContractID == contractID);

                    foreach (Device device in devices)
                    {
                        device.fkStatusID = Statuses.INACTIVE.Value();
                        device.IsActive = false;
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DevicesModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "DeleteDevicesForClient",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }
    }
}
