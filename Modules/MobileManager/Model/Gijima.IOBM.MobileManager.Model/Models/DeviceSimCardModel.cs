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
    public class DeviceSimCardModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DeviceSimCardModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new device simcard entity in the database
        /// </summary>
        /// <param name="deviceSimCard">The device simcard entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateClientService(DeviceSimCard deviceSimCard)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DeviceSimCards.Any(p => p.fkDeviceID == deviceSimCard.fkDeviceID && p.fkSimCardID == deviceSimCard.fkSimCardID))
                    {
                        db.DeviceSimCards.Add(deviceSimCard);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        MessageBoxResult msgResult = MessageBox.Show("Error: The device sim card already exist!",
                                                                 "Device SimCard Create", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DeviceSimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateClientService",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all device simcards from the database
        /// </summary>
        /// <param name="deviceID">The device id linked to the device view.</param>
        /// <returns>Collection of device simcards</returns>
        public ObservableCollection<DeviceSimCard> ReadDeviceSimCard(int deviceID)
        {
            try
            {
                IEnumerable<DeviceSimCard> devicesSimCard = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    devicesSimCard = ((DbQuery<DeviceSimCard>)(from deviceSimcard in db.DeviceSimCards
                                                               where deviceID == deviceSimcard.fkDeviceID
                                                               select deviceSimcard)).Include("SimCard").ToList();

                    return new ObservableCollection<DeviceSimCard>(devicesSimCard);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DeviceSimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDeviceSimCard",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Delete an existing device simcard entity in the database
        /// </summary>
        /// <param name="deviceSimCard">The device simcard entity to delete.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDeviceSimCards(Device device, string modifiedby, Dictionary<string, object> deviceSims)
        {
            try
            {
                bool result = false;

                using (var db = MobileManagerEntities.GetContext())
                {
                    if (deviceSims != null)
                    {
                        //Remove all previous entries
                        db.DeviceSimCards.RemoveRange(db.DeviceSimCards.Where(x => x.fkDeviceID == device.pkDeviceID));

                        //Create new entry for each selected service
                        foreach (KeyValuePair<string, object> sim in deviceSims)
                        {
                            DeviceSimCard deviceSimCard = new DeviceSimCard();
                            deviceSimCard.fkDeviceID = device.pkDeviceID;
                            deviceSimCard.fkSimCardID = Convert.ToInt32(sim.Value.ToString());
                            deviceSimCard.ModifiedBy = modifiedby;
                            deviceSimCard.ModifiedDate = DateTime.Now;

                            db.DeviceSimCards.Add(deviceSimCard);
                        }
                        db.SaveChanges();
                    }

                    //_activityLogger.CreateDataChangeAudits<Client>(_dataActivityHelper.GetDataChangeActivities<Client>(existingClient, client, client.fkContractID, db));
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DeviceSimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "DeleteClientService",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
