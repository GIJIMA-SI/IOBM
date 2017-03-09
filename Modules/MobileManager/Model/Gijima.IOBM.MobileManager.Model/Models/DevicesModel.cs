﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Windows.Documents;

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
                    foreach (DeviceIMENumber imeNumber in device.DeviceIMENumbers)
                    {
                        DeviceIMENumber existingNumber = db.DeviceIMENumbers.Where(p => p.IMENumber == imeNumber.IMENumber).FirstOrDefault();
                        if (existingNumber != null)
                        {
                            if (db.Devices.Any(p => p.pkDeviceID == existingNumber.fkDeviceID &&
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
                        }
                    }

                    if (!db.Devices.Any(p => p.fkDeviceMakeID == device.fkDeviceMakeID &&
                                             p.fkDeviceModelID == device.fkDeviceModelID &&
                                             p.fkContractID == device.fkContractID &&
                                             p.fkStatusID != reAllocatedStatusID && 
                                             p.IsActive == false))
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
        /// Creates a new device entity in the database from a spreadsheet row
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="modifiedBy"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool CreateDeviceImport(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, string modifiedBy, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                //Enusure that all changes are made before commit
                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        //Check if number is linked to a contract
                        Contract contract = db.Contracts.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                        if (contract == null || contract.IsActive == false)
                        {
                            errorMessage = string.Format("Cell number {0} not found.", searchCriteria);
                            return false;
                        }

                        //Set all previous devices for contract in-active
                        //Requested by Charl
                        ObservableCollection<Device> devices = ReadDevicesForContract(contract.pkContractID, true);
                        foreach (Device tmpDevice in devices)
                        {
                            tmpDevice.IsActive = false;
                            UpdateDevice(tmpDevice);
                        }

                        //Create a new empty device to add properties
                        Device device = new Device();
                        device.fkContractID = contract.pkContractID;
                        device.DeviceIMENumbers = new List<DeviceIMENumber>();
                        //Loop through the row and add each property to the device
                        foreach (string property in mappedProperties)
                        {
                            string[] mapping = property.Split('=');
                            string sheetHeader = mapping[0].Trim();
                            string dbHeader = mapping[1].Trim();
                            
                            switch ((DataImportColumn)Enum.Parse(typeof(DataImportColumn), dbHeader))
                            {
                                case DataImportColumn.fkDeviceMakeID:
                                    string deviceMakeRow = importValues[sheetHeader].ToString().ToUpper();
                                    DeviceMake deviceMake = db.DeviceMakes.Where(p => p.MakeDescription == deviceMakeRow).FirstOrDefault();
                                    if (deviceMake == null)
                                    {
                                        errorMessage = string.Format("Device make not found.");
                                        return false;
                                    }
                                    device.fkDeviceMakeID = deviceMake.pkDeviceMakeID;
                                    break;
                                case DataImportColumn.fkDeviceModelID:
                                    string deviceModelRow = importValues[sheetHeader].ToString().ToUpper();
                                    DeviceModel deviceModel = db.DeviceModels.Where(p => p.ModelDescription == deviceModelRow).FirstOrDefault();
                                    if (deviceModel == null)
                                    {
                                        errorMessage = string.Format("Device model not found.");
                                        return false;
                                    }
                                    device.fkDeviceModelID = deviceModel.pkDeviceModelID;
                                    break;
                                case DataImportColumn.fkStatusID:
                                    string statusRow = importValues[sheetHeader].ToString().ToUpper();
                                    Status status = db.Status.Where(p => p.StatusDescription == statusRow).FirstOrDefault();
                                    if (status == null)
                                    {
                                        errorMessage = string.Format("Status description not found.");
                                        return false;
                                    }
                                    device.fkStatusID = status.pkStatusID;
                                    break;
                                case DataImportColumn.SerialNumber:
                                    device.SerialNumber = importValues[sheetHeader].ToString();
                                    break;
                                case DataImportColumn.ReceiveDate:
                                    //Converts the date from an excel format
                                    device.ReceiveDate = DateTime.FromOADate(Convert.ToDouble(importValues[sheetHeader].ToString()));
                                    break;
                                case DataImportColumn.InsuranceCost:
                                    device.InsuranceCost = importValues[sheetHeader].ToString() != null && importValues[sheetHeader].ToString() != "" ? Convert.ToDecimal(importValues[sheetHeader].ToString()) : 0;
                                    break;
                                case DataImportColumn.InsuranceValue:
                                    device.InsuranceValue = importValues[sheetHeader].ToString() != null && importValues[sheetHeader].ToString() != "" ? Convert.ToDecimal(importValues[sheetHeader].ToString()) : 0;
                                    break;
                                case DataImportColumn.IMENumber:
                                    //Create a new deviceIMENumber
                                    DeviceIMENumber deviceIMENumber = new DeviceIMENumber();
                                    deviceIMENumber.IMENumber = importValues[sheetHeader].ToString();
                                    deviceIMENumber.fkDeviceID = 0;
                                    deviceIMENumber.ModifiedBy = modifiedBy;
                                    deviceIMENumber.ModifiedDate = DateTime.Now;
                                    device.DeviceIMENumbers.Add(deviceIMENumber);
                                    break;
                            }
                        }
                        //Add last missing values to the device
                        device.ModifiedBy = modifiedBy;
                        device.ModifiedDate = DateTime.Now;
                        device.IsActive = true;
                        //Create the device in the db
                        CreateDevice(device);
                        db.SaveChanges();
                    }

                    //Commit all changes to the database server
                    tc.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage(this.GetType().Name,
                                         string.Format("Error! {0}, {1}.",
                                         ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                         MethodBase.GetCurrentMethod().Name,
                                         ApplicationMessage.MessageTypes.SystemError));
                return false;
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
                    Device existingDevice = ((DbQuery<Device>)(from myDevice in db.Devices
                                                               where myDevice.pkDeviceID == device.pkDeviceID
                                                               select myDevice)).Include("DeviceIMENumbers")
                                                                .FirstOrDefault();

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
                        _activityLogger.CreateDataChangeAudits<DeviceIMENumber>(_dataActivityHelper.GetDataChangeActivities<DeviceIMENumber>(existingDevice.DeviceIMENumbers, device.DeviceIMENumbers, device.fkContractID, db));

                        // Save the new values
                        existingDevice.fkDeviceMakeID = device.fkDeviceMakeID;
                        existingDevice.fkDeviceModelID = device.fkDeviceModelID;
                        existingDevice.fkStatusID = device.fkStatusID;
                        existingDevice.SerialNumber = device.SerialNumber;
                        existingDevice.ReceiveDate = device.ReceiveDate;
                        existingDevice.InsuranceCost = device.InsuranceCost;
                        existingDevice.InsuranceValue = device.InsuranceValue;
                        existingDevice.ModifiedBy = device.ModifiedBy;
                        existingDevice.ModifiedDate = device.ModifiedDate;
                        existingDevice.IsActive = device.IsActive;
                        db.SaveChanges();

                        // Save device IMENumbers
                        if (device.DeviceIMENumbers != null && device.DeviceIMENumbers.Count > 0)
                            new DeviceIMENumberModel(_eventAggregator).UpdateDeviceIMENumber(device.DeviceIMENumbers, device.pkDeviceID);
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
        public void DeleteDevicesForClient(int contractID, MobileManagerEntities db)
        {
            try
            {
                IEnumerable<Device> devices = db.Devices.Where(p => p.fkContractID == contractID);

                foreach (Device device in devices)
                {
                    device.fkStatusID = Statuses.INACTIVE.Value();
                    device.IsActive = false;
                }

                db.SaveChanges();
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
