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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataValidationCFViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private DataValidationProcess _dataValidationProcess = DataValidationProcess.System;
        private DataValidationGroupName _dataValidationGroup = DataValidationGroupName.None;
        private string _defaultItem = "-- Please Select --";
        private string _defaultEnum = "None";

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) validation rule entity
        /// </summary>
        public Visibility InternalValidation
        {
            get { return _internalValidation; }
            set { SetProperty(ref _internalValidation, value); }
        }
        private Visibility _internalValidation = Visibility.Visible;

        /// <summary>
        /// Holds the selected (current) validation rule entity
        /// </summary>
        public Visibility ExternalValidation
        {
            get { return _externalValidation; }
            set { SetProperty(ref _externalValidation, value); }
        }
        private Visibility _externalValidation = Visibility.Collapsed;

        /// <summary>
        /// Holds the selected (current) validation rule entity
        /// </summary>
        public DataValidationRule SelectedValidationRule
        {
            get { return _selectedValidationRule; }
            set
            {
                SetProperty(ref _selectedValidationRule, value);

                if (value != null && value.enDataValidationGroupName > 0)
                {
                    switch (_dataValidationGroup)
                    {
                        case DataValidationGroupName.Client:
                            DataEntityDisplayName = "PrimaryCellNumber";
                            foreach (Client entity in DataEntityCollection)
                            {
                                if (entity.pkClientID == value.DataValidationEntityID)
                                {
                                    SelectedDataEntity = entity;
                                    break;
                                }
                            }
                            break;
                        case DataValidationGroupName.Company:
                        case DataValidationGroupName.CompanyClient:
                            DataEntityDisplayName = "CompanyName";
                            foreach (Company entity in DataEntityCollection)
                            {
                                if (entity.pkCompanyID == value.DataValidationEntityID)
                                {
                                    SelectedDataEntity = entity;
                                    break;
                                }
                            }
                            break;
                        case DataValidationGroupName.Device:
                        case DataValidationGroupName.SimCard:
                            ValidDataEntity = Brushes.Silver; break;
                        case DataValidationGroupName.ExternalData:
                            DataEntityDisplayName = "TableName";
                            foreach (ExternalBillingData entity in DataEntityCollection)
                            {
                                if (entity.pkExternalBillingDataID == value.DataValidationEntityID)
                                {
                                    SelectedDataEntity = entity;
                                    break;
                                }
                            }
                            break;
                    }

                    if (_dataValidationGroup == DataValidationGroupName.ExternalData)
                        SelectedDataProperty = DataPropertyCollection.Where(p => p.ExtDataValidationProperty == value.PropertyDescription).FirstOrDefault();
                    else
                        SelectedDataProperty = DataPropertyCollection != null ? DataPropertyCollection.First(p => p.pkDataValidationPropertyID == value.fkDataValidationPropertyID) :
                                               DataPropertyCollection != null ? DataPropertyCollection.First(p => p.pkDataValidationPropertyID == 0) : null;
                    SelectedOperatorType = value.OperatorTypeDescription;
                    SelectedOperator = value.OperatorDescription;
                    SelectedValidationValue = value.DataValidationValue;
                }
            }
        }
        private DataValidationRule _selectedValidationRule = new DataValidationRule();

        /// <summary>
        /// The data entity display name based on the selected group
        /// </summary>
        public string DataEntityDisplayName
        {
            get { return _dataEntityDisplayName; }
            set { SetProperty(ref _dataEntityDisplayName, value); }
        }
        private string _dataEntityDisplayName = string.Empty;

        /// <summary>
        /// The data entity display name based on the selected group
        /// </summary>
        public string DataPropertyDisplayName
        {
            get { return _dataPropertyDisplayName; }
            set { SetProperty(ref _dataPropertyDisplayName, value); }
        }
        private string _dataPropertyDisplayName = string.Empty;

        /// <summary>
        /// The data import progessbar description
        /// </summary>
        public string ImportUpdateDescription
        {
            get { return _importUpdateDescription; }
            set { SetProperty(ref _importUpdateDescription, value); }
        }
        private string _importUpdateDescription;

        /// <summary>
        /// The data import progessbar value
        /// </summary>
        public int ImportUpdateProgress
        {
            get { return _ImportUpdateProgress; }
            set { SetProperty(ref _ImportUpdateProgress, value); }
        }
        private int _ImportUpdateProgress;

        /// <summary>
        /// The number of import data items
        /// </summary>
        public int ImportUpdateCount
        {
            get { return _importUpdateCount; }
            set { SetProperty(ref _importUpdateCount, value); }
        }
        private int _importUpdateCount;

        /// <summary>
        /// The number of importd data that passed validation
        /// </summary>
        public int ImportUpdatesPassed
        {
            get { return _importUpdatesPassed; }
            set { SetProperty(ref _importUpdatesPassed, value); }
        }
        private int _importUpdatesPassed;

        /// <summary>
        /// The number of importd data that failed
        /// </summary>
        public int ImportUpdatesFailed
        {
            get { return _importUpdatesFailed; }
            set { SetProperty(ref _importUpdatesFailed, value); }
        }
        private int _importUpdatesFailed;

        /// <summary>
        /// The collection of imported excel data
        /// </summary>
        public DataTable ImportedDataCollection
        {
            get { return _importDataCollection; }
            set { SetProperty(ref _importDataCollection, value); }
        }
        private DataTable _importDataCollection = null;

        /// <summary>
        /// The collection of data import exceptions
        /// </summary>
        public ObservableCollection<string> ExceptionsCollection
        {
            get { return _exceptionsCollection; }
            set { SetProperty(ref _exceptionsCollection, value); }
        }
        private ObservableCollection<string> _exceptionsCollection = null;

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<DataValidationRule> ValidationRuleCollection
        {
            get { return _validationRuleCollection; }
            set { SetProperty(ref _validationRuleCollection, value); }
        }
        private ObservableCollection<DataValidationRule> _validationRuleCollection;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of data validation processes from
        /// the DataValidationProcess enum
        /// </summary>
        public ObservableCollection<string> ProcessCollection
        {
            get { return _processCollection; }
            set { SetProperty(ref _processCollection, value); }
        }
        private ObservableCollection<string> _processCollection;

        /// <summary>
        /// The collection of validation group entities from the DataValidationGroupName enum
        /// </summary>
        public ObservableCollection<string> EntityGroupCollection
        {
            get { return _entityGroupCollection; }
            set { SetProperty(ref _entityGroupCollection, value); }
        }
        private ObservableCollection<string> _entityGroupCollection;

        /// <summary>
        /// The collection of data entities for the selected group from the database
        /// </summary>
        public ObservableCollection<object> DataEntityCollection
        {
            get { return _dataEntityCollection; }
            set { SetProperty(ref _dataEntityCollection, value); } 
        }
        private ObservableCollection<object> _dataEntityCollection;

        /// <summary>
        /// The collection of company entities from the database
        /// </summary>
        public ObservableCollection<Company> CompanyCollection
        {
            get { return _companyCollection; }
            set { SetProperty(ref _companyCollection, value); }
        }
        private ObservableCollection<Company> _companyCollection;

        /// <summary>
        /// The collection of client entities from the database
        /// </summary>
        public ObservableCollection<Client> ClientCollection
        {
            get { return _clientCollection; }
            set { SetProperty(ref _clientCollection, value); }
        }
        private ObservableCollection<Client> _clientCollection;

        /// <summary>
        /// The collection of entity properties from the 
        /// DataValidationPropertyName enum linked to the
        /// configuration in the database
        /// </summary>
        public ObservableCollection<DataValidationProperty> DataPropertyCollection
        {
            get { return _dataPropertyCollection; }
            set { SetProperty(ref _dataPropertyCollection, value); }
        }
        private ObservableCollection<DataValidationProperty> _dataPropertyCollection;

        /// <summary>
        /// The collection of external DataPropertyCollection from the database
        /// </summary>
        public Dictionary<string, object> ExtDataPropertyCollection
        {
            get { return _extDataPropertyCollection; }
            set { SetProperty(ref _extDataPropertyCollection, value); }
        }
        private Dictionary<string, object> _extDataPropertyCollection = null;

        /// <summary>
        /// The collection of the selected external DataPropertyCollection
        /// </summary>
        public Dictionary<string, object> SelectedExtDataPropertyCollection
        {
            get { return _selectedExtDataPropertyCollection; }
            set { SetProperty(ref _selectedExtDataPropertyCollection, value); }
        }
        private Dictionary<string, object> _selectedExtDataPropertyCollection = null;

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

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected data validation process
        /// </summary>
        public string SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
                SetProperty(ref _selectedProcess, value);
                InitialiseViewControls();
                EntityGroupCollection = null;
                _dataValidationProcess = EnumHelper.GetEnumFromDescription<DataValidationProcess>(value);

                if (_dataValidationProcess != DataValidationProcess.None)
                    ReadDataValidationGroups();

                if (_dataValidationProcess == DataValidationProcess.ExternalBilling)
                {
                    InternalValidation = Visibility.Collapsed;
                    ExternalValidation = Visibility.Visible;
                }
                else
                {
                    InternalValidation = Visibility.Visible;
                    ExternalValidation = Visibility.Collapsed;
                }
            }
        }
        private string _selectedProcess = EnumHelper.GetDescriptionFromEnum(DataValidationProcess.System);

        /// <summary>
        /// The selected validation entity group
        /// </summary>
        public string SelectedEntityGroup
        {
            get { return _selectedEntityGroup; }
            set
            {
                SetProperty(ref _selectedEntityGroup, value);
                InitialiseViewControls();
                DataEntityCollection = null;
                OperatorCollection = null;

                if (value != null)
                {
                    _dataValidationGroup = EnumHelper.GetEnumFromDescription<DataValidationGroupName>(value);
                    DataValidationProcess selectedProcess = SelectedProcess != null ?
                                        EnumHelper.GetEnumFromDescription<DataValidationProcess>(SelectedProcess) :
                                        DataValidationProcess.None;

                    if (_dataValidationGroup != DataValidationGroupName.None)
                    {
                        ReadDataEntities();
                        Task.Run(() => ReadDataValidationRulesAsync());
                    }
                }
            }
        }
        private string _selectedEntityGroup = EnumHelper.GetDescriptionFromEnum(DataValidationGroupName.None);

        /// <summary>
        /// The selected validation data entity
        /// </summary>
        public object SelectedDataEntity
        {
            get { return _selectedDataEntity; }
            set
            {
                SetProperty(ref _selectedDataEntity, value);
                if (value != null)
                {
                    OperatorCollection = null;
                    ReadEnityProperties();
                }
            }
        }
        private object _selectedDataEntity = null;

        /// <summary>
        /// The selected data validation property
        /// </summary>
        public DataValidationProperty SelectedDataProperty
        {
            get { return _selectedDataProperty; }
            set
            {
                SetProperty(ref _selectedDataProperty, value);
                ReadOperatorTypes();
            }
        }
        private DataValidationProperty _selectedDataProperty = null;

        /// <summary>
        /// The selected operator
        /// </summary>
        public string SelectedOperatorType
        {
            get { return _selectedOperatorType; }
            set
            {
                SetProperty(ref _selectedOperatorType, value);
                ReadTypeOperators();
            }
        }
        private string _selectedOperatorType;

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
        /// The entered validation value
        /// </summary>
        public string SelectedValidationValue
        {
            get { return _selectedValidationValue; }
            set { SetProperty(ref _selectedValidationValue, value); }
        }
        private string _selectedValidationValue = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidEntityGroup
        {
            get { return _validEntityGroup; }
            set { SetProperty(ref _validEntityGroup, value); }
        }
        private Brush _validEntityGroup = Brushes.Red;

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
        public Brush ValidDataProperty
        {
            get { return _validDataProperty; }
            set { SetProperty(ref _validDataProperty, value); }
        }
        private Brush _validDataProperty = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidOperatorType
        {
            get { return _validOperatorType; }
            set { SetProperty(ref _validOperatorType, value); }
        }
        private Brush _validOperatorType = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidOperator
        {
            get { return _validOperator; }
            set { SetProperty(ref _validOperator, value); }
        }
        private Brush _validOperator = Brushes.Red;

        /// <summary>
        /// Input validate error message
        /// </summary>
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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
                    case "SelectedEntityGroup":
                        ValidEntityGroup = string.IsNullOrEmpty(SelectedEntityGroup) || SelectedEntityGroup == _defaultItem ? Brushes.Red : Brushes.Silver;
                        //if (ValidEntityGroup == Brushes.Silver)
                        //    switch (EnumHelper.GetEnumFromDescription<DataValidationGroupName>(SelectedEntityGroup))
                        //    {
                        //        case DataValidationGroupName.Device:
                        //        case DataValidationGroupName.SimCard:
                        //            ValidDataEntity = Brushes.Silver; break;
                        //    }
                        break;
                    case "SelectedDataEntity":
                        if (SelectedEntityGroup != null && SelectedEntityGroup != _defaultItem &&
                            SelectedDataEntity != null && SelectedDataEntity.ToString() != _defaultItem)
                            switch (EnumHelper.GetEnumFromDescription<DataValidationGroupName>(SelectedEntityGroup))
                            {
                                case DataValidationGroupName.Client:
                                    ValidDataEntity = SelectedDataEntity != null && ((Client)SelectedDataEntity).pkClientID > 0 ? Brushes.Silver : Brushes.Red; break;
                                case DataValidationGroupName.CompanyClient:
                                case DataValidationGroupName.Company:
                                    ValidDataEntity = SelectedDataEntity != null && ((Company)SelectedDataEntity).pkCompanyID > 0 ? Brushes.Silver : Brushes.Red; break;
                                case DataValidationGroupName.ExternalData:
                                    ValidDataEntity = SelectedDataEntity != null && ((ExternalBillingData)SelectedDataEntity).pkExternalBillingDataID > 0 ? Brushes.Silver : Brushes.Red; break;
                            }
                        else
                            ValidDataEntity = Brushes.Red;
                        break;
                    case "SelectedDataProperty":
                        if (_dataValidationGroup == DataValidationGroupName.ExternalData)
                            ValidDataProperty = SelectedDataProperty != null && SelectedDataProperty.ToString() != _defaultItem ? Brushes.Silver : Brushes.Red;
                        else
                            ValidDataProperty = SelectedDataProperty != null && SelectedDataProperty.ToString() != _defaultItem && 
                                                ((DataValidationProperty)SelectedDataProperty).pkDataValidationPropertyID > 0 ? Brushes.Silver : Brushes.Red;
                        break;
                    case "SelectedOperatorType":
                        ValidOperatorType = string.IsNullOrEmpty(SelectedOperatorType) || SelectedOperatorType == _defaultEnum ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedOperator":
                        ValidOperator = string.IsNullOrEmpty(SelectedOperator) || SelectedOperator == _defaultEnum ? Brushes.Red : Brushes.Silver; break;
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
        public ViewDataValidationCFViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseValidationCFView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseValidationCFView()
        {
            _model = new DataValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedEntityGroup);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd);
            DeleteCommand = new DelegateCommand(ExecuteDelete, CanExecuteDelete).ObservesProperty(() => SelectedValidationRule);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedEntityGroup)
                                                                          .ObservesProperty(() => SelectedDataEntity)
                                                                          .ObservesProperty(() => ValidDataProperty)
                                                                          .ObservesProperty(() => ValidOperatorType)
                                                                          .ObservesProperty(() => ValidOperator);

            // Load the view data
            ReadDataValidationProcesses();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedDataEntity = null;
            SelectedDataProperty = null;
            SelectedOperator = null;
            SelectedValidationValue = string.Empty;
            ValidationRuleCollection = null;
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        private async Task ReadDataValidationRulesAsync()
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadDataValidationRules(_dataValidationProcess, _dataValidationGroup));
                Application.Current.Dispatcher.Invoke(() => { SelectedValidationRule = null; });
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationRulesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the data validation processes from the  
        /// DataValidationProcess enum's
        /// </summary>
        private void ReadDataValidationProcesses()
        {
            try
            {
                ProcessCollection = new ObservableCollection<string>();

                foreach (DataValidationProcess process in Enum.GetValues(typeof(DataValidationProcess)))
                {
                    ProcessCollection.Add(EnumHelper.GetDescriptionFromEnum(process));
                }

                SelectedProcess = _defaultItem;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationProcesses",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the validation entity groups from the DataValidationGroupName enum
        /// </summary>
        private void ReadDataValidationGroups()
        {
            try
            {
                EntityGroupCollection = new ObservableCollection<string>();
                DataValidationProcess selectedProcess = SelectedProcess != null ?
                                                        EnumHelper.GetEnumFromDescription<DataValidationProcess>(SelectedProcess) :
                                                        DataValidationProcess.None;

                foreach (DataValidationGroupName source in Enum.GetValues(typeof(DataValidationGroupName)))
                {
                    if (selectedProcess == DataValidationProcess.ExternalBilling &&
                        (source == DataValidationGroupName.None || source == DataValidationGroupName.ExternalData))
                        EntityGroupCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                    else if (selectedProcess != DataValidationProcess.ExternalBilling && source != DataValidationGroupName.ExternalData)
                        EntityGroupCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                }

                SelectedEntityGroup = _defaultItem;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationGroups",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the operator types from the OperatorType enum
        /// </summary>
        private void ReadOperatorTypes()
        {
            try
            {
                OperatorTypeCollection = new ObservableCollection<string>();

                if (SelectedDataProperty != null && SelectedDataProperty.pkDataValidationPropertyID > 0)
                {
                    switch ((DataTypeName)SelectedDataProperty.enDataType)
                    {
                        case DataTypeName.Integer:
                        case DataTypeName.Decimal:
                        case DataTypeName.Float:
                        case DataTypeName.Long:
                        case DataTypeName.Short:
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.None));
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.NumericOperator));
                            break;
                        case DataTypeName.DateTime:
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.None));
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.DateOperator));
                            break;
                        case DataTypeName.Bool:
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.None));
                            OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(OperatorType.BooleanOperator));
                            break;
                        default:
                            foreach (OperatorType source in Enum.GetValues(typeof(OperatorType)))
                            {
                                OperatorTypeCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                            }
                            break;
                    }
                }

                SelectedOperatorType = _defaultEnum;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadOperatorTypes",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the data entities for the selected group from the database
        /// </summary>
        private void ReadDataEntities()
        {
            try
            {
                ClientCollection = null;
                CompanyCollection = null;

                switch (EnumHelper.GetEnumFromDescription<DataValidationGroupName>(SelectedEntityGroup))
                {
                    case DataValidationGroupName.Client:
                        DataEntityDisplayName = "PrimaryCellNumber";
                        DataEntityCollection = new ObservableCollection<object>(new ClientModel(_eventAggregator).ReadClients(true));
                        break;
                    case DataValidationGroupName.Company:
                    case DataValidationGroupName.CompanyClient:
                        DataEntityDisplayName = "CompanyName";
                        DataEntityCollection = new ObservableCollection<object>(new CompanyModel(_eventAggregator).ReadCompanies(true, false));
                        break;
                    case DataValidationGroupName.Package:
                    case DataValidationGroupName.PackageClient:
                        DataEntityDisplayName = "PackageName";
                        DataEntityCollection = new ObservableCollection<object>(new PackageModel(_eventAggregator).ReadPackages(true, false));
                        break;
                    case DataValidationGroupName.StatusClient:
                        DataEntityDisplayName = "StatusDescription";
                        DataEntityCollection = new ObservableCollection<object>(new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Contract, true, false));
                        break;
                    case DataValidationGroupName.SimCard:
                    case DataValidationGroupName.Device:
                        ValidDataEntity = Brushes.Silver;
                        SelectedDataEntity = 0;
                        break;
                    case DataValidationGroupName.ExternalData:
                        DataEntityDisplayName = "TableName";
                        DataEntityCollection = new ObservableCollection<object>(new ExternalBillingDataModel(_eventAggregator).ReadExternalData());
                        break;
                }

                SelectedDataEntity = _defaultItem;
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
        /// Load all the properties for the selected entity
        /// </summary>
        private void ReadEnityProperties()
        {
            try
            {
                if (SelectedEntityGroup != null)
                {
                    OperatorCollection = null;

                    switch (EnumHelper.GetEnumFromDescription<DataValidationGroupName>(SelectedEntityGroup))
                    {
                        case DataValidationGroupName.ExternalData:
                            if (SelectedDataEntity != null && SelectedDataEntity.ToString() != _defaultItem && ((ExternalBillingData)SelectedDataEntity).TableName != _defaultItem)
                            {
                                int dataEntityID = ((ExternalBillingData)SelectedDataEntity).pkExternalBillingDataID;
                                IEnumerable<DataValidationProperty> properties = new DataValidationPropertyModel(_eventAggregator).ReadExtDataValidationProperties(_dataValidationGroup.Value(), dataEntityID);
                                ExtDataPropertyCollection = new Dictionary<string, object>();

                                foreach (DataValidationProperty property in properties)
                                {
                                    ExtDataPropertyCollection.Add(property.ExtDataValidationProperty, property.pkDataValidationPropertyID);
                                }

                                ExtDataPropertyCollection = new Dictionary<string, object>(ExtDataPropertyCollection);
                            }
                            break;
                        default:
                            DataPropertyDisplayName = "PropertyDescription";
                            DataPropertyCollection = new ObservableCollection<DataValidationProperty>(new DataValidationPropertyModel(_eventAggregator).ReadDataValidationProperties(_dataValidationGroup, true)); break;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadEnityPropertiesAsync",
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

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCancel()
        {
            return SelectedEntityGroup != _defaultItem;
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            SelectedEntityGroup = _defaultItem;
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteAdd()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || 
                   _securityHelper.IsUserInRole(SecurityRole.DataManager.Value());
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            ExecuteCancel();
            SelectedValidationRule = new DataValidationRule();
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteDelete()
        {
            return SelectedValidationRule != null && SelectedValidationRule.pkDataValidationRuleID > 0;
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private async void ExecuteDelete()
        {
            try
            {
                if (_model.DeleteDataValidationRule(SelectedValidationRule.pkDataValidationRuleID))
                {
                    InitialiseViewControls();
                    await ReadDataValidationRulesAsync();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteDelete",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return ValidEntityGroup == Brushes.Silver && ValidDataEntity == Brushes.Silver && ValidDataProperty == Brushes.Silver && ValidOperator == Brushes.Silver;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            try
            { 
                bool result = false;
                DataTypeName dataType = (DataTypeName)((DataValidationProperty)SelectedDataProperty).enDataType;

                if (SelectedValidationRule == null)
                    SelectedValidationRule = new DataValidationRule();

                SelectedValidationRule.enDataValidationProcess = _dataValidationProcess.Value();
                SelectedValidationRule.enDataValidationGroupName = _dataValidationGroup.Value();
                SelectedValidationRule.fkDataValidationPropertyID = ((DataValidationProperty)SelectedDataProperty).pkDataValidationPropertyID;
                SelectedValidationRule.DataValidationValue = SelectedValidationValue.ToUpper();

                switch (_dataValidationGroup)
                {
                    case DataValidationGroupName.Client:
                        SelectedValidationRule.DataValidationEntityID = ((Client)SelectedDataEntity).pkClientID;
                        break;
                    case DataValidationGroupName.CompanyClient:
                    case DataValidationGroupName.Company:
                        SelectedValidationRule.DataValidationEntityID = ((Company)SelectedDataEntity).pkCompanyID;
                        break;
                    case DataValidationGroupName.Device:
                    case DataValidationGroupName.SimCard:
                        SelectedValidationRule.DataValidationEntityID = 0;
                        break;
                    case DataValidationGroupName.ExternalData:
                        SelectedValidationRule.DataValidationEntityID = ((ExternalBillingData)SelectedDataEntity).pkExternalBillingDataID;
                        break;
                }

                SelectedValidationRule.enOperatorType = EnumHelper.GetEnumFromDescription<OperatorType>(SelectedOperatorType).Value();
                switch ((OperatorType)SelectedValidationRule.enOperatorType)
                {
                    case OperatorType.StringOperator:
                        SelectedValidationRule.enOperator = ((StringOperator)Enum.Parse(typeof(StringOperator), SelectedOperator)).Value();
                        break;
                    case OperatorType.NumericOperator:
                        SelectedValidationRule.enOperator = ((NumericOperator)Enum.Parse(typeof(NumericOperator), SelectedOperator)).Value();
                        break;
                    case OperatorType.DateOperator:
                        SelectedValidationRule.enOperator = ((DateOperator)Enum.Parse(typeof(DateOperator), SelectedOperator)).Value();
                        break;
                    case OperatorType.BooleanOperator:
                        SelectedValidationRule.enOperator = ((BooleanOperator)Enum.Parse(typeof(BooleanOperator), SelectedOperator)).Value();
                        break;
                    case OperatorType.RuleOperator:
                        SelectedValidationRule.enOperator = ((RuleOperator)Enum.Parse(typeof(RuleOperator), SelectedOperator)).Value();
                        break;
                    default:
                        SelectedValidationRule.enOperator = StringOperator.None.Value();
                        break;
                }

                SelectedValidationRule.ModifiedBy = SecurityHelper.LoggedInUserFullName;
                SelectedValidationRule.ModifiedDate = DateTime.Now;

                if (SelectedValidationRule.pkDataValidationRuleID == 0)
                    result = _model.CreateDataValidationRule(SelectedValidationRule);
                else
                    result = _model.UpdateDataValidationRule(SelectedValidationRule);

                if (result)
                {
                    InitialiseViewControls();
                    await ReadDataValidationRulesAsync();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteSave",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}
