
using Gijima.IOBM.MobileManager.Model.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gijima.IOBM.MobileManager
{
    public class AdvancedSearchEntities
    {
        /// <summary>
        /// Collection of the name and type of entity
        /// </summary>
        public ObservableCollection<EntityDetail> EntityDetails
        {
            get { return _entityDetails; }
            set { _entityDetails = value; }
        }
        private ObservableCollection<EntityDetail> _entityDetails;
        /// <summary>
        /// Constructor
        /// </summary>
        public AdvancedSearchEntities()
        {
            EntityDetails = new ObservableCollection<EntityDetail>();
            CreateEntities();
        }

        /// <summary>
        /// Creates all the EntitieDetails
        /// </summary>
        public void CreateEntities()
        {
            #region old
            //var myEnum = typeof(AdvancedDataBaseEntity);

            //foreach(var field in myEnum.GetFields())
            //{
            //    string entityName = Convert.ToString(Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute);
            //    object entityType = null;

            //    switch (EnumHelper.GetEnumFromDescription<AdvancedDataBaseEntity>(entityName))
            //    {
            //        case AdvancedDataBaseEntity.None:
            //            entityType = null;
            //            break;
            //        case AdvancedDataBaseEntity.AuditLog:
            //            entityType = new AuditLog();
            //            break;
            //        case AdvancedDataBaseEntity.City:
            //            entityType = new City();
            //            break;
            //        case AdvancedDataBaseEntity.Client:
            //            entityType = new Client();
            //            break;
            //        case AdvancedDataBaseEntity.ClientBilling:
            //            entityType = new ClientBilling();
            //            break;
            //        case AdvancedDataBaseEntity.ClientLocation:
            //            entityType = new ClientLocation();
            //            break;
            //        case AdvancedDataBaseEntity.Company:
            //            entityType = new Company();
            //            break;
            //        case AdvancedDataBaseEntity.CompanyBillingLevel:
            //            entityType = new CompanyBillingLevel();
            //            break;
            //        case AdvancedDataBaseEntity.CompanyGroup:
            //            entityType = new CompanyGroup();
            //            break;
            //        case AdvancedDataBaseEntity.Contract:
            //            entityType = new Contract();
            //            break;
            //        case AdvancedDataBaseEntity.ContractService:
            //            entityType = new ContractService();
            //            break;
            //        case AdvancedDataBaseEntity.Device:
            //            entityType = new Device();
            //            break;
            //        case AdvancedDataBaseEntity.DeviceIMENumber:
            //            entityType = new DeviceIMENumber();
            //            break;
            //        case AdvancedDataBaseEntity.DeviceMake:
            //            entityType = new DeviceMake();
            //            break;
            //        case AdvancedDataBaseEntity.DeviceModel:
            //            entityType = new DeviceModel();
            //            break;
            //        case AdvancedDataBaseEntity.Invoice:
            //            entityType = new Invoice();
            //            break;
            //        case AdvancedDataBaseEntity.InvoiceDetail:
            //            entityType = new InvoiceDetail();
            //            break;
            //        case AdvancedDataBaseEntity.Package:
            //            entityType = new Package();
            //            break;
            //        case AdvancedDataBaseEntity.PackageSetup:
            //            entityType = new PackageSetup();
            //            break;
            //        case AdvancedDataBaseEntity.Province:
            //            entityType = new Province();
            //            break;
            //        case AdvancedDataBaseEntity.Report:
            //            entityType = new Report();
            //            break;
            //        case AdvancedDataBaseEntity.Role:
            //            entityType = new Role();
            //            break;
            //        case AdvancedDataBaseEntity.Service:
            //            entityType = new Service();
            //            break;
            //        case AdvancedDataBaseEntity.ServiceProvider:
            //            entityType = new ServiceProvider();
            //            break;
            //        case AdvancedDataBaseEntity.SimCard:
            //            entityType = new SimCard();
            //            break;
            //        case AdvancedDataBaseEntity.Suburb:
            //            entityType = new Suburb();
            //            break;
            //    }
            //    EntityDetails.Add(new EntityDetail(entityName, entityType));
            #endregion

            EntityDetails.Add(new EntityDetail("-- Please Select --", null));
            EntityDetails.Add(new EntityDetail("Audit Log", new AuditLog()));
            EntityDetails.Add(new EntityDetail("Cities", new City()));
            EntityDetails.Add(new EntityDetail("Clients", new Client()));
            EntityDetails.Add(new EntityDetail("Client Billing", new ClientBilling()));
            EntityDetails.Add(new EntityDetail("Client Locations", new ClientLocation()));
            EntityDetails.Add(new EntityDetail("Client Service", new ClientService()));
            EntityDetails.Add(new EntityDetail("Companies", new Company()));
            EntityDetails.Add(new EntityDetail("Company Billing Levels", new CompanyBillingLevel()));
            EntityDetails.Add(new EntityDetail("Company Groups", new CompanyGroup()));
            EntityDetails.Add(new EntityDetail("Contracts", new Contract()));
            EntityDetails.Add(new EntityDetail("Contract Services", new ContractService()));
            EntityDetails.Add(new EntityDetail("Devices", new Device()));
            EntityDetails.Add(new EntityDetail("Devices IME Number", new DeviceIMENumber()));
            EntityDetails.Add(new EntityDetail("Device Makes", new DeviceMake()));
            EntityDetails.Add(new EntityDetail("Device Models", new DeviceModel()));
            EntityDetails.Add(new EntityDetail("Invoices", new Invoice()));
            EntityDetails.Add(new EntityDetail("Invoice Details", new InvoiceDetail()));
            EntityDetails.Add(new EntityDetail("Packages", new Package()));
            EntityDetails.Add(new EntityDetail("Client Package Setup", new PackageSetup()));
            EntityDetails.Add(new EntityDetail("Provinces", new Province()));
            EntityDetails.Add(new EntityDetail("Reports", new Report()));
            EntityDetails.Add(new EntityDetail("Roles", new Role()));
            EntityDetails.Add(new EntityDetail("Services", new Service()));
            EntityDetails.Add(new EntityDetail("Service Providers", new ServiceProvider()));
            EntityDetails.Add(new EntityDetail("Sim Cards", new SimCard()));
            EntityDetails.Add(new EntityDetail("Suburbs", new Suburb()));
        }
    }

    public class EntityDetail
    {
        /// <summary>
        /// The entities name as a string
        /// </summary>
        public string EntityName
        {
            get { return _entityName; }
            set { _entityName = value; }
        }
        private string _entityName;
        /// <summary>
        /// The entities type
        /// </summary>
        public object EntityType
        {
            get { return _entityType; }
            set { _entityType = value; }
        }
        private object _entityType;
        /// <summary>
        /// The columns type
        /// </summary>
        public List<string> ColumnTypes
        {
            get { return _columnName; }
            set { _columnName = value; }
        }
        private List<string> _columnName;

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityDetail(string EntityName, object EntityType)
        {
            this.EntityName = EntityName;
            this.EntityType = EntityType;
            ColumnTypes = new List<string>();
        }
        /// <summary>
        /// Returns the requested entity's column names
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<string> ReadAllColumnNames()
        {
            if (EntityType != null)
            {
                IEnumerable<string> names = EntityType.GetType().GetProperties()
                            .Select(property => property.Name)
                            .ToList();
                
                ObservableCollection<string> observableNames = new ObservableCollection<string>();
                observableNames.Add("-- Please Select --");
                ColumnTypes.Add("None");

                foreach (string name in names)
                {
                    if (!name.StartsWith("pk") && !name.StartsWith("fk") && !name.StartsWith("en") && !name.EndsWith("ID"))
                    {
                        observableNames.Add(name);
                        ColumnTypes.Add (EntityType.GetType().GetProperty(name).PropertyType.Name);
                    }
                }

                return observableNames;
            }
            else
                return null;
        }
    }
}
