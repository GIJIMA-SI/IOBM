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
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ContractModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public ContractModel()
        {
        }

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ContractModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Update an existing client contract entity in the database
        /// </summary>
        /// <param name="searchEntity">The contract data to search on.</param>
        /// <param name="searchCriteria">The contract search criteria to search for.</param>
        /// <param name="updateColumn">The contract entity property (column) to update.</param>
        /// <param name="updateValue">The value to update contract entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateContract(SearchEntity searchEntity, string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            Contract existingContract = null;
            Contract contractToUpdate = null;
            bool result = false;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (searchEntity)
                    {
                        case SearchEntity.CellNumber:
                            existingContract = db.Contracts.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                            break;
                        case SearchEntity.AccountNumber:
                            existingContract = db.Contracts.Where(p => p.AccountNumber == searchCriteria).FirstOrDefault();
                            break;
                    }

                    if (existingContract == null)
                    {
                        errorMessage = string.Format("Contract not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                        return false;
                    }
                }

                if (existingContract != null)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        using (var db = MobileManagerEntities.GetContext())
                        {
                            contractToUpdate = db.Contracts.Where(p => p.pkContractID == existingContract.pkContractID).FirstOrDefault();

                            // Get the contract table properties (Fields)
                            PropertyDescriptor[] entityProperties = EDMHelper.GetEntityStructure<Contract>();

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
                                    property.SetValue(contractToUpdate, updateValue);

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<Contract>(_dataActivityHelper.GetDataChangeActivities<Contract>(existingContract, contractToUpdate, contractToUpdate.pkContractID, db));

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
                    errorMessage = string.Format("Client contract not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                    return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ContractModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateContract",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update client contract from spreadsheet
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="enSelectedEntity"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool UpdateContractUpdate(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Contract existingContract = null;
            Contract contractToImport = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool result = false;
            bool state = true;
            string errorHelp = "";

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    existingContract = db.Contracts.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                    if (existingContract == null)
                    {
                        errorMessage = string.Format("Cell number {0} not found.", searchCriteria);
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {

                    using (var db = MobileManagerEntities.GetContext())
                    {
                        contractToImport = db.Contracts.Where(p => p.pkContractID == existingContract.pkContractID).FirstOrDefault();
                        
                        //Updates the package setup for the client
                        bool packageSetupComplete = new PackageSetupModel(_eventAggregator).UpdatePackageSetupUpdate(searchCriteria,
                                                                                         mappedProperties,
                                                                                         importValues,
                                                                                         enSelectedEntity,
                                                                                         db,
                                                                                         out errorMessage);
                        if (!packageSetupComplete)
                            return false;

                        //Remove all the client services to add import values
                        new ClientServiceModel(_eventAggregator).DeleteClientServices(contractToImport.pkContractID, db);
                        //Create the Services for the contract
                        foreach (string mapping in mappedProperties)
                        {
                            if (mapping.Contains("Service"))
                            {
                                string[] arrMapping = mapping.Split('=');
                                string sheetHeader = arrMapping[0].Trim();
                                int serviceID = 0;

                                try
                                {
                                    string tmp = importValues[sheetHeader].ToString().ToUpper();
                                    ContractService tmp1 = db.ContractServices.Where(p => p.ServiceDescription == tmp).FirstOrDefault();
                                    serviceID = tmp1.pkContractServiceID;
                                }
                                catch
                                {
                                    errorMessage = string.Format("Service not found for cell nr. {0}", searchCriteria);
                                    return false;
                                }
                                
                                ClientService clientService = new ClientService();
                                clientService.fkContractID = contractToImport.pkContractID;
                                clientService.fkContractServiceID = serviceID;
                                clientService.ModifiedBy = SecurityHelper.LoggedInFullName;
                                clientService.ModifiedDate = DateTime.Now;
                                contractToImport.ClientServices.Add(clientService);
                            }
                        }

                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<Contract>();

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

                                    // Set the device to in-active 
                                    // if the status is not issued
                                    if (status.StatusDescription != "ACTIVE")
                                        state = false;

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
                                    sourceValue = EnumHelper.GetEnumFromDescription<CostType>("AMORTIZED").Value();
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
                                    try
                                    { sourceValue = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(sourceValue.ToString()))); }
                                    catch
                                    { sourceValue = Convert.ToDateTime(Convert.ToDateTime(sourceValue.ToString())); }
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
                                property.SetValue(contractToImport, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            // Add the data activity log
                            result = _activityLogger.CreateDataChangeAudits<Contract>(_dataActivityHelper.GetDataChangeActivities<Contract>(existingContract,
                                                                                                                                            contractToImport,
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
                errorMessage = searchCriteria + ", " + errorHelp + ": " + ex.Message;
                return false;
            }
        }
    }
}

