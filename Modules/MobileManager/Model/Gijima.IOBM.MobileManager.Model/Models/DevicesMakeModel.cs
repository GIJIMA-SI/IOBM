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
    public class DevicesMakeModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DevicesMakeModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new device make entity in the database
        /// </summary>
        /// <param name="deviceMake">The device make entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDeviceMake(DeviceMake deviceMake)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DeviceMakes.Any(p => p.MakeDescription.ToUpper() == deviceMake.MakeDescription))
                    {
                        db.DeviceMakes.Add(deviceMake);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} devices make already exist.", deviceMake.MakeDescription));
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
        /// Read all or active only device makes from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Device makes</returns>
        public ObservableCollection<DeviceMake> ReadDeviceMakes(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<DeviceMake> deviceMakes = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    deviceMakes = ((DbQuery<DeviceMake>)(from devicesMake in db.DeviceMakes
                                                         where activeOnly ? devicesMake.IsActive : true &&
                                                               excludeDefault ? devicesMake.pkDeviceMakeID > 0 : true
                                                         select devicesMake)).OrderBy(p => p.MakeDescription).ToList();

                    return new ObservableCollection<DeviceMake>(deviceMakes);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing device make entity in the database
        /// </summary>
        /// <param name="deviceMake">The device make entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDeviceMake(DeviceMake deviceMake)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DeviceMake existingDeviceMake = db.DeviceMakes.Where(p => p.MakeDescription == deviceMake.MakeDescription).FirstOrDefault();

                    // Check to see if the device make description already exist for another entity 
                    if (existingDeviceMake != null && existingDeviceMake.pkDeviceMakeID != deviceMake.pkDeviceMakeID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} device make already exist.", deviceMake.MakeDescription));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingDeviceMake != null)
                            db.Entry(existingDeviceMake).State = System.Data.Entity.EntityState.Detached;

                        db.DeviceMakes.Attach(deviceMake);
                        db.Entry(deviceMake).State = System.Data.Entity.EntityState.Modified;
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
