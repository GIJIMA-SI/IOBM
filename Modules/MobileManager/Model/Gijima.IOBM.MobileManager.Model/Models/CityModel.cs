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

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class CityModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public CityModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new city entity in the database
        /// </summary>
        /// <param name="city">The city entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateCity(City city)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Cities.Any(p => p.CityName.ToUpper() == city.CityName))
                    {
                        db.Cities.Add(city);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("ReportModel",
                                                 string.Format("The {0} city already exist.", city.CityName),
                                                 "CreateReport",
                                                 ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CityModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateCity",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all or active only cities from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Cityes</returns>
        public ObservableCollection<City> ReadCityes(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<City> cities = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    cities = ((DbQuery<City>)(from city in db.Cities
                                              where activeOnly ? city.IsActive : true &&
                                                    excludeDefault ? city.pkCityID > 0 : true
                                              select city)).Include("Province").OrderBy(p => p.CityName).ToList();

                    return new ObservableCollection<City>(cities);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CityModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCityes",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing city entity in the database
        /// </summary>
        /// <param name="city">The city entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateCity(City city)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    City existingCity = db.Cities.Where(p => p.CityName == city.CityName).FirstOrDefault();

                    if (existingCity != null)
                    {
                        // Check to see if the city description already exist for another entity 
                        if (existingCity.pkCityID != city.pkCityID)
                        {
                            _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                            .Publish(new ApplicationMessage("ReportModel",
                                                     string.Format("The {0} city already exist.", city.CityName),
                                                     "UpdateCity",
                                                     ApplicationMessage.MessageTypes.Information));
                            return false;
                        }

                        existingCity.CityName = city.CityName;
                        existingCity.fkProvinceID = city.fkProvinceID;
                        existingCity.ModifiedBy = city.ModifiedBy;
                        existingCity.ModifiedDate = city.ModifiedDate;
                        existingCity.IsActive = city.IsActive;
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CityModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateCity",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
