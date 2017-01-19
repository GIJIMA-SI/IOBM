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
    public class ProvinceModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ProvinceModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new province entity in the database
        /// </summary>
        /// <param name="province">The province entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateProvince(Province province)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Provinces.Any(p => p.ProvinceName.ToUpper() == province.ProvinceName))
                    {
                        db.Provinces.Add(province);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} province already exist.", province.ProvinceName));
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
        /// Read all or active only provinces from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Provincees</returns>
        public ObservableCollection<Province> ReadProvincees(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Province> provinces = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    provinces = ((DbQuery<Province>)(from province in db.Provinces
                                                     where activeOnly ? province.IsActive : true &&
                                                           excludeDefault ? province.pkProvinceID > 0 : true
                                                     select province)).OrderBy(p => p.ProvinceName).ToList();

                    return new ObservableCollection<Province>(provinces);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing province entity in the database
        /// </summary>
        /// <param name="province">The province entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateProvince(Province province)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Province existingProvince = db.Provinces.Where(p => p.ProvinceName == province.ProvinceName).FirstOrDefault();

                    // Check to see if the province description already exist for another entity 
                    if (existingProvince != null && existingProvince.pkProvinceID != province.pkProvinceID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} province already exist.", province.ProvinceName));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingProvince != null)
                            db.Entry(existingProvince).State = System.Data.Entity.EntityState.Detached;

                        db.Provinces.Attach(province);
                        db.Entry(province).State = System.Data.Entity.EntityState.Modified;
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
