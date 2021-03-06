﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Helpers;
using Gijima.IOBM.Security;
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
                        // Get the available status ID to be used in re-allaction valdation
                        int availableSatusID = db.Status.Where(p => p.StatusDescription == "AVAILABLE").First().pkStatusID;


                        Client existingClient = ((DbQuery<Client>)(from exClient in db.Clients
                                                                   where exClient.PrimaryCellNumber == client.PrimaryCellNumber &&
                                                                         exClient.IsActive == true
                                                                   select exClient)).Include("Contract").FirstOrDefault();

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
                            Client existingClientForReAllocation = ((DbQuery<Client>)(from exClient in db.Clients
                                                                                      join exContract in db.Contracts
                                                                                      on exClient.fkContractID equals exContract.pkContractID
                                                                                      where exClient.PrimaryCellNumber == client.PrimaryCellNumber &&
                                                                                            exContract.fkStatusID == availableSatusID
                                                                                      select exClient)).FirstOrDefault();

                            if (existingClientForReAllocation != null) // Re-allocation check
                            {
                                //Check the reallocation
                                if (!new ContractModel(_eventAggregator).CheckContractForReAllocation(client.PrimaryCellNumber))
                                {
                                    return false;
                                }
                            }

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
                                                                              .Include("ClientDepartmentManagers")
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
                                new DevicesModel(_eventAggregator).ChangeDevicesStatusForClient(client.fkContractID, client.Contract.fkStatusID, db);
                                new SimCardModel(_eventAggregator).ChangeSimcardsStatusForClient(client.fkContractID, client.Contract.fkStatusID, db);
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

                            // Get the available status ID to be used in re-allaction valdation
                            int availableSatusID = db.Status.Where(p => p.StatusDescription == "AVAILABLE").First().pkStatusID;

                            // Check if the client is available for reAllocation
                            Client existingClientForReAllocation = ((DbQuery<Client>)(from exClient in db.Clients
                                                                                      join exContract in db.Contracts
                                                                                      on exClient.fkContractID equals exContract.pkContractID
                                                                                      where exClient.PrimaryCellNumber == client.PrimaryCellNumber &&
                                                                                            exContract.fkStatusID == availableSatusID
                                                                                      select exClient)).FirstOrDefault();

                            if (existingClientForReAllocation != null) // Re-allocation check
                            {
                                //Check the reallocation
                                if (!new ContractModel(_eventAggregator).CheckContractForReAllocation(client.PrimaryCellNumber))
                                {
                                    return false;
                                }
                            }

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

        /// <summary>
        /// Update the client details in data update
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="enSelectedEntity"></param>
        /// <param name="selectedDestinationSearch"></param>
        /// <param name="useCompanies"></param>
        /// <param name="selectedCompany"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool UpdateClientUpdate(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, string selectedDestinationSearch, bool groupCompanies, string selectedCompany, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Client existingClient = null;
            Client clientToUpdate = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool result = false;
            bool state = true;
            Company company;
            CompanyGroup companyGroup;
            //A list of clients since emplyee number can 
            //be linked to more than one contract
            List<Client> clients = new List<Client>();

            try
            {

                using (var db = MobileManagerEntities.GetContext())
                {
                    if (selectedDestinationSearch.Replace(" ", "") == "PrimaryCellNumber")
                    {
                        existingClient = db.Clients.Where(p => p.PrimaryCellNumber == searchCriteria).FirstOrDefault();
                        if (existingClient == null)
                        {
                            errorMessage = string.Format("Client with cell number {0} not found.", searchCriteria);
                            return false;
                        }
                        clients.Add(existingClient);
                    }
                    else
                    {
                        //Make sure the employee number is 8 digits long
                        while (searchCriteria.Length < 8)
                        {
                            searchCriteria = "0" + searchCriteria;
                        }

                        //Determine if the client is part of the company/group
                        if (groupCompanies)
                        {
                            companyGroup = db.CompanyGroups.Where(p => p.GroupName == selectedCompany).FirstOrDefault();
                            List<Company> validCompanies = new CompanyModel(_eventAggregator).ReadCompanies(true).Where(p => p.fkCompanyGroupID == companyGroup.pkCompanyGroupID).ToList();
                            List<Client> tempClients = db.Clients.Where(p => p.EmployeeNumber == searchCriteria).ToList();

                            foreach (Client tempClient in tempClients)
                            {
                                foreach (Company tempCompany in validCompanies)
                                {
                                    if (tempCompany.pkCompanyID == tempClient.fkCompanyID)
                                        clients.Add(tempClient);
                                }
                            }
                        }
                        else
                        {
                            company = db.Companies.Where(p => p.CompanyName == selectedCompany).FirstOrDefault();
                            clients = db.Clients.Where(p => p.EmployeeNumber == searchCriteria && p.fkCompanyID == company.pkCompanyID).ToList();
                        }
                        
                        if (clients.Count() == 0)
                        {
                            errorMessage = string.Format("Client with employee number {0} not found.", searchCriteria);
                            return false;
                        }
                    }

                }

                foreach (Client client in clients)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {

                        using (var db = MobileManagerEntities.GetContext())
                        {
                            clientToUpdate = db.Clients.Where(p => p.pkClientID == client.pkClientID).FirstOrDefault();

                            // Get the sql table structure of the entity
                            PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<Client>();

                            foreach (PropertyDescriptor property in properties)
                            {
                                mustUpdate = false;
                                // Get the source property and source value 
                                // mapped the simcard entity property
                                foreach (string mappedProperty in mappedProperties)
                                {
                                    string[] arrMappedProperty = mappedProperty.Split('=');
                                    string propertyName = new DataUpdatePropertyModel(_eventAggregator).GetPropertyName(arrMappedProperty[1].Trim(), enSelectedEntity);
                                    if (propertyName == property.Name)
                                    {
                                        importProperties = mappedProperty.Split('=');
                                        sourceProperty = importProperties[0].Trim();
                                        sourceValue = importValues[sourceProperty];
                                        dataChanged = mustUpdate = true;
                                        break;
                                    }
                                }

                                // Always update these values
                                if (dataChanged && (property.Name == "ModifiedBy" || property.Name == "ModifiedDate" || property.Name == "IsActive"))
                                    mustUpdate = true;

                                if (mustUpdate)
                                {
                                    //If an error occur it will tell in what column what value
                                    errorMessage = $"Error column '{sourceProperty.ToString()}' with value '{sourceValue.ToString()}' ";

                                    // Validate the source suburb and get
                                    // the value for the fkSuburbID
                                    if (property.Name == "fkSuburbID")
                                    {
                                        Suburb suburb = db.Suburbs.Where(p => p.SuburbName == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                        if (suburb == null)
                                        {
                                            errorMessage = "Suburb not found for nr." + searchCriteria;
                                            return false;
                                        }
                                        sourceValue = suburb.pkSuburbID;
                                    }
                                    //Validate the source company
                                    //and validate te value for the fkCompanyID
                                    if (property.Name == "fkCompanyID")
                                    {
                                        Company companyID = db.Companies.Where(p => p.CompanyName == sourceValue.ToString()).FirstOrDefault();
                                        if (companyID == null)
                                        {
                                            errorMessage = "Company not found for nr." + searchCriteria;
                                            return false;
                                        }
                                        sourceValue = companyID.pkCompanyID;
                                    }
                                    //Validate the source location and set the fkClientLocationID
                                    // to the location ID
                                    if (property.Name == "fkClientLocationID")
                                    {
                                        ClientLocation clientLocation = db.ClientLocations.Where(p => p.LocationDescription == sourceValue.ToString()).FirstOrDefault();
                                        if (clientLocation == null)
                                        {
                                            errorMessage = "Location not found for nr." + searchCriteria;
                                            return false;
                                        }
                                        sourceValue = clientLocation.pkClientLocationID;
                                    }
                                    //Validate the IsPrivate value
                                    //and conver string to bool
                                    if (property.Name == "IsPrivate")
                                    {
                                        if (sourceValue.ToString().ToUpper() == "PRIVATE")
                                            sourceValue = true;
                                        else if (sourceValue.ToString().ToUpper() == "Company")
                                            sourceValue = false;
                                        else
                                        {
                                            errorMessage = "Type not found (Should be 'Private' or 'Company'). ";
                                            return false;
                                        }
                                    }

                                    // Set the default values
                                    if (property.Name == "ModifiedBy")
                                        sourceValue = SecurityHelper.LoggedInFullName;
                                    if (property.Name == "ModifiedDate")
                                        sourceValue = DateTime.Now;
                                    if (property.Name == "IsActive")
                                        sourceValue = client.IsActive;

                                    // Convert the db type into the type of the property in our entity
                                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    {
                                        try
                                        { sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]); }
                                        catch
                                        { sourceValue = null; }
                                    }
                                    else if (property.PropertyType == typeof(System.DateTime))
                                    {
                                        //Excel date format convertion
                                        try
                                        { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                        catch { }
                                        try
                                        { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
                                        catch
                                        { return false; }
                                    }
                                    else if (property.PropertyType == typeof(System.Guid))
                                        sourceValue = new Guid(sourceValue.ToString());
                                    else if (property.PropertyType == typeof(System.Byte[]))
                                        sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                    else if (property.PropertyType == typeof(System.Boolean))
                                        sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                    else if (property.PropertyType == typeof(System.Int32))
                                        sourceValue = Convert.ToInt32(sourceValue.ToString());
                                    else if (property.PropertyType == typeof(System.Decimal))
                                        sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                    else if (property.PropertyType == typeof(System.Double))
                                        sourceValue = Convert.ToDouble(sourceValue.ToString());
                                    else
                                        sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                    // Set the value of the property with the value from the db
                                    property.SetValue(clientToUpdate, sourceValue);
                                }
                            }

                            if (dataChanged)
                            {
                                errorMessage = "Logging error please try the update again if error persist contact the development team.";

                                // Add the data activity log
                                result = _activityLogger.CreateDataChangeAudits<Client>(_dataActivityHelper.GetDataChangeActivities<Client>(client,
                                                                                                                                            clientToUpdate,
                                                                                                                                            clientToUpdate.fkContractID,
                                                                                                                                            db));
                                db.SaveChanges();
                                tc.Complete();
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CreateClientImport(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, bool ExistingClient, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Client existingClient = null;
            Client clientToCreate = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool state = true;
            //List to keep the information on all the client services
            List<int> contractServiceIDs = new List<int>();
            
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    //If existing client get the client to get the clients' details
                    if (ExistingClient)
                    {
                        existingClient = db.Clients.OrderByDescending(p => p.PrimaryCellNumber == searchCriteria).FirstOrDefault();

                        if (existingClient == null)
                        {
                            errorMessage = string.Format("Client with cell number {0} not found.", searchCriteria);
                            return false;
                        }
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    if (ExistingClient)
                    {
                        clientToCreate = (Client)existingClient;
                        clientToCreate.pkClientID = 0;
                        clientToCreate.fkClientBillingID = clientToCreate.fkContractID = 0;
                    }
                    else
                    {
                        clientToCreate = new Client();
                        clientToCreate.PrimaryCellNumber = searchCriteria;
                    }

                    //Creates the empty client billing
                    clientToCreate.ClientBilling = new ClientBilling();
                    clientToCreate.ClientBilling.pkClientBillingID = 0;
                    clientToCreate.ClientBilling.IsBillable = false;
                    clientToCreate.ClientBilling.IsSplitBilling = false;
                    clientToCreate.ClientBilling.SplitBillingException = false;
                    clientToCreate.ClientBilling.VoiceAllowance = 0;
                    clientToCreate.ClientBilling.InternationalDailing = false;
                    clientToCreate.ClientBilling.PermanentIntDailing = false;
                    clientToCreate.ClientBilling.InternationalRoaming = false;
                    clientToCreate.ClientBilling.PermanentIntRoaming = false;
                    clientToCreate.ClientBilling.ModifiedBy = SecurityHelper.LoggedInFullName;
                    clientToCreate.ClientBilling.ModifiedDate = DateTime.Now;
                    clientToCreate.ClientBilling.IsActive = true;

                    //Creates an empty contract
                    clientToCreate.Contract = new Contract();
                    clientToCreate.Contract.pkContractID = 0;

                    using (var db = MobileManagerEntities.GetContext())
                    {
                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<Client>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            mustUpdate = false;
                            // Get the source property and source value 
                            // mapped the simcard entity property
                            foreach (string mappedProperty in mappedProperties)
                            {
                                string[] arrMappedProperty = mappedProperty.Split('=');
                                string propertyName = new DataImportPropertyModel(_eventAggregator).GetPropertyName(arrMappedProperty[1].Trim(), enSelectedEntity);
                                if (arrMappedProperty[1].Trim() == "Service")
                                {
                                    try
                                    { contractServiceIDs.Add(new ContractServiceModel(_eventAggregator).ReadContractServiceID(Convert.ToString(importValues[importProperties[0].Trim()]), db));}
                                    catch
                                    { errorMessage = "Service description not found"; }
                                }
                                if (propertyName == property.Name)
                                {
                                    importProperties = mappedProperty.Split('=');
                                    sourceProperty = importProperties[0].Trim();
                                    sourceValue = importValues[sourceProperty];
                                    dataChanged = mustUpdate = true;
                                    break;
                                }
                            }

                            // Always update these values
                            if (dataChanged && (property.Name == "ModifiedBy" || property.Name == "ModifiedDate" || property.Name == "IsActive"))
                                mustUpdate = true;

                            if (mustUpdate)
                            {
                                //If an error occur it will tell in what column what value
                                errorMessage = $"Error column '{sourceProperty.ToString()}' with value '{sourceValue.ToString()}' ";

                                // Validate the source suburb and get
                                // the value for the fkSuburbID
                                if (property.Name == "fkSuburbID")
                                {
                                    Suburb suburb = db.Suburbs.Where(p => p.SuburbName == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (suburb == null)
                                    {
                                        errorMessage = "Suburb not found for nr." + searchCriteria;
                                        return false;
                                    }
                                    sourceValue = suburb.pkSuburbID;
                                }
                                //Validate the source company
                                //and validate te value for the fkCompanyID
                                if (property.Name == "fkCompanyID")
                                {
                                    Company company = db.Companies.Where(p => p.CompanyName == sourceValue.ToString()).FirstOrDefault();
                                    if (company == null)
                                    {
                                        errorMessage = "Company not found for nr." + searchCriteria;
                                        return false;
                                    }
                                    sourceValue = company.pkCompanyID;
                                }
                                //Validate the source location and set the fkClientLocationID
                                // to the location ID
                                if (property.Name == "fkClientLocationID")
                                {
                                    ClientLocation clientLocation = db.ClientLocations.Where(p => p.LocationDescription == sourceValue.ToString()).FirstOrDefault();
                                    if (clientLocation == null)
                                    {
                                        errorMessage = "Location not found for nr." + searchCriteria;
                                        return false;
                                    }
                                    sourceValue = clientLocation.pkClientLocationID;
                                }
                                //Validate the IsPrivate value
                                //and conver string to bool
                                if (property.Name == "IsPrivate")
                                {
                                    if (sourceValue.ToString().ToUpper() == "PRIVATE")
                                        sourceValue = true;
                                    else if (sourceValue.ToString().ToUpper() == "COMPANY")
                                        sourceValue = false;
                                    else
                                    {
                                        errorMessage = "Type not found (Should be 'Private' or 'Company'). ";
                                        return false;
                                    }
                                }

                                // Set the default values
                                if (property.Name == "ModifiedBy")
                                    sourceValue = SecurityHelper.LoggedInFullName;
                                if (property.Name == "ModifiedDate")
                                    sourceValue = DateTime.Now;
                                if (property.Name == "IsActive")
                                    sourceValue = state;

                                // Convert the db type into the type of the property in our entity
                                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    try
                                    { sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]); }
                                    catch
                                    { sourceValue = null; }
                                }
                                else if (property.PropertyType == typeof(System.DateTime))
                                {
                                    //Excel date format convertion
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch { }
                                    try
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
                                    catch
                                    { return false; }
                                }
                                else if (property.PropertyType == typeof(System.Guid))
                                    sourceValue = new Guid(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Byte[]))
                                    sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Int32))
                                    sourceValue = Convert.ToInt32(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Decimal))
                                    sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Double))
                                    sourceValue = Convert.ToDouble(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(clientToCreate, sourceValue);
                            }
                        }
                        
                        //--------------------------------------------------------------------------------------------------------------------------------------

                        //Create the clients contract information
                        
                        

                        //Updates the package setup for the client
                        PackageSetup packageSetupComplete = new PackageSetupModel(_eventAggregator).ImportPackageSetupImport(searchCriteria,
                                                                                         mappedProperties,
                                                                                         importValues,
                                                                                         enSelectedEntity,
                                                                                         db,
                                                                                         out errorMessage);
                        clientToCreate.Contract.PackageSetup = packageSetupComplete;
                        if (errorMessage != "Success")
                            return false;

                        errorMessage = $"Error column '{sourceProperty.ToString()}' with value '{sourceValue.ToString()}' ";

                        //Check if services are being updated
                        bool newServices = true;
                        //Create the Services for the contract
                        foreach (string mapping in mappedProperties)
                        {
                            if (mapping.Contains("Service"))
                            {
                                if (newServices)
                                {
                                    new ClientServiceModel(_eventAggregator).DeleteClientServices(clientToCreate.Contract.pkContractID, db);
                                    newServices = false;
                                }
                                string[] arrMapping = mapping.Split('=');
                                string sheetHeader = arrMapping[0].Trim();
                                int serviceID = 0;

                                try
                                {
                                    string tmpValue = importValues[sheetHeader].ToString().ToUpper();
                                    ContractService tmpValueCS = db.ContractServices.Where(p => p.ServiceDescription == tmpValue).FirstOrDefault();
                                    serviceID = tmpValueCS.pkContractServiceID;
                                }
                                catch
                                {
                                    errorMessage = string.Format("Service not found for cell nr. {0}", searchCriteria);
                                    return false;
                                }

                                ClientService clientService = new ClientService();
                                clientService.fkContractID = clientToCreate.Contract.pkContractID;
                                clientService.fkContractServiceID = serviceID;
                                clientService.ModifiedBy = SecurityHelper.LoggedInFullName;
                                clientService.ModifiedDate = DateTime.Now;
                                clientToCreate.Contract.ClientServices.Add(clientService);
                            }
                        }

                        // Get the sql table structure of the entity
                        properties = EDMHelper.GetEntityStructure<Contract>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            mustUpdate = false;
                            // Get the source property and source value 
                            // mapped the simcard entity property
                            foreach (string mappedProperty in mappedProperties)
                            {
                                string[] arrMappedProperty = mappedProperty.Split('=');
                                string propertyName = new DataImportPropertyModel(_eventAggregator).GetPropertyName(arrMappedProperty[1].Trim(), enSelectedEntity);
                                if (propertyName == property.Name)
                                {
                                    importProperties = mappedProperty.Split('=');
                                    sourceProperty = importProperties[0].Trim();
                                    sourceValue = importValues[sourceProperty];
                                    dataChanged = mustUpdate = true;
                                    break;
                                }
                            }

                            // Always update these values
                            if (dataChanged && (property.Name == "ModifiedBy" || property.Name == "ModifiedDate" || property.Name == "IsActive"))
                                mustUpdate = true;

                            if (mustUpdate)
                            {
                                //If an error occur it will tell in what column what value
                                errorMessage = $"Error column '{sourceProperty.ToString()}' with value '{sourceValue.ToString()}' ";

                                // Validate the source status and get
                                // the value for the fkStatusID
                                if (property.Name == "fkStatusID")
                                {
                                    Status status = db.Status.Where(p => p.StatusDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (status == null)
                                    {
                                        errorMessage = "Status not found for nr." + searchCriteria;
                                        return false;
                                    }

                                    // Set the contract and client to in-active 
                                    // if the status is not active
                                    if (status.StatusDescription != "ACTIVE")
                                    {
                                        state = false;
                                        clientToCreate.IsActive = state;
                                    }
                                    else
                                        clientToCreate.IsActive = state;

                                    sourceValue = status.pkStatusID;
                                }
                                // Validate the source package and get
                                // the value for the fkPackageID
                                if (property.Name == "fkPackageID")
                                {
                                    Package package = db.Packages.Where(p => p.PackageName.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (package == null)
                                    {
                                        errorMessage = "Package not found for nr." + searchCriteria;
                                        return false;
                                    }

                                    sourceValue = package.pkPackageID;
                                    
                                }
                                // Validate the CostType and get
                                // the value for the enCostType
                                if (property.Name == "enCostType")
                                {
                                    sourceValue = EnumHelper.GetEnumFromDescription<CostType>(sourceValue.ToString().ToUpper()).Value();
                                }
                                // Validate the date and get
                                // the correct date format
                                if (property.Name == "ContractStartDate" || property.Name == "ContractEndDate" || property.Name == "ContractUpgradeDate")
                                {
                                    bool validDate = false;
                                    try
                                    { int temp = Convert.ToInt32(sourceValue); }
                                    catch
                                    { validDate = true; }

                                    if (validDate)
                                        sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString()));
                                    else
                                        sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString())));
                                }
                                //Check when a payment cancel period is provider
                                //that it is in the correct format.
                                if (property.Name == "PaymentCancelPeriod")
                                {
                                    try
                                    {
                                        int year = Convert.ToInt32(sourceValue.ToString().Substring(0, 4));
                                        int month = Convert.ToInt32(sourceValue.ToString().Substring(5, 2));
                                        sourceValue = $"{sourceValue.ToString().Substring(0, 4)}/{sourceValue.ToString().Substring(5, 2)}";
                                    }
                                    catch
                                    {
                                        errorMessage = "Error: Payment cancel should be in format yyyy/mm (2050/09). ";
                                        return false;
                                    }
                                }

                                // Set the default values
                                if (property.Name == "ModifiedBy")
                                    sourceValue = SecurityHelper.LoggedInFullName;
                                if (property.Name == "ModifiedDate")
                                    sourceValue = DateTime.Now;
                                if (property.Name == "IsActive")
                                    sourceValue = state;

                                // Convert the db type into the type of the property in our entity
                                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    try
                                    { sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]); }
                                    catch
                                    { sourceValue = null; }
                                }
                                else if (property.PropertyType == typeof(System.DateTime))
                                {
                                    //Excel date format convertion
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch { }
                                    try
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
                                    catch
                                    { return false; }
                                }
                                else if (property.PropertyType == typeof(System.Guid))
                                    sourceValue = new Guid(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Byte[]))
                                    sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Int32))
                                    sourceValue = Convert.ToInt32(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Decimal))
                                    sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Double))
                                    sourceValue = Convert.ToDouble(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(clientToCreate.Contract, sourceValue);
                            }
                        }
                        
                        if (dataChanged)
                        {
                            errorMessage = "Error: Saving Error if persist contact developer.";
                            db.ClientBillings.Add(clientToCreate.ClientBilling);
                            db.PackageSetups.Add(clientToCreate.Contract.PackageSetup);
                            db.SaveChanges();
                            db.Contracts.Add(clientToCreate.Contract);
                            db.SaveChanges();
                            db.Clients.Add(clientToCreate);
                            db.SaveChanges();

                            errorMessage = "Creating client contract service error";
                            //Add all the contract services
                            foreach (int contractServiceID in contractServiceIDs)
                            {
                                ClientService clientService = new ClientService();
                                clientService.pkClientServiceID = 0;
                                clientService.fkContractID = clientToCreate.Contract.pkContractID;
                                clientService.fkContractServiceID = contractServiceID;
                                clientService.ModifiedBy = SecurityHelper.LoggedInFullName;
                                clientService.ModifiedDate = DateTime.Now;
                                new ClientServiceModel(_eventAggregator).CreateClientService(clientService, db);
                            }
                            
                            tc.Complete();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
