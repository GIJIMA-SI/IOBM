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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ClientBillingModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public ClientBillingModel()
        {
        }

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ClientBillingModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Update an existing client billing entity in the database
        /// </summary>
        /// <param name="searchEntity">The client billing data to search on.</param>
        /// <param name="searchCriteria">The client search criteria to search for.</param>
        /// <param name="updateColumn">The client billing entity property (column) to update.</param>
        /// <param name="updateValue">The value to update client billing entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClientBilling(SearchEntity searchEntity, string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            Client existingClient = null;
            ClientBilling clientBillingCurrent = null;
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

                    if (existingClient == null)
                    {
                        errorMessage = string.Format("Client not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                        return false;
                    }

                    clientBillingCurrent = db.ClientBillings.Where(p => p.pkClientBillingID == existingClient.fkClientBillingID).FirstOrDefault();
                    
                }

                if (clientBillingCurrent != null)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        using (var db = MobileManagerEntities.GetContext())
                        {
                            ClientBilling clientBillingToUpdate = db.ClientBillings.Where(p => p.pkClientBillingID == clientBillingCurrent.pkClientBillingID).FirstOrDefault();

                            // Get the client table properties (Fields)
                            PropertyDescriptor[] entityProperties = EDMHelper.GetEntityStructure<ClientBilling>();

                            foreach (PropertyDescriptor property in entityProperties)
                            {
                                // Find the data column (property) to update
                                if (property.Name == updateColumn)
                                {
                                    // Convert the db type into the type of the property in our entity
                                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                        updateValue = Convert.ChangeType(updateValue, property.PropertyType.GetGenericArguments()[0]);
                                    else if (property.PropertyType == typeof(Guid))
                                        updateValue = new Guid(updateValue.ToString());
                                    else if (property.PropertyType == typeof(Byte[]))
                                        updateValue = Convert.FromBase64String(updateValue.ToString());
                                    else
                                        updateValue = Convert.ChangeType(updateValue, property.PropertyType);

                                    // Set the value of the property with the value from the db
                                    property.SetValue(clientBillingToUpdate, updateValue);

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<ClientBilling>(_dataActivityHelper.GetDataChangeActivities<ClientBilling>(clientBillingCurrent, clientBillingToUpdate, existingClient.fkContractID, db));

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
                    errorMessage = string.Format("Client billing not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                    return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ClientBillingModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateClientBilling",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update billing data from spread sheet
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="enSelectedEntity"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool UpdateClientBillingUpdate(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            ClientBilling existingClientBilling = null;
            ClientBilling clientBillingToImport = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool result = false;
            bool state = true;
            string errorHelp = "";

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    try
                    {
                        int clientBillingID = db.Clients.Where(p => p.PrimaryCellNumber == searchCriteria).FirstOrDefault().fkClientBillingID;
                        existingClientBilling = db.ClientBillings.Where(p => p.pkClientBillingID == clientBillingID).FirstOrDefault();
                    }
                    catch
                    { 
                        errorMessage = string.Format("Contract with cell number {0} not found.", searchCriteria);
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {

                    using (var db = MobileManagerEntities.GetContext())
                    {
                        clientBillingToImport = db.ClientBillings.Where(p => p.pkClientBillingID == existingClientBilling.pkClientBillingID).FirstOrDefault();
                        
                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<ClientBilling>();

                        foreach (PropertyDescriptor property in properties)
                        {
                            mustUpdate = false;
                            errorHelp = property.Name.ToString();
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
                                // Validate the source billing level and get
                                // the value for the fkCompanyBillingLevelID
                                if (property.Name == "fkCompanyBillingLevelID")
                                {
                                    BillingLevel billingLevel = db.BillingLevels.Where(p => p.LevelDescription.ToUpper() == sourceValue.ToString().ToUpper().Trim()).FirstOrDefault();

                                    if (billingLevel == null)
                                        sourceValue = null;
                                    else
                                        sourceValue = billingLevel.pkBillingLevelID;
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
                                else if (property.PropertyType == typeof(System.Guid))
                                    sourceValue = new Guid(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Byte[]))
                                    sourceValue = Convert.FromBase64String(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.DateTime))
                                {
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
                                }
                                else if (property.PropertyType == typeof(System.Boolean))
                                {
                                    try
                                    { sourceValue = Convert.ToBoolean(Convert.ToInt32(sourceValue.ToString())); }
                                    catch { sourceValue = Convert.ToBoolean(sourceValue.ToString()); }
                                }
                                else if (property.PropertyType == typeof(System.Int32))
                                    sourceValue = Convert.ToInt32(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Decimal))
                                    sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Double))
                                    sourceValue = Convert.ToDouble(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(clientBillingToImport, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            // Add the data activity log
                            result = _activityLogger.CreateDataChangeAudits<ClientBilling>(_dataActivityHelper.GetDataChangeActivities<ClientBilling>(existingClientBilling, 
                                                                                                                                                      clientBillingToImport,
                                                                                                                                                      db.Clients.Where(p => p.PrimaryCellNumber == searchCriteria).FirstOrDefault().fkContractID,
                                                                                                                                                      db));

                            db.SaveChanges();
                            tc.Complete();
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                errorMessage = errorHelp + ": " + ex.Message;
                return false;
            }
        }
    }
}
