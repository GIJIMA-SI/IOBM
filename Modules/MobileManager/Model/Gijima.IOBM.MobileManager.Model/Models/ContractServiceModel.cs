﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ContractServiceModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ContractServiceModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new device make entity in the database
        /// </summary>
        /// <param name="serviceModel">The device make entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateContractService(ContractService serviceModel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ContractServices.Any(p => p.ServiceDescription.ToUpper() == serviceModel.ServiceDescription))
                    {
                        db.ContractServices.Add(serviceModel);
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
        public ObservableCollection<ContractService> ReadContractService(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<ContractService> deviceMakes = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    deviceMakes = ((DbQuery<ContractService>)(from contractService in db.ContractServices
                                                         where activeOnly ? contractService.IsActive : true &&
                                                               excludeDefault ? contractService.pkContractServiceID > 0 : true
                                                         select contractService)).OrderBy(p => p.ServiceDescription).ToList();

                    return new ObservableCollection<ContractService>(deviceMakes);
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
        public bool UpdateContractService(ContractService deviceMake)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ContractService existingContractService = db.ContractServices.Where(p => p.ServiceDescription == deviceMake.ServiceDescription).FirstOrDefault();

                    // Check to see if the device make description already exist for another entity 
                    if (existingContractService != null && existingContractService.pkContractServiceID != deviceMake.pkContractServiceID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} device make already exist.", deviceMake.MakeDescription));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingContractService != null)
                            db.Entry(existingContractService).State = System.Data.Entity.EntityState.Detached;

                        db.ContractServices.Attach(deviceMake);
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