using Gijima.IOBM.Infrastructure.Events;
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
using System.Reflection;
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class SimCardModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public SimCardModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new Sim card entity in the database
        /// </summary>
        /// <param name="simCard">The Sim card entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateSimCard(SimCard simCard)
        {
            // Get the available status ID to be used in re-allaction valdation
            int availableSatusID;
            // Get the available re-allocated ID to be used to update device
            int reAllocatedID;
            //Simcard that can be re - allocated
            SimCard availableSimCard;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    availableSatusID = db.Status.Where(p => p.StatusDescription == "AVAILABLE").First().pkStatusID;
                    reAllocatedID = db.Status.Where(p => p.StatusDescription == "REALLOCATED").First().pkStatusID;

                    // If a sim card gets re-allocated ensure that all the required properties 
                    // is valid to allow re-alloaction
                    if (db.SimCards.Any(p => p.CellNumber == simCard.CellNumber &&
                                             p.PUKNumber.ToUpper().Trim() == simCard.PUKNumber.ToUpper().Trim() &&
                                             p.fkStatusID != availableSatusID &&
                                             p.IsActive == true))
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("SimCardModel",
                                                                        "The simcard is still allocated to another client.",
                                                                        "CreateSimCard",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }

                    availableSimCard = db.SimCards.Where(p => p.CellNumber == simCard.CellNumber && p.PUKNumber == simCard.PUKNumber && p.IsActive == false && p.fkStatusID == availableSatusID).FirstOrDefault();
                }
                
                using (var db = MobileManagerEntities.GetContext())
                {
                    //Re-alocate the simcard
                    if (db.SimCards.Any(p => p.CellNumber == simCard.CellNumber && p.PUKNumber == simCard.PUKNumber && p.IsActive == false && p.fkStatusID == availableSatusID))
                    {
                        if (availableSimCard == null)
                            return false;

                        SimCard reAllocatedSimCard = db.SimCards.Where(p => p.CellNumber == simCard.CellNumber && p.PUKNumber == simCard.PUKNumber && p.IsActive == false && p.fkStatusID == availableSatusID).FirstOrDefault();
                        reAllocatedSimCard.fkStatusID = reAllocatedID;

                        _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(availableSimCard, reAllocatedSimCard, reAllocatedSimCard.fkContractID.Value, db));

                        db.SimCards.Add(simCard);
                        db.SaveChanges();
                        return true;
                    }

                    if (!db.SimCards.Any(p => p.CellNumber == simCard.CellNumber && p.PUKNumber == simCard.PUKNumber))
                    {
                        db.SimCards.Add(simCard);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("SimCardModel",
                                                                        "The simcard already exist.",
                                                                        "CreateSimCard",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                     .Publish(new ApplicationMessage(this.GetType().Name,
                                              string.Format("Error! {0}, {1}.",
                                              ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                              MethodBase.GetCurrentMethod().Name,
                                              ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Create a new Sim card entity in the database (Import)
        /// </summary>
        /// <param name="simCard"></param>
        /// <param name="db"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool CreateSimCard(SimCard simCard, MobileManagerEntities db, ref string errorMessage)
        {
            try
            {
                // Get the re-allacted status ID to be used in re-allaction valdation
                int reAllocatedStatusID = db.Status.Where(p => p.StatusDescription == "REALLOCATED").First().pkStatusID;

                // If a sim card gets re-allocated ensure that all the required properties 
                // is valid to allow re-alloaction
                if (db.SimCards.Any(p => p.CellNumber == simCard.CellNumber &&
                                         p.PUKNumber.ToUpper().Trim() == simCard.PUKNumber.ToUpper().Trim() &&
                                         p.fkStatusID != reAllocatedStatusID &&
                                         p.fkContractID != simCard.fkContractID))
                {
                    errorMessage = "The simcard is still allocated to another client.";
                    return false;
                }
                // If a sim card gets re-allocated ensure that all the required properties 
                // is valid to allow re-alloaction
                if (db.SimCards.Any(p => p.CellNumber == simCard.CellNumber &&
                                         //p.PUKNumber.ToUpper().Trim() == simCard.PUKNumber.ToUpper().Trim() &&
                                         p.fkContractID == simCard.fkContractID &&
                                         p.CardNumber == simCard.CardNumber))
                {
                    errorMessage = "The simcard details hasn't changed";
                    return false;
                }

                if (!db.SimCards.Any(p => p.CellNumber == simCard.CellNumber && p.PUKNumber == simCard.PUKNumber))
                {
                    db.SimCards.Add(simCard);
                    db.SaveChanges();
                    return true;
                }
                else
                {
                    errorMessage = "Simcard already exist";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Read all or active only Sim cards from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of SimCard</returns>
        public ObservableCollection<SimCard> ReadSimCard(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<SimCard> simCards = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    simCards = ((DbQuery<SimCard>)(from simCard in db.SimCards
                                                   where activeOnly ? simCard.IsActive : true &&
                                                         excludeDefault ? simCard.pkSimCardID > 0 : true
                                                   select simCard)).ToList();

                    return new ObservableCollection<SimCard>(simCards);
                }
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
        /// Read all or active only Sim cards linked to the specified contract from the database
        /// </summary>
        /// <param name="contractID">The contract primary key linked to the SimCards.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of SimCard</returns>
        public ObservableCollection<SimCard> ReadSimCardsForContract(int contractID, bool activeOnly = false)
        {
            try
            {
                IEnumerable<SimCard> simCards = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    simCards = ((DbQuery<SimCard>)(from simCard in db.SimCards
                                                   where simCard.fkContractID == contractID
                                                   select simCard)).Include("Status")
                                                                   .OrderByDescending(p => p.IsActive)
                                                                   .ThenBy(p => p.Status.StatusDescription).ToList();

                    if (activeOnly)
                        simCards = simCards.Where(p => p.IsActive);

                    return new ObservableCollection<SimCard>(simCards);
                }
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
        /// Update an existing Sim card entity in the database
        /// </summary>
        /// <param name="simCard">The Sim card entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateSimCard(SimCard simCard)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    SimCard existingSimCard = db.SimCards.Where(p => p.pkSimCardID == simCard.pkSimCardID).FirstOrDefault();

                    // Check to see if the sim card already exist for another entity 
                    if (existingSimCard != null && existingSimCard.pkSimCardID != simCard.pkSimCardID &&
                        existingSimCard.CellNumber == simCard.CellNumber &&
                        existingSimCard.PUKNumber == simCard.PUKNumber &&
                        existingSimCard.fkStatusID == Statuses.ISSUED.Value())
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("SimCardModel",
                                                                        "The simcard already exist. Or is allocated to another contract.",
                                                                        "UpdateSimCard",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                    else if (existingSimCard != null)
                    {
                        // Log the data values that changed
                        _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimCard, simCard, simCard.fkContractID.Value, db));

                        // Save the new values
                        existingSimCard.fkStatusID = simCard.fkStatusID;
                        existingSimCard.CardNumber = simCard.CardNumber;
                        existingSimCard.CellNumber = simCard.CellNumber;
                        existingSimCard.PinNumber = simCard.PinNumber;
                        existingSimCard.PUKNumber = simCard.PUKNumber;
                        existingSimCard.ReceiveDate = simCard.ReceiveDate;
                        existingSimCard.IsActive = simCard.IsActive;
                        existingSimCard.ModifiedBy = simCard.ModifiedBy;
                        existingSimCard.ModifiedDate = simCard.ModifiedDate;
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update an existing sim card entity in the database
        /// </summary>
        /// <param name="searchEntity">The simcard data to search on.</param>
        /// <param name="searchCriteria">The simcard search criteria to search for.</param>
        /// <param name="updateColumn">The simcard entity property (column) to update.</param>
        /// <param name="updateValue">The value to update simcard entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateSimCard(SearchEntity searchEntity, string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            SimCard existingSimCard = null;
            SimCard SimCardToUpdate = null;
            bool result = false;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    existingSimCard = db.SimCards.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                }

                if (existingSimCard != null)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        using (var db = MobileManagerEntities.GetContext())
                        {
                            SimCardToUpdate = db.SimCards.Where(p => p.pkSimCardID == existingSimCard.pkSimCardID).FirstOrDefault();

                            // Get the simcard table properties (Fields)
                            PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<SimCard>();

                            foreach (PropertyDescriptor property in properties)
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
                                    property.SetValue(SimCardToUpdate, updateValue);

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimCard, SimCardToUpdate, SimCardToUpdate.fkContractID.Value, db));

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
                    errorMessage = string.Format("Simcard not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                    return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Import sim card entity into the database
        /// </summary>
        /// <param name="searchEntity">The simcard data to search on.</param>
        /// <param name="searchCriteria">The simcard search criteria to search for.</param>
        /// <param name="mappedProperties">The simcard properties (columns) to import.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateSimCard(SearchEntity searchEntity, string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            SimCard existingSimCard = null;
            SimCard simCardToImport = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool result = false;
            bool state = true;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    existingSimCard = db.SimCards.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();

                    if (existingSimCard == null)
                    {
                        errorMessage = string.Format("Cell number {0} not found.", searchCriteria);
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {

                    using (var db = MobileManagerEntities.GetContext())
                    {
                        simCardToImport = db.SimCards.Where(p => p.pkSimCardID == existingSimCard.pkSimCardID).FirstOrDefault();


                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<SimCard>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            mustUpdate = false;

                            // Get the source property and source value 
                            // mapped the simcard entity property
                            foreach (string mappedProperty in mappedProperties)
                            {
                                if (mappedProperty.Contains(property.Name))
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
                                // Validate the source status and get
                                // the value for the fkStatusID
                                if (property.Name == "fkStatusID")
                                {
                                    Status status = db.Status.Where(p => p.StatusDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (status == null)
                                    {
                                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                                        .Publish(new ApplicationMessage("SimCardModel",
                                                                 string.Format("Invalid status {0}.", sourceValue.ToString()),
                                                                 "ImportSimCard",
                                                                 ApplicationMessage.MessageTypes.SystemError));
                                        return false;
                                    }

                                    // Set the simcard to in-active 
                                    // if the status is not issued
                                    if (status.StatusDescription != "ISSUED")
                                        state = false;

                                    sourceValue = status.pkStatusID;
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
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]);
                                else if (property.PropertyType == typeof(System.Guid))
                                    sourceValue = new Guid(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Byte[]))
                                    sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.DateTime))
                                    sourceValue = Convert.ToDateTime(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(simCardToImport, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            // Add the data activity log
                            result = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimCard, simCardToImport, simCardToImport.fkContractID.Value, db));

                            db.SaveChanges();
                            tc.Complete();
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Import a new sim card entity into the database
        /// </summary>
        /// <param name="searchEntity">The simcard data to search on.</param>
        /// <param name="searchCriteria">The simcard search criteria to search for.</param>
        /// <param name="mappedProperties">The simcard properties (columns) to import.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool CreateSimCardImport(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            SimCard simCardToImport = null;
            Contract contract = null;
            bool mustImport = false;
            bool dataChanged = false;
            bool state = true;
            
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    //Check if number is linked to a contract
                    contract = db.Contracts.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                    if (contract == null || contract.IsActive == false)
                    {
                        errorMessage = string.Format("Cell number {0} not linked to a contract, ", searchCriteria);
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {

                    using (var db = MobileManagerEntities.GetContext())
                    {
                        //Create a new empty device to add properties
                        simCardToImport = new SimCard();
                        simCardToImport.pkSimCardID = 0;
                        simCardToImport.CellNumber = searchCriteria;
                        simCardToImport.fkContractID = contract.pkContractID;


                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<SimCard>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            mustImport = false;

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
                                    dataChanged = mustImport = true;
                                    break;
                                }
                            }

                            // Always update these values
                            if (dataChanged && (property.Name == "ModifiedBy" || property.Name == "ModifiedDate" || property.Name == "IsActive"))
                                mustImport = true;

                            if (mustImport)
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
                                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                                        .Publish(new ApplicationMessage("SimCardModel",
                                                                 string.Format("Invalid status {0}.", sourceValue.ToString()),
                                                                 "ImportSimCard",
                                                                 ApplicationMessage.MessageTypes.SystemError));
                                        return false;
                                    }

                                    // Set the simcard to in-active 
                                    // if the status is not issued
                                    if (status.StatusDescription != "ISSUED")
                                        state = false;

                                    sourceValue = status.pkStatusID;
                                }
                                //Test the excel spreadsheet date for actual date or numbers
                                if (property.Name == "ReceiveDate")
                                {
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
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
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]);
                                else if (property.PropertyType == typeof(System.Guid))
                                    sourceValue = new Guid(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Byte[]))
                                    sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.DateTime))
                                    sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString()));
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(simCardToImport, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            if (!CreateSimCard(simCardToImport, db, ref errorMessage))
                                return false;
                            // Add the data activity log
                            //result = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimCard, simCardToImport, simCardToImport.fkContractID.Value, db));

                            db.SaveChanges();
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

        /// <summary>
        /// Updates a simcard (data update)
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="enSelectedEntity"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool UpdateSimCardUpdate(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            SimCard existingSimcard = null;
            SimCard simcardToUpdate = null;
            Contract existingContract = null;
            Contract contractToUpdate = null;
            Client existingClient = null;
            Client clientToUpdate = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool state = true;
            
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    //Check if number is linked to a contract
                    existingSimcard = db.SimCards.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                    if (existingSimcard == null)
                    {
                        errorMessage = string.Format("Simcard with number {0} not found", searchCriteria);
                        return false;
                    }
                    else
                    {
                        //If the cell number should change update the contract and client
                        existingContract = db.Contracts.Where(p => p.pkContractID == existingSimcard.fkContractID).FirstOrDefault();
                        existingClient = db.Clients.Where(p => p.fkContractID == existingContract.pkContractID).FirstOrDefault();

                        if (existingContract == null || existingClient == null)
                        {
                            errorMessage = $"Contract/Client not found for cell number {searchCriteria} ";
                            return false;
                        }
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        simcardToUpdate = db.SimCards.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                        
                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<SimCard>();

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
                                //new number are updated and not set back at the end
                                bool updateNumber = false;

                                //If an error occur it will tell in what column what value
                                errorMessage = $"Error column '{sourceProperty.ToString()}' with value '{sourceValue.ToString()}' ";

                                // Validate the source status and get
                                // the value for the fkStatusID
                                if (property.Name == "fkStatusID")
                                {
                                    Status status = db.Status.Where(p => p.StatusDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (status == null)
                                    {
                                        return false;
                                    }

                                    // Set the simcard to in-active 
                                    // if the status is not issued
                                    if (status.StatusDescription != "ISSUED")
                                        state = false;

                                    sourceValue = status.pkStatusID;
                                }
                                //Test the excel spreadsheet date for actual date or numbers
                                if (property.Name == "ReceiveDate")
                                {
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
                                }
                                //Update the contract and the clinet primary cell number
                                if (property.Name == "CellNumber")
                                {
                                    contractToUpdate = db.Contracts.Where(p => p.pkContractID == existingSimcard.fkContractID).FirstOrDefault();
                                    clientToUpdate = db.Clients.Where(p => p.fkContractID == existingContract.pkContractID).FirstOrDefault();

                                    if (contractToUpdate.CellNumber == searchCriteria)
                                        contractToUpdate.CellNumber = sourceValue.ToString();
                                    if (clientToUpdate.PrimaryCellNumber == searchCriteria)
                                        clientToUpdate.PrimaryCellNumber = sourceValue.ToString();

                                    simcardToUpdate.CellNumber = sourceValue.ToString();
                                    updateNumber = true;
                                }

                                // Set the default values
                                if (property.Name == "ModifiedBy")
                                    sourceValue = SecurityHelper.LoggedInFullName;
                                if (property.Name == "ModifiedDate")
                                    sourceValue = DateTime.Now;
                                if (property.Name == "IsActive")
                                    sourceValue = state;

                                if (updateNumber == false)
                                {
                                    // Convert the db type into the type of the property in our entity
                                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                        sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]);
                                    else if (property.PropertyType == typeof(System.Guid))
                                        sourceValue = new Guid(sourceValue.ToString());
                                    else if (property.PropertyType == typeof(System.Byte[]))
                                        sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                    else if (property.PropertyType == typeof(System.DateTime))
                                        sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString()));
                                    else if (property.PropertyType == typeof(System.Boolean))
                                        sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                    else
                                        sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                    // Set the value of the property with the value from the db
                                    property.SetValue(simcardToUpdate, sourceValue);
                                }
                            }
                        }

                        if (dataChanged)
                        {
                            errorMessage = "Data logging error, if persist contact developer";
                            // Add the data activity log
                            object itemSimcard = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimcard, simcardToUpdate, simcardToUpdate.fkContractID.Value, db));
                            if (contractToUpdate != null && clientToUpdate != null)
                            {
                                object itemContract = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<Contract>(existingContract, contractToUpdate, contractToUpdate.pkContractID, db));
                                object itemClient = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<Client>(existingClient, clientToUpdate, clientToUpdate.fkContractID, db));
                                if (itemContract == null || itemClient == null)
                                    return false;
                            }

                            if (itemSimcard == null)
                                return false;

                            db.SaveChanges();
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

        /// <summary>
        /// Sets the linked simcards inactive
        /// </summary>
        /// <param name="contractID">The contract linked to the simcards</param>
        /// <param name="context">The context inside the transaction</param>
        public void ChangeSimcardsStatusForClient(int contractID, int statusID, MobileManagerEntities db)
        {
            try
            {
                IEnumerable<SimCard> simCards = db.SimCards.Where(p => p.fkContractID == contractID);

                if (statusID == Statuses.AVAILABLE.Value())
                {
                    foreach (SimCard simCard in simCards)
                    {
                        if (simCard.IsActive == true)
                            simCard.fkStatusID = Statuses.AVAILABLE.Value();
                        simCard.IsActive = false;
                    }
                }
                else
                {
                    foreach (SimCard simCard in simCards)
                    {
                        simCard.fkStatusID = Statuses.INACTIVE.Value();
                        simCard.IsActive = false;
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
            }
        }
    }
}
