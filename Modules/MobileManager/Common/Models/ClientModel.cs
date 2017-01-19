using Gijima.IOBM.Common.Infrastructure;
using Gijima.IOBM.Infrastructure.Logging;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ClientModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private ActivityLogger _activityLogger = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ClientModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new ActivityLogger(_eventAggregator);
        }

        /// <summary>
        /// Create a new client entity in the database
        /// </summary>
        /// <param name="client">The client entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateClient(Client client)
        {
            try
            {
                //using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                //{
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        if (!db.Clients.Any(p => p.ClientName.ToUpper() == client.ClientName && p.PrimaryCellNumber == client.PrimaryCellNumber))
                        {
                        // Save the client entity data
                            client.IsActive = true;
                            db.Clients.Add(client);
                            db.SaveChanges();
                            return true;
                        }
                        else
                        {
                            _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} client already exist.", client.ClientName));
                            return false;
                        }
                    }
                //}
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Read client data from the database
        /// </summary>
        /// <param name="clientID">The client ID to read data for.</param>
        /// <returns>Client Entity</returns>
        public Client ReadClient(int clientID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Client selectedClient = ((DbQuery<Client>)(from client in db.Clients
                                                               where client.pkClientID == clientID
                                                               select client)).Include("Contract")
                                                                              .Include("Contract.PackageSetup")
                                                                              .Include("ClientBilling").FirstOrDefault();

                    return selectedClient;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only clients from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Clients</returns>
        public ObservableCollection<Client> ReadClients(bool activeOnly)
        {
            try
            {
                IEnumerable<Client> clients = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    clients = ((DbQuery<Client>)(from client in db.Clients
                                                 where activeOnly ? client.IsActive : true
                                                 select client)).OrderBy(p => p.ClientName).ToList();

                    return new ObservableCollection<Client>(clients);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing client entity in the database
        /// </summary>
        /// <param name="client">The client entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClient(Client client)
        {
            try
            {
                //using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                //{
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        Client existingClient = db.Clients.Where(p => p.ClientName == client.ClientName && p.PrimaryCellNumber == client.PrimaryCellNumber).FirstOrDefault();

                        // Check to see if the client name already exist for another entity 
                        if (existingClient != null && existingClient.pkClientID != client.pkClientID)
                        {
                            _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} client already exist.", client.ClientName));
                            return false;
                        }
                        else
                        {
                            // Prevent primary key confilcts when using attach property
                            if (existingClient != null)
                                db.Entry(existingClient).State = System.Data.Entity.EntityState.Detached;

                            db.Clients.Attach(client);
                            db.Entry(client).State = System.Data.Entity.EntityState.Modified;

                            db.Contracts.Attach(client.Contract);
                            db.Entry(client.Contract).State = System.Data.Entity.EntityState.Modified;

                            db.PackageSetups.Attach(client.Contract.PackageSetup);
                            db.Entry(client.Contract.PackageSetup).State = System.Data.Entity.EntityState.Modified;

                            db.ClientBillings.Attach(client.ClientBilling);
                            db.Entry(client.ClientBilling).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                        // Commit changes
                        //tc.Complete();
                        //_activityLogger.CreateDataChangeActivities<Client>(_activityLogger.ReadDataChangeActivities<Client>(existingClient, client, db), db);

                            return true;
                        }
                    }
                //}
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
