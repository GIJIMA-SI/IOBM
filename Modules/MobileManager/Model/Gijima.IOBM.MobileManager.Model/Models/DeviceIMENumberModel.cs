using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DeviceIMENumberModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DeviceIMENumberModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }
                
        /// <summary>
        /// Read all device IME numbers from the database
        /// </summary>
        /// <param name="deviceID">The device id linked to the device view.</param>
        /// <returns>Collection of device IME numbers</returns>
        public ObservableCollection<DeviceIMENumber> ReadDeviceIMENumber(int deviceID)
        {
            try
            {
                IEnumerable<DeviceIMENumber> deviceIMENumber = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    deviceIMENumber = ((DbQuery<DeviceIMENumber>)(from deviceIMENumbers in db.DeviceIMENumbers
                                                                  where deviceID == deviceIMENumbers.fkDeviceID
                                                                  select deviceIMENumbers)).ToList();

                    return new ObservableCollection<DeviceIMENumber>(deviceIMENumber);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DeviceIMENumberModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDeviceIMENumber",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Delete an existing device IME number entity in the database
        /// </summary>
        /// <param name="deviceIMENumbers">The device IME number entity to delete.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDeviceIMENumber(IEnumerable<DeviceIMENumber> deviceIMENumbers, int DeviceID)
        {
            try
            {
                bool result = false;

                using (var db = MobileManagerEntities.GetContext())
                {
                    if (deviceIMENumbers != null && deviceIMENumbers.Count() > 0)
                    {
                        //Remove all previous entries
                        db.DeviceIMENumbers.RemoveRange(db.DeviceIMENumbers.Where(x => x.fkDeviceID == DeviceID));
                        db.SaveChanges();
                        
                        foreach (DeviceIMENumber deviceIMENumber in deviceIMENumbers)
                        {
                            deviceIMENumber.pkDeviceIMENumberID = 0;
                            db.DeviceIMENumbers.Add(deviceIMENumber);
                        }

                        db.SaveChanges();
                    }
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DeviceIMENumberModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateDeviceIMENumber",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
