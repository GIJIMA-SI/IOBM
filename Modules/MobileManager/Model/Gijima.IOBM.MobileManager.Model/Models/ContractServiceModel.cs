using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;

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
        /// Create a new contract service entity in the database
        /// </summary>
        /// <param name="serviceModel">The contract service entity to add.</param>
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
                        MessageBoxResult msgResult = MessageBox.Show("Error: The contract service already exist!",
                                                                 "Contract Service Save", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ContractServiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateContractService",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read contract services from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Contract Services</returns>
        public ObservableCollection<ContractService> ReadContractService(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<ContractService> contractService = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    contractService = ((DbQuery<ContractService>)(from contractServices in db.ContractServices
                                                         where activeOnly ? contractServices.IsActive : true &&
                                                               excludeDefault ? contractServices.pkContractServiceID > 0 : true
                                                         select contractService)).OrderBy(p => p.ServiceDescription).ToList();

                    return new ObservableCollection<ContractService>(contractService);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ContractServiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadContractService",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;

            }
        }

        /// <summary>
        /// Update an existing cntract service entity in the database
        /// </summary>
        /// <param name="contractService">The contract service entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateContractService(ContractService contractService)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ContractService existingContractService = db.ContractServices.Where(p => p.ServiceDescription == contractService.ServiceDescription).FirstOrDefault();

                    // Check to see if the contract service description already exist for another entity 
                    if (existingContractService != null && existingContractService.pkContractServiceID != contractService.pkContractServiceID)
                    {
                        MessageBoxResult msgResult = MessageBox.Show("Error: The contract service already exist!",
                                                                                         "Contract Service Update", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingContractService != null)
                            db.Entry(existingContractService).State = System.Data.Entity.EntityState.Detached;

                        db.ContractServices.Attach(contractService);
                        db.Entry(contractService).State = System.Data.Entity.EntityState.Modified;
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
