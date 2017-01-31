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
    class ClientServiceModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ClientServiceModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new client service entity in the database
        /// </summary>
        /// <param name="clientService">The client service entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateClientService(ClientService clientService)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ClientServices.Any(p => p.fkContractID == clientService.fkContractID && p.fkContractServiceID == clientService.fkContractServiceID))
                    {
                        db.ClientServices.Add(clientService);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return UpdateClientService(clientService);
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
        /// Read all client services from the database
        /// </summary>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Device makes</returns>
        public ObservableCollection<ClientService> ReadClientService(int contractID, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<ClientService> clientService = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    clientService = ((DbQuery<ClientService>)(from clientServices in db.ClientServices
                                                              where contractID == clientServices.fkContractID
                                                              where excludeDefault ? clientServices.pkClientServiceID > 0 : true
                                                              select clientServices)).ToList();

                    return new ObservableCollection<ClientService>(clientService);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing client service entity in the database
        /// </summary>
        /// <param name="clientService">The client service entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClientService(ClientService clientService)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ClientService existingClientService = db.ClientServices.Where(p => p.fkContractID == clientService.fkContractID && p.fkContractServiceID == clientService.fkContractServiceID).FirstOrDefault();


                    // Prevent primary key confilcts when using attach property
                    if (existingClientService != null)
                        db.Entry(existingClientService).State = System.Data.Entity.EntityState.Detached;

                    db.ClientServices.Attach(existingClientService);
                    db.Entry(clientService).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return true;
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
