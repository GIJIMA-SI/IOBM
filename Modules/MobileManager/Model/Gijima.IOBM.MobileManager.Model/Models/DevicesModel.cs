﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Helpers;
using Gijima.IOBM.Security;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Transactions;

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
            //Search with a flag to know if device already exist
            bool deviceExist = false;
            //devices with the IME number
            IEnumerable<Device> devices = null;
            // Get the available status ID to be used in re-allaction valdation
            int availableSatusID;
            // Get the available re-allocated ID to be used to update device
            int reAllocatedID;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    availableSatusID = db.Status.Where(p => p.StatusDescription == "AVAILABLE").First().pkStatusID;
                    reAllocatedID = db.Status.Where(p => p.StatusDescription == "REALLOCATED").First().pkStatusID;

                    //Check if device is still issued to antoher client
                    foreach (DeviceIMENumber deviceIMENumber in device.DeviceIMENumbers)
                    {
                        DeviceIMENumber existingNumber = db.DeviceIMENumbers.Where(p => p.IMENumber == deviceIMENumber.IMENumber).FirstOrDefault();
                        if (existingNumber != null && db.Devices.Any(p => p.pkDeviceID == existingNumber.fkDeviceID &&
                                        p.fkStatusID != availableSatusID &&
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
                    //Get the device to be re allocated
                    foreach (DeviceIMENumber deviceIMENumber in device.DeviceIMENumbers)
                    {
                        devices = ((DbQuery<Device>)(from listDevice in db.Devices
                                                     join imeNumbers in db.DeviceIMENumbers
                                                     on listDevice.pkDeviceID equals imeNumbers.fkDeviceID
                                                     where imeNumbers.IMENumber.Equals(deviceIMENumber.IMENumber) &&
                                                           listDevice.fkStatusID.Equals(availableSatusID)
                                                     select listDevice)).ToList();
                        if (devices != null && devices.Count() > 0)
                            break;
                    }
                }

                using (var db = MobileManagerEntities.GetContext())
                {
                    // If a device gets re-allocated ensure that all the required properties 
                    // is valid to allow re-alloaction
                    if (devices != null && devices.Count() > 0)
                    {
                        int pkDeviceID = devices.FirstOrDefault().pkDeviceID;
                        Device availableDevice = db.Devices.Where(p => p.pkDeviceID == pkDeviceID).FirstOrDefault();
                        availableDevice.fkStatusID = db.Status.Where(p => p.pkStatusID == reAllocatedID).FirstOrDefault().pkStatusID;

                        // Log the data values that changed
                        _activityLogger.CreateDataChangeAudits<Device>(_dataActivityHelper.GetDataChangeActivities<Device>(devices.FirstOrDefault(), availableDevice, availableDevice.fkContractID, db));

                        db.Devices.Add(device);
                        db.SaveChanges();

                        return true;
                    }
                    
                    //Check if the device IMENumber already exist
                    foreach (DeviceIMENumber deviceIMENumber in device.DeviceIMENumbers)
                    {
                        if (db.DeviceIMENumbers.Any(p => p.IMENumber == deviceIMENumber.IMENumber) &&
                            db.Devices.Where(p => p.pkDeviceID == db.DeviceIMENumbers.Where(x => x.IMENumber == deviceIMENumber.IMENumber).FirstOrDefault().fkDeviceID).FirstOrDefault().IsActive == true)
                        {
                            deviceExist = true;
                        }
                    }

                    if (!deviceExist)
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
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Create a device in the database (Import)
        /// </summary>
        /// <param name="device"></param>
        /// <param name="db"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool CreateDevice(Device device, MobileManagerEntities db, ref string errorMessage)
        {
            try
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
                            errorMessage = "The device is still allocated to another client.";
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
                    //Set all client previous devices inactive for the contract
                    //Requested by Charl
                    ChangeDevicesStatusForClient(device.fkContractID, Statuses.INACTIVE.Value(), db);
                    db.Devices.Add(device);
                    db.SaveChanges();

                    return true;
                }
                else
                {
                    errorMessage = "Device already exists";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Creates a new device entity in the database from a spreadsheet row
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool CreateDeviceImport(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Device deviceToImport = null;
            Contract contract = null;
            bool mustImport = false;
            bool dataChanged = false;
            bool state = true;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    //Check if number is linked to a contract
                    contract = db.Contracts.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                    if (contract == null || contract.IsActive == false)
                    {
                        errorMessage = string.Format("Cell number {0} not linked to a contract/found, ", searchCriteria);
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        //Create a new empty device to add properties
                        deviceToImport = new Device();
                        deviceToImport.pkDeviceID = 0;
                        deviceToImport.fkContractID = contract.pkContractID;
                        deviceToImport.DeviceIMENumbers = new List<DeviceIMENumber>();

                        //Create the IME numbers for the device
                        foreach (string mapping in mappedProperties)
                        {
                            if (mapping.Contains("IME Number"))
                            {
                                string[] arrMapping = mapping.Split('=');
                                string sheetHeader = arrMapping[0].Trim();

                                DeviceIMENumber deviceIMENumber = new DeviceIMENumber();
                                deviceIMENumber.IMENumber = importValues[sheetHeader].ToString();
                                deviceIMENumber.fkDeviceID = 0;
                                deviceIMENumber.ModifiedBy = SecurityHelper.LoggedInFullName;
                                deviceIMENumber.ModifiedDate = DateTime.Now;
                                deviceToImport.DeviceIMENumbers.Add(deviceIMENumber);
                            }
                        }

                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<Device>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            mustImport = false;

                            // Get the source property and source value 
                            // mapped the device entity property
                            foreach (string mappedProperty in mappedProperties)
                            {
                                string[] arrMappedProperty = mappedProperty.Split('=');
                                string propertyName = new DataImportPropertyModel(_eventAggregator).GetPropertyName(arrMappedProperty[1].Trim(), enSelectedEntity);
                                if (propertyName == property.Name)
                                {
                                    importProperties = mappedProperty.Split('=');
                                    sourceProperty = importProperties[0].Trim();
                                    sourceValue = importValues[sourceProperty];
                                    dataChanged = mustImport = true;
                                    break;
                                }
                            }

                            // Always update these values
                            if (dataChanged && (property.Name == "ModifiedBy" || property.Name == "ModifiedDate" || property.Name == "IsActive"))
                                mustImport = true;

                            if (mustImport)
                            {
                                //If an error occur it will tell in what column what value
                                errorMessage = $"Error column '{sourceProperty.ToString()}' with " +
                                    $"value '{sourceValue.ToString()}' ";

                                // Validate the source status and get
                                // the value for the fkStatusID
                                if (property.Name == "fkStatusID")
                                {
                                    Status status = db.Status.Where(p => p.StatusDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (status == null)
                                    {
                                        errorMessage = "Status not found for nr." + searchCriteria;
                                        return false;
                                    }

                                    // Set the device to in-active 
                                    // if the status is not issued
                                    if (status.StatusDescription != "ISSUED")
                                        state = false;

                                    sourceValue = status.pkStatusID;
                                }
                                // Validate the source device make and get
                                // the value for the fkDeviceMakeID
                                if (property.Name == "fkDeviceMakeID")
                                {
                                    DeviceMake deviceMake = db.DeviceMakes.Where(p => p.MakeDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (deviceMake == null)
                                    {
                                        errorMessage = "Device make not found for nr." + searchCriteria;
                                        return false;
                                    }

                                    sourceValue = deviceMake.pkDeviceMakeID;
                                }
                                // Validate the source device model and get
                                // the value fot the fkDeviceModelID
                                if (property.Name == "fkDeviceModelID")
                                {
                                    DeviceModel deviceModel = db.DeviceModels.Where(p => p.ModelDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (deviceModel == null)
                                    {
                                        errorMessage = "Device model not found for nr." + searchCriteria;
                                        return false;
                                    }

                                    sourceValue = deviceModel.pkDeviceModelID;
                                }

                                // Set the default values
                                if (property.Name == "ModifiedBy")
                                    sourceValue = SecurityHelper.LoggedInFullName;
                                if (property.Name == "ModifiedDate")
                                    sourceValue = DateTime.Now;
                                if (property.Name == "IsActive")
                                    sourceValue = state;

                                // Convert the db type into the type of the property in our entity
                                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    try
                                    { sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]); }
                                    catch
                                    { sourceValue = null; }
                                }
                                else if (property.PropertyType == typeof(System.Guid))
                                    sourceValue = new Guid(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Byte[]))
                                    sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.DateTime))
                                {
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
                                }
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Int32))
                                    sourceValue = Convert.ToInt32(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Decimal))
                                    sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Double))
                                    sourceValue = Convert.ToDouble(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(deviceToImport, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            if (!CreateDevice(deviceToImport, db, ref errorMessage))
                                return false;
                            // Add the data activity log
                            //result = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimCard, simCardToImport, simCardToImport.fkContractID.Value, db));

                            db.SaveChanges();
                            tc.Complete();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

       /// <summary>
       /// Update devices in system update
       /// </summary>
       /// <param name="searchCriteria"></param>
       /// <param name="mappedProperties"></param>
       /// <param name="importValues"></param>
       /// <param name="enSelectedEntity"></param>
       /// <param name="errorMessage"></param>
       /// <returns></returns>
        public bool UpdateDeviceUpdate(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Device existingDevice = null;
            Device deviceToUpdate = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool result = false;
            int deviceID;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    deviceID = new DeviceIMENumberModel(_eventAggregator).ReadDeviceIDByIMENumber(searchCriteria, db, ref errorMessage);
                    existingDevice = db.Devices.Where(p => p.pkDeviceID == deviceID).FirstOrDefault();

                    if (existingDevice == null)
                    {
                        errorMessage = string.Format("Device not found on IME number {0}.", searchCriteria);
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {

                    using (var db = MobileManagerEntities.GetContext())
                    {
                        deviceToUpdate = db.Devices.Where(p => p.pkDeviceID == deviceID).FirstOrDefault();

                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<Device>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            mustUpdate = false;

                            // Get the source property and source value 
                            // mapped the device entity property
                            foreach (string mappedProperty in mappedProperties)
                            {
                                string[] arrMappedProperty = mappedProperty.Split('=');
                                string propertyName = new DataUpdatePropertyModel(_eventAggregator).GetPropertyName(arrMappedProperty[1].Trim(), enSelectedEntity);
                                if (propertyName == property.Name)
                                {
                                    importProperties = mappedProperty.Split('=');
                                    sourceProperty = importProperties[0].Trim();
                                    sourceValue = importValues[sourceProperty];
                                    dataChanged = mustUpdate = true;
                                    break;
                                }
                            }

                            // Always update these values
                            if (dataChanged && (property.Name == "ModifiedBy" || property.Name == "ModifiedDate" || property.Name == "IsActive"))
                                mustUpdate = true;

                            if (mustUpdate)
                            {
                                //If an error occur it will tell in what column what value
                                errorMessage = $"Error column '{sourceProperty.ToString()}' with value '{sourceValue.ToString()}' ";

                                // Validate the source status and get
                                // the value for the fkStatusID
                                if (property.Name == "fkStatusID")
                                {
                                    Status status = db.Status.Where(p => p.StatusDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (status == null)
                                    {
                                        errorMessage = $"Status {sourceValue.ToString()} not found.";
                                        return false;
                                    }

                                    sourceValue = status.pkStatusID;
                                }
                                
                                // Set the default values
                                if (property.Name == "ModifiedBy")
                                    sourceValue = SecurityHelper.LoggedInFullName;
                                if (property.Name == "ModifiedDate")
                                    sourceValue = DateTime.Now;
                                if (property.Name == "IsActive")
                                    sourceValue = true;

                                // Convert the db type into the type of the property in our entity
                                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]);
                                else if (property.PropertyType == typeof(System.Guid))
                                    sourceValue = new Guid(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Byte[]))
                                    sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.DateTime))
                                {
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
                                }
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Int32))
                                    sourceValue = Convert.ToInt32(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Decimal))
                                    sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(deviceToUpdate, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            errorMessage = "Error with data logging if persist contact developer ";
                            // Add the data activity log
                            object item = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingDevice, deviceToUpdate, deviceToUpdate.fkContractID, db));

                            if (item != null)
                            {
                                db.SaveChanges();
                                tc.Complete();
                                result = true;
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
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
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
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
        public void ChangeDevicesStatusForClient(int contractID, int statusID, MobileManagerEntities db)
        {
            try
            {
                IEnumerable<Device> devices = db.Devices.Where(p => p.fkContractID == contractID);

                if (statusID == Statuses.AVAILABLE.Value())
                {
                    foreach (Device device in devices)
                    {
                        if (device.IsActive == true)
                            device.fkStatusID = Statuses.AVAILABLE.Value();
                        device.IsActive = false;
                    }
                }
                else
                {
                    foreach (Device device in devices)
                    {
                        device.fkStatusID = Statuses.INACTIVE.Value();
                        device.IsActive = false;
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
            }
        }
    }
}
