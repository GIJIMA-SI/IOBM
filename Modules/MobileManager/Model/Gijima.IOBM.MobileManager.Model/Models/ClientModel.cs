using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Helpers;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ClientModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public ClientModel()
        {
        }

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ClientModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
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
                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        Client existingClient = db.Clients.Where(p => p.PrimaryCellNumber == client.PrimaryCellNumber && p.IsActive).FirstOrDefault();

                        if (existingClient != null)
                        {
                            _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                            .Publish(new ApplicationMessage("ClientModel",
                                                                            string.Format("Cell number {0} already\nexist for client {1}.", client.PrimaryCellNumber, existingClient.ClientName),
                                                                            "CreateClient",
                                                                            ApplicationMessage.MessageTypes.ProcessError));
                            return false;
                        }
                        else
                        {
                            // Save the client entity data
                            client.IsActive = true;
                            db.Clients.Add(client);
                            db.SaveChanges();

                            // Commit changes
                            tc.Complete();

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateClient",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                                                                              .Include("Contract.ClientServices")
                                                                              .Include("ClientBilling").FirstOrDefault();

                    return selectedClient;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadClient",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadClients",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read all or active only clients linked to the specified company from the database
        /// </summary>
        /// <param name="companyID">The company ID the client is linked to.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Clients</returns>
        public ObservableCollection<Client> ReadClientsForCompany(int companyID, bool activeOnly)
        {
            try
            {
                IEnumerable<Client> clients = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    clients = ((DbQuery<Client>)(from client in db.Clients
                                                 where client.fkCompanyID == companyID
                                                 select client)).OrderBy(p => p.ClientName).ToList();

                    if (activeOnly)
                        clients = clients.Where(p => p.IsActive);

                    return new ObservableCollection<Client>(clients);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadClientsForCompany",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Count all the employees linked to the specified company ID
        /// </summary>
        /// <param name="companyID">The company ID the employees are linked to.</param>
        /// <returns>Employee Count</returns>
        public int EmployeeCountForCompany(int companyID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.Clients.Count(p => p.fkCompanyID == companyID && p.IsActive);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "EmployeeCountForCompany",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return 0;
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
                bool result = false;

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        Client existingClient = ReadClient(client.pkClientID);

                        // Check to see if the client name already exist for another entity 
                        if (existingClient != null && 
                            existingClient.ClientName != client.ClientName &&
                            existingClient.EmployeeNumber != client.EmployeeNumber &&
                            existingClient.IDNumber != client.IDNumber &&
                            existingClient.Email != client.Email &&
                            existingClient.CostCode != client.CostCode &&
                            existingClient.AddressLine1 != client.AddressLine1)
                        {
                            _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                            .Publish(new ApplicationMessage("ClientModel",
                                                                            "This method of re-allocation is not allowed.",
                                                                            "UpdateClient",
                                                                            ApplicationMessage.MessageTypes.ProcessError));
                            result = false;
                        }
                        else
                        {
                            // If the client is in-active then make all 
                            // devices and simcards for client in-active
                            if (!client.IsActive)
                            {
                                new DevicesModel(_eventAggregator).DeleteDevicesForClient(client.fkContractID, db);
                                new SimCardModel(_eventAggregator).DeleteSimcardsForClient(client.fkContractID, db);
                            }

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

                            db.ClientBillings.Attach(client.ClientBilling);
                            db.Entry(client.ClientBilling).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                            _activityLogger.CreateDataChangeAudits<Client>(_dataActivityHelper.GetDataChangeActivities<Client>(existingClient, client, client.fkContractID, db));
                            _activityLogger.CreateDataChangeAudits<Contract>(_dataActivityHelper.GetDataChangeActivities<Contract>(existingClient.Contract, client.Contract, client.fkContractID, db));
                            _activityLogger.CreateDataChangeAudits<PackageSetup>(_dataActivityHelper.GetDataChangeActivities<PackageSetup>(existingClient.Contract.PackageSetup, client.Contract.PackageSetup, client.fkContractID, db));
                            _activityLogger.CreateDataChangeAudits<ClientBilling>(_dataActivityHelper.GetDataChangeActivities<ClientBilling>(existingClient.ClientBilling, client.ClientBilling, client.fkContractID, db));

                            // Commit changes
                            tc.Complete();

                            result = true;
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateClient",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update an existing client entity in the database
        /// </summary>
        /// <param name="searchEntity">The client data to search on.</param>
        /// <param name="searchCriteria">The client search criteria to search for.</param>
        /// <param name="updateColumn">The client entity property (column) to update.</param>
        /// <param name="updateValue">The value to update client entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClient(SearchEntity searchEntity, string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            Client existingClient = null;
            Client clientToUpdate = null;
            bool result = false;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (searchEntity)
                    {
                        case SearchEntity.ClientID:
                            existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.ClientID).FirstOrDefault();
                            break;
                        case SearchEntity.PrimaryCellNumber:
                            existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.PrimaryCellNumber).FirstOrDefault();
                            break;
                        case SearchEntity.EmployeeNumber:
                            if (companyGroup == null)
                                existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.EmployeeNumber).FirstOrDefault();
                            else
                                existingClient = new SearchEngineModel(_eventAggregator).SearchForClientByCompanyGroup(searchCriteria, SearchEntity.EmployeeNumber, companyGroup).FirstOrDefault();
                            break;
                        case SearchEntity.IDNumber:
                            existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.IDNumber).FirstOrDefault();
                            break;
                    }
                }

                if (existingClient != null)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        using (var db = MobileManagerEntities.GetContext())
                        {
                            clientToUpdate = db.Clients.Where(p => p.pkClientID == existingClient.pkClientID).FirstOrDefault();

                            // Get the client table properties (Fields)
                            PropertyDescriptor[] entityProperties = EDMHelper.GetEntityStructure<Client>();

                            foreach (PropertyDescriptor property in entityProperties)
                            {
                                // Find the data column (property) to update
                                if (property.Name == updateColumn)
                                {
                                    // Convert the db type into the type of the property in our entity
                                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                        updateValue = Convert.ChangeType(updateValue, property.PropertyType.GetGenericArguments()[0]);
                                    else if (property.PropertyType == typeof(System.Guid))
                                        updateValue = new Guid(updateValue.ToString());
                                    else if (property.PropertyType == typeof(System.Byte[]))
                                        updateValue = Convert.FromBase64String(updateValue.ToString());
                                    else
                                        updateValue = Convert.ChangeType(updateValue, property.PropertyType);

                                    // Set the value of the property with the value from the db
                                    property.SetValue(clientToUpdate, updateValue);

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<Client>(_dataActivityHelper.GetDataChangeActivities<Client>(existingClient, clientToUpdate, clientToUpdate.fkContractID, db));

                                    db.SaveChanges();
                                }
                            }
                        }

                        // Commit changes
                        tc.Complete();
                    }
                }
                else
                {
                    errorMessage = string.Format("Client not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                    return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateClient",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update the clients selected contract services in the database
        /// </summary>
        /// <param name="clientServices">The list of contract services for the client.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClientServices(Client client, string modifiedby, Dictionary<string, object> clientServices)
        {
            try
            {
                bool result = false;

                using (var db = MobileManagerEntities.GetContext())
                {
                    if (clientServices != null)
                    {
                        //Remove all previous entries
                        db.ClientServices.RemoveRange(db.ClientServices.Where(x => x.fkContractID == client.fkContractID));
                        
                        //Create new entry for each selected service
                        foreach (KeyValuePair<string, object> service in clientServices)
                        {
                            ClientService clientService = new ClientService();
                            clientService.fkContractID = client.fkContractID;
                            clientService.fkContractServiceID = Convert.ToInt32(service.Value.ToString());
                            clientService.ModifiedBy = modifiedby;
                            clientService.ModifiedDate = DateTime.Now;

                            db.ClientServices.Add(clientService);
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
                                .Publish(new ApplicationMessage("ClientModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateClientServices",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
