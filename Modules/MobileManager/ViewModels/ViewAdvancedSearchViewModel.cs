using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewAdvancedSearchViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private string _defaultItem = "-- Please Select --";
        private string _defaultEnum = "None";

        #region Commands

        public DelegateCommand AddCommand { get; set; }

        #endregion

        #region Properties
        
        /// <summary>
        /// The data entity display name
        /// </summary>
        public string DataEntityDisplayName
        {
            get { return _dataEntityDisplayName; }
            set { SetProperty(ref _dataEntityDisplayName, value); }
        }
        private string _dataEntityDisplayName = string.Empty;


        #region View Lookup Data Collection
        
        /// <summary>
        /// The collection of entity columns for the selected category from the database
        /// </summary>
        public ObservableCollection<AdvancedSearchField> SearchCollumnCollection
        {
            get { return _searchCollumnCollection; }
            set { SetProperty(ref _searchCollumnCollection, value); }
        }
        private ObservableCollection<AdvancedSearchField> _searchCollumnCollection;

        /// <summary>
        /// The collection of operators types from the OperatorType enum's
        /// </summary>
        public ObservableCollection<string> OperatorTypeCollection
        {
            get { return _operatorTypeCollection; }
            set { SetProperty(ref _operatorTypeCollection, value); }
        }
        private ObservableCollection<string> _operatorTypeCollection;

        /// <summary>
        /// The collection of operators from the string, numeric
        /// date, math and validation OperatorType enum's
        /// </summary>
        public ObservableCollection<string> OperatorCollection
        {
            get { return _operatorCollection; }
            set { SetProperty(ref _operatorCollection, value); }
        }
        private ObservableCollection<string> _operatorCollection;

        /// <summary>
        /// Collection of the enum categories to search
        /// </summary>
        public ObservableCollection<string> SearchCategorieCollection
        {
            get { return _searchCategorieCollection; }
            set { SetProperty(ref _searchCategorieCollection, value); ; }
        }
        private ObservableCollection<string> _searchCategorieCollection;

        /// <summary>
        /// Collection of all the advanced search field
        /// </summary>
        public ObservableCollection<AdvancedSearchField> AdvancedSearchFieldCollection
        {
            get { return _advancedSearchFieldCollection; }
            set { SetProperty(ref _advancedSearchFieldCollection, value); }
        }
        private ObservableCollection<AdvancedSearchField> _advancedSearchFieldCollection;

        /// <summary>
        /// Collection of the required fields data
        /// </summary>
        public ObservableCollection<string> ComboBoxValidationCollection
        {
            get { return _comboBoxValidationCollection; }
            set { SetProperty(ref _comboBoxValidationCollection, value); }
        }
        private ObservableCollection<string> _comboBoxValidationCollection;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected validation data entity
        /// </summary>
        public string SelectedSearchCategory
        {
            get { return _selectedSearchCategory; }
            set
            {
                SetProperty(ref _selectedSearchCategory, value);
                if (value != null && value != _defaultItem)
                {
                    //Create Please select to set as default item
                    AdvancedSearchField defaultItem = new AdvancedSearchField();
                    defaultItem.ColumnName = defaultItem.DataType = defaultItem.ColumnName = defaultItem.ControlType = string.Empty;
                    defaultItem.enSearchCategory = 0;
                    defaultItem.DisplayName = _defaultItem;

                    List<AdvancedSearchField> searchColumns = new List<AdvancedSearchField>();
                    searchColumns = AdvancedSearchFieldCollection.Where(p => p.enSearchCategory == EnumHelper.GetEnumFromDescription<SearchCategory>(SelectedSearchCategory).Value()).ToList();
                    searchColumns.Insert(0, defaultItem);
                    SearchCollumnCollection = new ObservableCollection<AdvancedSearchField>(searchColumns);
                }
            }
        }
        private string _selectedSearchCategory = null;

        /// <summary>
        /// The selected validation entity
        /// </summary>
        public string SelectedEntity
        {
            get { return _selectedEntity; }
            set
            {
                SetProperty(ref _selectedEntity, value);
                ReadDataEntities();
            }
        }
        private string _selectedEntity = EnumHelper.GetDescriptionFromEnum(DataValidationGroupName.None);

        public AdvancedSearchField SelectedColumn
        {
            get { return _selectedColumn; }
            set
            {
                SetProperty(ref _selectedColumn, value);
                if (value != null)
                {
                    ShowValueComponent(value.ControlType);
                    SelectedOperatorType = value.DataType != string.Empty ? value.DataType : "None";
                    //if it is a combobox get the collection to show
                    if (value.ControlType == "ComboBox")
                        GetComboBoxCollection(value);
                    ReadTypeOperators();
                }
                //ReadOperatorTypes();
            }
        }
        private AdvancedSearchField _selectedColumn;

        /// <summary>
        /// TextBox value visiblity
        /// </summary>
        public string TextBoxValueShow
        {
            get { return _textBoxValueShow; }
            set { SetProperty(ref _textBoxValueShow, value); }
        }
        private string _textBoxValueShow;

        /// <summary>
        /// ComboBox value visiblity
        /// </summary>
        public string ComboBoxValueShow
        {
            get { return _comboBoxValueShow; }
            set { SetProperty(ref _comboBoxValueShow, value); }
        }
        private string _comboBoxValueShow;

        /// <summary>
        /// Calendar value visiblity
        /// </summary>
        public string CalendarValueShow
        {
            get { return _calendarValueShow; }
            set { SetProperty(ref _calendarValueShow, value); }
        }
        private string _calendarValueShow;

        /// <summary>
        /// Operator type show visibility
        /// </summary>
        public string ShowOperatorType
        {
            get { return _showOperatorType; }
            set { SetProperty(ref _showOperatorType, value); }
        }
        private string _showOperatorType;

        /// <summary>
        /// The selected operator
        /// </summary>
        public string SelectedOperatorType
        {
            get { return _selectedOperatorType; }
            set
            {
                SetProperty(ref _selectedOperatorType, value);
                //ReadTypeOperators();
            }
        }
        private string _selectedOperatorType;

        /// <summary>
        /// The selected data validation property
        /// </summary>
        public DataValidationProperty SelectedDataProperty
        {
            get { return _selectedDataProperty; }
            set
            {
                SetProperty(ref _selectedDataProperty, value);
                //ReadOperatorTypes();
            }
        }
        private DataValidationProperty _selectedDataProperty = null;

        /// <summary>
        /// The selected operator
        /// </summary>
        public string SelectedOperator
        {
            get { return _selectedOperator; }
            set { SetProperty(ref _selectedOperator, value); }
        }
        private string _selectedOperator;

        /// <summary>
        /// Textbox validation test value
        /// </summary>
        public string TextBoxValidationValue
        {
            get { return _textBoxValidationValue; }
            set { SetProperty(ref _textBoxValidationValue, value); }
        }
        private string _textBoxValidationValue;

        /// <summary>
        /// Combobox validation test value
        /// </summary>
        public string ComboBoxValidationValue
        {
            get { return _comboBoxValidationValue; }
            set { SetProperty(ref _comboBoxValidationValue, value); }
        }
        private string _comboBoxValidationValue;

        /// <summary>
        /// Calendar validation test value
        /// </summary>
        public DateTime CalendarValidationValue
        {
            get { return _calendarValidationValue; }
            set { SetProperty(ref _calendarValidationValue, value); }
        }
        private DateTime _calendarValidationValue;

        /// <summary>
        /// 
        /// </summary>
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataEntity
        {
            get { return _validDataEntity; }
            set { SetProperty(ref _validDataEntity, value); }
        }
        private Brush _validDataEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidColumnName
        {
            get { return _validColumnName; }
            set { SetProperty(ref _validColumnName, value); }
        }
        private Brush _validColumnName = Brushes.Red;

        /// <summary>
        /// Set the operator combobox border colour
        /// </summary>
        public Brush ValidOperator
        {
            get { return _validOperator; }
            set { SetProperty(ref _validOperator, value); }
        }
        private Brush _validOperator = Brushes.Red;

        /// <summary>
        /// Set the validation grid border colour
        /// </summary>
        public Brush ValidValidation
        {
            get { return _validValidation; }
            set { SetProperty(ref _validValidation, value); }
        }
        private Brush _validValidation = Brushes.Red;


        /// <summary>
        /// Input validation properties
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                switch (columnName)
                {
                    case "SelectedSearchCategory":
                        ValidDataEntity = SelectedSearchCategory != null && SelectedSearchCategory.ToString() != _defaultItem ? Brushes.Silver : Brushes.Red; break;
                    case "SelectedDataProperty":
                        ValidColumnName = SelectedDataProperty != null && SelectedDataProperty.ToString() != _defaultItem ? Brushes.Silver : Brushes.Red; break;
                    case "SelectedColumn":
                        ValidColumnName = SelectedColumn != null && SelectedColumn.DisplayName != _defaultItem ? Brushes.Silver : Brushes.Red; break;
                    case "SelectedOperator":
                        ValidOperator = string.IsNullOrEmpty(SelectedOperator) || SelectedOperator == _defaultEnum ? Brushes.Red : Brushes.Silver; break;
                    case "TextBoxValidationValue":
                        ValidValidation = TextBoxValueShow == "Visible" && string.IsNullOrEmpty(TextBoxValidationValue) ? Brushes.Red : Brushes.Silver; break;
                    case "ComboBoxValidationValue":
                        ValidValidation = ComboBoxValueShow == "Visible" && ComboBoxValidationValue != null && ComboBoxValidationValue == _defaultItem ? Brushes.Red : Brushes.Silver; break;
                    case "CalendarValidationValue":
                        ValidValidation = CalendarValueShow == "Visible" && CalendarValidationValue == DateTime.MinValue ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public ViewAdvancedSearchViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseViewControls();
            _securityHelper = new SecurityHelper(eventAggregator);
            ReadDataEntities();
            InitialiseValidationCFView();
        }

        /// <summary>
        /// Reset the form to a start state
        /// </summary>
        public void InitialiseViewControls()
        {
            ReadSearchCategories();
            ReadSearchColumnCollection();
            TextBoxValueShow = ComboBoxValueShow = CalendarValueShow = "Hidden";
            ShowOperatorType = "Visible";
            //Set the selected SearchCategory
            SelectedSearchCategory = SearchCategorieCollection.Where(x => x == _defaultItem).FirstOrDefault();

            SelectedColumn = null;
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseValidationCFView()
        {
            //Test if rule can be added
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => ValidDataEntity)
                                                                       .ObservesProperty(() => ValidColumnName)
                                                                       .ObservesProperty(() => ValidOperator)
                                                                       .ObservesProperty(() => ValidValidation);
        }

        /// <summary>
        /// Set the required component visible
        /// </summary>
        /// <param name="Component"></param>
        public void ShowValueComponent(string Component)
        {
            switch (Component)
            {
                case "TextBox":
                    TextBoxValueShow = ShowOperatorType = "Visible";
                    TextBoxValidationValue = string.Empty;
                    ComboBoxValueShow = CalendarValueShow = "Hidden";
                    break;
                case "ComboBox":
                    ComboBoxValueShow = "Visible";
                    ComboBoxValidationValue = _defaultItem;
                    TextBoxValueShow = CalendarValueShow = ShowOperatorType = "Hidden";
                    break;
                case "Calendar":
                    CalendarValueShow = ShowOperatorType = "Visible";
                    CalendarValidationValue = DateTime.Now;
                    ComboBoxValueShow = TextBoxValueShow = "Hidden";
                    break;
                default:
                    TextBoxValueShow = ComboBoxValueShow = CalendarValueShow = "Hidden";
                    break;
            }
        }

        /// <summary>
        /// All the combobox values with yes or no options
        /// </summary>
        /// <param name="collection"></param>
        public ObservableCollection<string> GetBooleanValues()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();
            collection.Add(_defaultItem);
            collection.Add("True");
            collection.Add("False");
            return collection;
        }

        /// <summary>
        /// Returs a boolean if mapping was successful
        /// Mapping is accessed in the ref path list
        /// </summary>
        /// <param name="allMappings"></param>
        /// <param name="mappingToSearch"></param>
        /// <param name="searchingFor"></param>
        /// <param name="path"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool GetMappingPath(List<Mapping> allMappings, string mappingToSearch, string searchingFor, ref List<string> path, ref List<int> keys)
        {
            Mapping map = allMappings.Where(p => p.Me == mappingToSearch).FirstOrDefault();
            List<string> newEntities = new List<string>();

            foreach (AdvancedSearchMap advancedSearchMap in map.Connections)
            {
                string otherEntity = advancedSearchMap.FromEntity == mappingToSearch ? advancedSearchMap.ToEntity : advancedSearchMap.FromEntity;
                if (otherEntity == searchingFor)
                {
                    path.Add(otherEntity);
                    return true;
                }
                else
                {
                    if (!keys.Any(p => p == advancedSearchMap.pkAdvancedSearchMapID))
                    {
                        newEntities.Add(otherEntity);
                        keys.Add(advancedSearchMap.pkAdvancedSearchMapID);
                        path.Add(otherEntity);
                    }
                }

                foreach (string newMapping in newEntities)
                {
                    if (GetMappingPath(allMappings, newMapping, searchingFor, ref path, ref keys))
                        return true;
                    if (newMapping == otherEntity)
                        path.Remove(otherEntity);
                }

            }
            return false;
        }

        #region Lookup Data Loading

        /// <summary>
        /// Computes the mapping for the requested tables
        /// </summary>
        /// <param name="Entity1"></param>
        /// <param name="Column1"></param>
        /// <param name="Entity2"></param>
        /// <param name="Column2"></param>
        public void CalculateMapping(string Entity1, string Column1, string Entity2, string Column2)
        {
            
            if (Column1.StartsWith("fk"))
            {
                IEnumerable<AdvancedSearchMap> advancedSearchMapping = new AdvancedSearchMapModel(_eventAggregator).ReadAdvancedSearchMappings();
                List<string> allEntities = new AdvancedSearchFieldModel(_eventAggregator).DistinctEntityName();
                List<Mapping> allMappings = new List<Mapping>();

                foreach (string entity in allEntities)
                {
                    Mapping mapping = new Mapping();
                    mapping.Me = entity;

                    foreach (AdvancedSearchMap advancedSearchMap in advancedSearchMapping)
                    {
                        if (advancedSearchMap.FromEntity == entity || advancedSearchMap.ToEntity == entity)
                        {
                            mapping.Connections.Add(advancedSearchMap);
                        }
                    }
                    allMappings.Add(mapping);
                }

                List<string> path = new List<string>();
                Mapping map = allMappings.Where(p => p.Me == Entity1).FirstOrDefault();
                List<int> keys = new List<int>();
                path.Add(Entity1);
                bool found = GetMappingPath(allMappings, Entity1, Entity2, ref path, ref keys);

            }
            else
            { }
        }

        /// <summary>
        /// Read all the advanced search entity names from the enum class
        /// </summary>
        public void ReadDataEntities()
        {
            IEnumerable<string> enumEntities = Enum.GetValues(typeof(AdvancedDataBaseEntity)).Cast<AdvancedDataBaseEntity>().Select(v => v.ToString()).ToList();
            string[] names = Enum.GetNames(typeof(AdvancedDataBaseEntity)).ToArray<string>();
            ObservableCollection<string> entities = new ObservableCollection<string>();

            foreach (string entity in enumEntities)
            {
                entities.Add(entity);
            }

            //DataEntityCollection = entities;
        }

        /// <summary>
        /// Load all the data entities for the selected group from the database
        /// </summary>
        private void ReadEntityColumnNames()
        {
            try
            {
                DataEntityDisplayName = "Audit Log";
                //DataEntityCollection = new ObservableCollection<string>(new AuditLogModel(_eventAggregator).ReadColumnNames());

                switch (EnumHelper.GetEnumFromDescription<AdvancedDataBaseEntity>(SelectedEntity))
                {
                    case AdvancedDataBaseEntity.AuditLog:
                        DataEntityDisplayName = "Audit Log";
                        //DataEntityCollection = new ObservableCollection<string>(new AuditLogModel(_eventAggregator).ReadColumnNames());
                        break;
                }

                //SelectedDataEntity = _defaultItem;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataEntitiesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Populate the package types combobox from the PackageType enum
        /// </summary>
        private void ReadSearchCategories()
        {
            SearchCategorieCollection = new ObservableCollection<string>();

            foreach (SearchCategory source in Enum.GetValues(typeof(SearchCategory)))
            {
                SearchCategorieCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
            }
        }

        /// <summary>
        /// Populate AdvancedSearchFieldCollection with all the rows
        /// </summary>
        private async void ReadSearchColumnCollection()
        {
            try
            {
                AdvancedSearchFieldCollection = await Task.Run(() => new AdvancedSearchFieldModel(_eventAggregator).ReadAdvancedSearchColumns());
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

        /// <summary>
        /// Load all the data type operators from the string, 
        /// numeric and date OperatorType enum's
        /// </summary>
        private void ReadTypeOperators()
        {
            try
            {
                OperatorCollection = new ObservableCollection<string>();

                if (SelectedOperatorType != null && SelectedOperatorType != _defaultEnum)
                {
                    switch (EnumHelper.GetEnumFromDescription<OperatorType>(SelectedOperatorType))
                    {
                        case OperatorType.StringOperator:
                            foreach (StringOperator source in Enum.GetValues(typeof(StringOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.NumericOperator:
                            foreach (NumericOperator source in Enum.GetValues(typeof(NumericOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.DateOperator:
                            foreach (DateOperator source in Enum.GetValues(typeof(DateOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.BooleanOperator:
                            foreach (BooleanOperator source in Enum.GetValues(typeof(BooleanOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case OperatorType.RuleOperator:
                            foreach (RuleOperator source in Enum.GetValues(typeof(RuleOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                    }

                    SelectedOperator = _defaultEnum;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadTypeOperators",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Get the Validationvalue combobox collection
        /// </summary>
        /// <param name="SearchFor"></param>
        public async void GetComboBoxCollection(AdvancedSearchField Row)
        {
            string entityName = Row.EntityName;
            string displayName = Row.DisplayName;

            try
            {
                switch (entityName)
                {
                    case "Client":
                        switch (displayName)
                        {
                            case "Site":
                                ComboBoxValidationCollection = await Task.Run(() => new ClientLocationModel(_eventAggregator).ReadClientLocationNames(true));
                                break;
                            case "Private OR Company":
                                ComboBoxValidationCollection = await Task.Run(() => new CompanyModel(_eventAggregator).ReadCompanieNames(true));
                                break;
                            case "State":
                                ObservableCollection<string> activeInActive = new ObservableCollection<string>();
                                activeInActive.Add(_defaultItem);
                                activeInActive.Add("Active");
                                activeInActive.Add("InActive");
                                ComboBoxValidationCollection = activeInActive;
                                break;
                            case "Suburb":
                                ComboBoxValidationCollection = await Task.Run(() => new SuburbModel(_eventAggregator).ReadSuburbeNames(true));
                                break;
                        }
                        break;
                    case "Suburb":
                        switch (displayName)
                        {
                            case "City":
                                ComboBoxValidationCollection = await Task.Run(() => new CityModel(_eventAggregator).ReadCityNames(false));
                                break;
                        }
                        break;
                    case "City":
                        switch (displayName)
                        {
                            case "Province":
                                ComboBoxValidationCollection = await Task.Run(() => new ProvinceModel(_eventAggregator).ReadProvinceNames(false));
                                break;
                        }
                        break;
                    case "Contract":
                        switch (displayName)
                        {
                            case "Package":
                                ComboBoxValidationCollection = await Task.Run(() => new PackageModel(_eventAggregator).ReadPackageNames(false));
                                break;
                            case "Status":
                                ComboBoxValidationCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuseOptions(StatusLink.Contract));
                                break;
                        }
                        break;
                    case "Package":
                        switch (displayName)
                        {
                            case "Provider":
                                ComboBoxValidationCollection = await Task.Run(() => new ServiceProviderModel(_eventAggregator).ReadProviderNames());
                                break;
                        }
                        break;
                    case "Enum":
                        switch (displayName)
                        {
                            case "Cost Type":
                                ObservableCollection<string> costTypes = new ObservableCollection<string>();
                                costTypes.Add(_defaultItem);
                                foreach (CostType source in Enum.GetValues(typeof(CostType)))
                                {
                                    costTypes.Add(source.ToString());
                                }
                                ComboBoxValidationCollection = costTypes;
                                break;
                            case "Package Type":
                                ObservableCollection<string> packageTypes = new ObservableCollection<string>();
                                packageTypes.Add(_defaultItem);
                                foreach (PackageType source in Enum.GetValues(typeof(PackageType)))
                                {
                                    packageTypes.Add(source.ToString());
                                }
                                ComboBoxValidationCollection = packageTypes;
                                break;
                        }
                        break;
                    case "ClientService":
                        switch (displayName)
                        {
                            case "Service":
                                ComboBoxValidationCollection = await Task.Run(() => new ContractServiceModel(_eventAggregator).ReadContractService());
                                break;
                        }
                        break;
                    case "ClientBilling":
                        switch (displayName)
                        {
                            case "Split Billing":
                                ComboBoxValidationCollection = GetBooleanValues();
                                break;
                            case "Split Billing exception":
                                ComboBoxValidationCollection = GetBooleanValues();
                                break;
                            case "Billing Level":
                                ComboBoxValidationCollection = new BillingLevelModel(_eventAggregator).ReadBillingLevelNames();
                                break;
                            case "International Dailing":
                                ComboBoxValidationCollection = GetBooleanValues();
                                break;
                            case "International Dailing Permanent":
                                ComboBoxValidationCollection = GetBooleanValues();
                                break;
                            case "International Roaming":
                                ComboBoxValidationCollection = GetBooleanValues();
                                break;
                            case "International Roaming Permanent":
                                ComboBoxValidationCollection = GetBooleanValues();
                                break;
                        }
                        break;
                    case "Device":
                        switch (displayName)
                        {
                            case "Make":
                                ComboBoxValidationCollection = await Task.Run(() => new DevicesMakeModel(_eventAggregator).ReadDeviceMakesNames());
                                break;
                            case "Model":
                                ComboBoxValidationCollection = await Task.Run(() => new DevicesModelModel(_eventAggregator).ReadDeviceMakeModelNames());
                                break;
                            case "Status":
                                ComboBoxValidationCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuseOptions(StatusLink.Device));
                                break;
                            case "Insured":
                                ComboBoxValidationCollection = GetBooleanValues();
                                break;
                        }
                        break;
                    case "SimCard":
                        switch (displayName)
                        {
                            case "Status":
                                ComboBoxValidationCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuseOptions(StatusLink.Sim));
                                break;
                        }
                        break;
                    default:
                        ComboBoxValidationCollection = null;
                        break;
                }
                ComboBoxValidationValue = ComboBoxValidationCollection != null ? ComboBoxValidationCollection.Where(p => p == _defaultItem).FirstOrDefault() : null;
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

        #endregion

        #region Command Execution

        /// <summary>
        /// Set add command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteAdd()
        {
            bool result = true;
            result =  _securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || _securityHelper.IsUserInRole(SecurityRole.DataManager.Value());

            if (result)
            {
                result = ValidDataEntity == Brushes.Silver && ValidColumnName == Brushes.Silver && ValidValidation == Brushes.Silver ? true : false;
            }

            if (result)
            {
                result = ShowOperatorType == "Visible" && ValidOperator == Brushes.Silver || ShowOperatorType == "Hidden" ? true : false;
            }

            return result;
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// A class to keep data on the mapping
    /// to compute till path is found
    /// </summary>
    public class Mapping
    {
        /// <summary>
        /// The AdvancedSearch object name
        /// </summary>
        public string Me
        {
            get { return _me; }
            set { _me = value; }
        }
        private string _me;

        /// <summary>
        /// All the joins to me
        /// </summary>
        public List<AdvancedSearchMap> Connections
        {
            get { return _connections; }
            set { _connections = value; }
        }
        private List<AdvancedSearchMap> _connections;

        /// <summary>
        /// Constructor
        /// </summary>
        public Mapping()
        {
            Connections = new List<AdvancedSearchMap>();
        }
    }
}
