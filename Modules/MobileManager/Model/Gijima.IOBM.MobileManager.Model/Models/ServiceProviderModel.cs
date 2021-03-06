﻿using Gijima.IOBM.Infrastructure.Events;
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
    public class ServiceProviderModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ServiceProviderModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new service provider entity in the database
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateServiceProvider(ServiceProvider serviceProvider)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ServiceProviders.Any(p => p.ServiceProviderName.ToUpper() == serviceProvider.ServiceProviderName))
                    {
                        db.ServiceProviders.Add(serviceProvider);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} service provider already exist.", serviceProvider.ServiceProviderName));
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
        /// Read all or active only service providers from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of ServiceProviders</returns>
        public ObservableCollection<ServiceProvider> ReadServiceProviders(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<ServiceProvider> serviceProviders = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    serviceProviders = ((DbQuery<ServiceProvider>)(from sp in db.ServiceProviders
                                                                   where activeOnly ? sp.IsActive : true &&
                                                                         excludeDefault ? sp.pkServiceProviderID > 0 : true
                                                                   select sp)).OrderBy(p => p.ServiceProviderName).ToList();

                    return new ObservableCollection<ServiceProvider>(serviceProviders);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Select all the service provider names
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<string> ReadProviderNames()
        {
            try
            {
                IEnumerable<ServiceProvider> serviceProviders = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    serviceProviders = ((DbQuery<ServiceProvider>)(from providers in db.ServiceProviders
                                                                   select providers)).OrderBy(p => p.ServiceProviderName).ToList();
                }

                //Converto to observabile collection of string
                ObservableCollection<string> providerNames = new ObservableCollection<string>();
                foreach (ServiceProvider provider in serviceProviders)
                {
                    providerNames.Add(provider.ServiceProviderName);
                }
                return providerNames;
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
        /// Update an existing service provider entity in the database
        /// </summary>
        /// <param name="serviceProvider">The service provider entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateServiceProvider(ServiceProvider serviceProvider)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ServiceProvider existingServiceProvider = db.ServiceProviders.Where(p => p.ServiceProviderName == serviceProvider.ServiceProviderName).FirstOrDefault();

                    // Check to see if the service provider name already exist for another entity 
                    if (existingServiceProvider != null && existingServiceProvider.pkServiceProviderID != serviceProvider.pkServiceProviderID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} service provider already exist.", serviceProvider.ServiceProviderName));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingServiceProvider != null)
                            db.Entry(existingServiceProvider).State = System.Data.Entity.EntityState.Detached;

                        db.ServiceProviders.Attach(serviceProvider);
                        db.Entry(serviceProvider).State = System.Data.Entity.EntityState.Modified;
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
