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
using System.Reflection;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ClientLocationModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ClientLocationModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new client location entity in the database
        /// </summary>
        /// <param name="clientLocation">The client location entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateClientLocation(ClientLocation clientLocation)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ClientLocations.Any(p => p.LocationDescription.ToUpper() == clientLocation.LocationDescription))
                    {
                        db.ClientLocations.Add(clientLocation);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} client location already exist.", clientLocation.LocationDescription));
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
        /// Read all or active only client location from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of ClientLocations</returns>
        public ObservableCollection<ClientLocation> ReadClientLocations(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<ClientLocation> clientLocations = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    clientLocations = ((DbQuery<ClientLocation>)(from location in db.ClientLocations
                                                                 where activeOnly ? location.IsActive : true &&
                                                                       excludeDefault ? location.pkClientLocationID > 0 : true
                                                                 select location)).OrderBy(p => p.LocationDescription).ToList();

                    return new ObservableCollection<ClientLocation>(clientLocations);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only client location descriptions from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of ClientLocations</returns>
        public ObservableCollection<string> ReadClientLocationNames(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<ClientLocation> clientLocations = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    clientLocations = ((DbQuery<ClientLocation>)(from location in db.ClientLocations
                                                                 where activeOnly ? location.IsActive : true &&
                                                                       excludeDefault ? location.pkClientLocationID > 0 : true
                                                                 select location)).OrderBy(p => p.LocationDescription).ToList();
                }

                //Converto to observabile collection of string
                ObservableCollection<string> clientLocationsString = new ObservableCollection<string>();
                foreach (ClientLocation location in clientLocations)
                {
                    clientLocationsString.Add(location.LocationDescription);
                }
                return clientLocationsString;
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
        /// Update an existing client location entity in the database
        /// </summary>
        /// <param name="clientLocation">The client location entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClientLocation(ClientLocation clientLocation)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ClientLocation existingLocation = db.ClientLocations.Where(p => p.LocationDescription == clientLocation.LocationDescription).FirstOrDefault();

                    // Check to see if the location description already exist for another entity 
                    if (existingLocation != null && existingLocation.pkClientLocationID != clientLocation.pkClientLocationID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} client location already already exist.", clientLocation.LocationDescription));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingLocation != null)
                            db.Entry(existingLocation).State = System.Data.Entity.EntityState.Detached;

                        db.ClientLocations.Attach(clientLocation);
                        db.Entry(clientLocation).State = System.Data.Entity.EntityState.Modified;
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
