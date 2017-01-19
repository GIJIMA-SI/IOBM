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
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    // Get the re-allacted status ID to be used in re-allaction valdation
                    int reAllocatedStatusID = db.Status.Where(p => p.StatusDescription == "REALLOCATED").First().pkStatusID;

                    // If a sim card gets re-allocated ensure that all the required properties 
                    // is valid to allow re-alloaction
                    if (db.SimCards.Any(p => p.CellNumber == simCard.CellNumber &&
                                             p.PUKNumber.ToUpper().Trim() == simCard.PUKNumber.ToUpper().Trim() &&
                                             p.fkStatusID != reAllocatedStatusID &&
                                             p.IsActive == true))
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("SimCardModel",
                                                                        "The simcard is still allocated to another client.",
                                                                        "CreateSimCard",
                                                                        ApplicationMessage.MessageTypes.Information));
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
                                .Publish(new ApplicationMessage("SimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateSimCard",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                                .Publish(new ApplicationMessage("SimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadSimCard",
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
                                                   select simCard)).Include("Devices")
                                                                   .Include("Devices.DeviceMake")
                                                                   .Include("Devices.DeviceModel")
                                                                   .Include("Status")
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
                                .Publish(new ApplicationMessage("SimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadSimCardsForContract",
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
                        existingSimCard.PUKNumber == simCard.PUKNumber)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("SimCardModel",
                                                                        "The simcard already exist.",
                                                                        "UpdateSimCard",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                    else if(existingSimCard != null)
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
                                .Publish(new ApplicationMessage("SimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateSimCard",
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
                                .Publish(new ApplicationMessage("SimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateSimCard",
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
        public bool ImportSimCard(SearchEntity searchEntity, string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Client existingClient = null;
            SimCard existingSimCard = null;
            SimCard simCardToImport = null;
            bool result = false;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    existingClient = db.Clients.Where(p => p.PrimaryCellNumber == searchCriteria).FirstOrDefault();

                    if (existingClient == null)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("SimCardModel",
                                                 string.Format("No client found for simcard {0}.", searchCriteria),
                                                 "ImportSimCard",
                                                 ApplicationMessage.MessageTypes.SystemError));
                        return false;
                    }

                    existingSimCard = db.SimCards.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                }

                using (var db = MobileManagerEntities.GetContext())
                {
                    // If simcard exist the update else
                    // add a new simcard
                    if (existingSimCard == null)
                        simCardToImport = new SimCard();
                    else
                        simCardToImport = db.SimCards.Where(p => p.pkSimCardID == existingSimCard.pkSimCardID).FirstOrDefault();

                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<SimCard>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            // Get the source property and source value 
                            // mapped the simcard entity property
                            foreach (string mappedProperty in mappedProperties)
                            {
                                if (mappedProperty.Contains(property.Name))
                                {
                                    importProperties = mappedProperty.Split('=');
                                    sourceProperty = importProperties[0].Trim();
                                    sourceValue = importValues[sourceProperty];
                                    break;
                                }
                            }

                            // Set the source value to the primary key
                            if (property.Name == "pkSimCardID")
                                sourceValue = simCardToImport.pkSimCardID;

                            // Set the source value to the client contract id
                            if (property.Name == "fkContractID")
                                sourceValue = existingClient.fkContractID;

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

                                sourceValue = status.pkStatusID;
                            }

                            // Set the default values
                            if (property.Name == "ModifiedBy")
                                sourceValue = SecurityHelper.LoggedInFullName;
                            if (property.Name == "ModifiedDate")
                                sourceValue = DateTime.Now;
                            if (property.Name == "IsActive")
                                sourceValue = true;

                            // Convert the db type into the type of the property in our entity
                            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                sourceValue = Convert.ChangeType(sourceValue, property.PropertyType.GetGenericArguments()[0]);
                            else if (property.PropertyType == typeof(System.Guid))
                                sourceValue = new Guid(sourceValue.ToString());
                            else if (property.PropertyType == typeof(System.Byte[]))
                                sourceValue = Convert.FromBase64String(sourceValue.ToString());
                            else
                                sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                            // Set the value of the property with the value from the db
                            property.SetValue(simCardToImport, sourceValue);
                        }

                        // If new simcard add it else
                        // update and add activity log
                        if (simCardToImport.pkSimCardID == 0)
                        {
                            db.SimCards.Add(simCardToImport);
                            result = true;
                        }
                        else
                        {
                            // Add the data activity log
                            result = _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimCard, simCardToImport, simCardToImport.fkContractID.Value, db));
                        }

                        db.SaveChanges();

                        // Commit changes
                        tc.Complete();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("SimCardModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ImportSimCard",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
