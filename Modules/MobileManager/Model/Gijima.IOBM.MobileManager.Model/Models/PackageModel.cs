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
    public class PackageModel
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
        public PackageModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new package entity in the database
        /// </summary>
        /// <param name="package">The package entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreatePackage(Package package)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Packages.Any(p => p.PackageName.ToUpper() == package.PackageName))
                    {
                        db.Packages.Add(package);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} package already exist.", package.PackageName));
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
        /// Read all or active only packages from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Packages</returns>
        public ObservableCollection<Package> ReadPackages(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Package> packages = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    packages = ((DbQuery<Package>)(from package in db.Packages
                                                   where activeOnly ? package.IsActive : true &&
                                                         excludeDefault ? package.pkPackageID > 0 : true
                                                   select package)).Include("ServiceProvider")
                                                                   .OrderBy(p => p.PackageName).ToList();

                    return new ObservableCollection<Package>(packages);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all package names from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Packages</returns>
        public ObservableCollection<string> ReadPackageNames(bool activeOnly)
        {
            try
            {
                IEnumerable<Package> packages = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    packages = ((DbQuery<Package>)(from package in db.Packages
                                                   where activeOnly ? package.IsActive : true
                                                   select package)).OrderBy(p => p.PackageName).ToList();
                }

                //Converto to observabile collection of string
                ObservableCollection<string> packageNames = new ObservableCollection<string>();
                foreach (Package package in packages)
                {
                    packageNames.Add(package.PackageName);
                }
                return packageNames;
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
        /// Read the package name from the database
        /// </summary>
        /// <param name="packageID">The package ID to get the name for.</param>
        /// <returns>Package Name</returns>
        public string ReadPackageName(int packageID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.Packages.Where(p => p.pkPackageID == packageID).First().PackageName;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return string.Empty;
            }
        }

        /// <summary>
        /// Update an existing package entity in the database
        /// </summary>
        /// <param name="package">The package entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdatePackage(Package package)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Package existingPackage = db.Packages.Where(p => p.PackageName == package.PackageName).FirstOrDefault();

                    // Check to see if the package name already exist for another entity 
                    if (existingPackage != null && existingPackage.pkPackageID != package.pkPackageID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} package already exist.", package.PackageName));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingPackage != null)
                        {
                            existingPackage.fkServiceProviderID = package.fkServiceProviderID;
                            existingPackage.enPackageType = package.enPackageType;
                            existingPackage.PackageName = package.PackageName;
                            existingPackage.Cost = package.Cost;
                            existingPackage.MBData = package.MBData;
                            existingPackage.TalkTimeMinutes = package.TalkTimeMinutes;
                            existingPackage.SMSNumber = package.SMSNumber;
                            existingPackage.RandValue = package.RandValue;
                            existingPackage.ModifiedBy = package.ModifiedBy;
                            existingPackage.ModifiedDate = package.ModifiedDate;
                            existingPackage.IsActive = package.IsActive;
                            db.SaveChanges();
                            return true;
                        }

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
        /// Update the packages in the system update window
        /// </summary>
        /// <param name="searchEntity"></param>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool UpdatePackageUpdate(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Package existingPackage = null;
            Package packageToUpdate = null;
            bool mustUpdate = false;
            bool dataChanged = false;
            bool result = false;
            
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    existingPackage = db.Packages.Where(p => p.PackageName.ToUpper() == searchCriteria.ToUpper()).FirstOrDefault();

                    if (existingPackage == null)
                    {
                        errorMessage = string.Format("Package with name {0} not found.", searchCriteria);
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {

                    using (var db = MobileManagerEntities.GetContext())
                    {
                        packageToUpdate = db.Packages.Where(p => p.PackageName.ToUpper() == searchCriteria.ToUpper()).FirstOrDefault();


                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<Package>();

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

                                // Validate the source status and get
                                // the value for the fkStatusID
                                if (property.Name == "fkStatusID")
                                {
                                    Status status = db.Status.Where(p => p.StatusDescription.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (status == null)
                                    {
                                        errorMessage = $"Status {sourceValue.ToString()} not found.";
                                        return false;
                                    }
                                    
                                    sourceValue = status.pkStatusID;
                                }

                                // Validate the source service provider and get
                                // the value for the fkServiceProviderID
                                if (property.Name == "fkServiceProviderID")
                                {
                                    ServiceProvider serviceProvider = db.ServiceProviders.Where(p => p.ServiceProviderName.ToUpper() == sourceValue.ToString().ToUpper()).FirstOrDefault();

                                    if (serviceProvider == null)
                                    {
                                        errorMessage = $"Service provider {sourceValue.ToString()} not found.";
                                        return false;
                                    }

                                    sourceValue = serviceProvider.pkServiceProviderID;
                                }

                                // Validate the source package type and get
                                // the value for the enPackageType
                                if (property.Name == "enPackageType")
                                {
                                    int packageType = -1;
                                    packageType = EnumHelper.GetEnumFromDescription<PackageType>(sourceValue.ToString().ToUpper()).Value();

                                    if (packageType == -1)
                                    {
                                        errorMessage = $"Package type {sourceValue.ToString()} not found.";
                                        return false;
                                    }
                                    sourceValue = packageType.ToString(); ;
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
                                else if (property.PropertyType == typeof(System.DateTime))
                                    sourceValue = Convert.ToDateTime(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Int32))
                                    sourceValue = Convert.ToInt32(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Decimal))
                                    sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(packageToUpdate, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            // Add the data activity log
                            //_activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingPackage, packageToUpdate, packageToUpdate, db));

                            db.SaveChanges();
                            tc.Complete();
                            result = true;
                        }
                    }
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new package (Data Import)
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="mappedProperties"></param>
        /// <param name="importValues"></param>
        /// <param name="enSelectedEntity"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool CreatePackageImport(string searchCriteria, IEnumerable<string> mappedProperties, DataRow importValues, short enSelectedEntity, out string errorMessage)
        {
            errorMessage = string.Empty;
            string[] importProperties = null;
            string sourceProperty = string.Empty;
            object sourceValue = null;
            Package packageToImport = new Package();
            bool mustUpdate = false;
            bool dataChanged = false;
            
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    errorMessage = "Cant find service provider";
                    packageToImport.pkPackageID = 0;
                    packageToImport.fkServiceProviderID = 0;
                    packageToImport.fkServiceProviderID = db.ServiceProviders.Where(p => p.ServiceProviderName.ToUpper() == searchCriteria.ToUpper()).FirstOrDefault().pkServiceProviderID;

                    if (packageToImport.fkServiceProviderID == 0)
                    {
                        return false;
                    }
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        // Get the sql table structure of the entity
                        PropertyDescriptor[] properties = EDMHelper.GetEntityStructure<Package>();

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
                                
                                // Validate the source package type and get
                                // the value for the enPackageType
                                if (property.Name == "enPackageType")
                                {
                                    int packageType = -1;
                                    packageType = EnumHelper.GetEnumFromDescription<PackageType>(sourceValue.ToString().ToUpper()).Value();

                                    if (packageType == -1)
                                    {
                                        errorMessage = $"Package type {sourceValue.ToString()} not found.";
                                        return false;
                                    }
                                    sourceValue = packageType.ToString();
                                }
                                //Change the package name to Uppercase
                                if (property.Name == "PackageName")
                                {
                                    sourceValue = sourceValue.ToString().ToUpper();
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
                                else if (property.PropertyType == typeof(System.DateTime))
                                    sourceValue = Convert.ToDateTime(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Boolean))
                                    sourceValue = Convert.ToBoolean(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Int32))
                                    sourceValue = Convert.ToInt32(sourceValue.ToString());
                                else if (property.PropertyType == typeof(System.Decimal))
                                    sourceValue = Convert.ToDecimal(sourceValue.ToString());
                                else
                                    sourceValue = Convert.ChangeType(sourceValue, property.PropertyType);

                                // Set the value of the property with the value from the db
                                property.SetValue(packageToImport, sourceValue);
                            }
                        }

                        if (dataChanged)
                        {
                            errorMessage = "Saving data error if persist please contact developer";
                            // Add the data activity log
                            //_activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingPackage, packageToUpdate, packageToUpdate, db));
                            db.Packages.Add(packageToImport);
                            db.SaveChanges();
                            tc.Complete();
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
