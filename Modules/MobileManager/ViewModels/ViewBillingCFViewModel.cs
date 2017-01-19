using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewBillingCFViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) validation rule entity
        /// </summary>
        public ValidationRule SelectedValidationRule
        {
            get { return _selectedValidationRule; }
            set
            {
                SetProperty(ref _selectedValidationRule, value);

                if (value != null && value.enValidationRuleGroup > 0)
                {
                    switch (((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)))
                    {
                        case ValidationRuleGroup.Client:
                            EntityDisplayName = "PrimaryCellNumber";
                            SelectedEntity = new Client();
                            SelectedEntity = ClientCollection != null && value.EntityID > 0 ? ClientCollection.First(p => p.pkClientID == value.EntityID) :
                                             ClientCollection != null ? ClientCollection.First(p => p.pkClientID == 0) : null;
                            break;
                        case ValidationRuleGroup.Company:
                            EntityDisplayName = "CompanyName";
                            foreach (Company entity in EntityCollection)
                            {
                                if (entity.pkCompanyID == value.EntityID)
                                {
                                    SelectedEntity = entity;
                                    break;
                                }
                            }
                            break;
                    }

                    SelectedRuleData = RuleDataCollection != null ? RuleDataCollection.First(p => p.pkValidationRulesDataID == value.fkValidationRulesDataID) :
                                       RuleDataCollection != null ? RuleDataCollection.First(p => p.pkValidationRulesDataID == 0) : null;

                    if (value.RuleDataTypeName != null)
                    {
                        switch (((ValidationDataType)Enum.Parse(typeof(ValidationDataType), value.RuleDataTypeName)))
                        {
                            case ValidationDataType.String:
                                SelectedOperator = value != null && value.enStringCompareType > 0 ? ((StringOperator)value.enStringCompareType).ToString() : StringOperator.None.ToString();
                                break;
                            case ValidationDataType.Bool:
                                SelectedOperator = "Equal";
                                break;
                            default:
                                SelectedOperator = value != null && value.enNumericCompareType > 0 ? ((NumericOperator)value.enStringCompareType).ToString() : StringOperator.None.ToString();
                                break;
                        }
                    }
                    else if (OperatorCollection != null)
                    {
                        SelectedOperator = OperatorCollection.First();
                    }

                    SelectedValidationValue = value.ValidationDataValue;
                    SetProperty(ref _selectedValidationRule, value);
                }
            }
        }
        private ValidationRule _selectedValidationRule = new ValidationRule();

        /// <summary>
        /// The entered validation value
        /// </summary>
        public string SelectedValidationValue
        {
            get { return _selectedValidationValue; }
            set { SetProperty(ref _selectedValidationValue, value); }
        }
        private string _selectedValidationValue = string.Empty;

        /// <summary>
        /// The entity display name based on the selected category
        /// </summary>
        public string EntityDisplayName
        {
            get { return _entityDisplayName; }
            set { SetProperty(ref _entityDisplayName, value); }
        }
        private string _entityDisplayName = string.Empty;

        /// <summary>
        /// The entity label based on the selected category
        /// </summary>
        public string EntityLabel
        {
            get { return _entityLabel; }
            set { SetProperty(ref _entityLabel, value); }
        }
        private string _entityLabel = "Entity:";

        /// <summary>
        /// The selected billing process
        /// </summary>
        public BillingProcess SelectedBillingProcess
        {
            get { return _selectedBillingProcess; }
            set { SetProperty(ref _selectedBillingProcess, value); }
        }
        private BillingProcess _selectedBillingProcess = BillingProcess.DataValidation;

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<BillingProcess> BillingProcessCollection
        {
            get { return _billingProcessCollection; }
            set { SetProperty(ref _billingProcessCollection, value); }
        }
        private ObservableCollection<BillingProcess> _billingProcessCollection = null;

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<ValidationRule> ValidationRuleCollection
        {
            get { return _validationRuleCollection; }
            set { SetProperty(ref _validationRuleCollection, value); }
        }
        private ObservableCollection<ValidationRule> _validationRuleCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of validation categories from the ValidationRuleGroup enum
        /// </summary>
        public ObservableCollection<string> CategoryCollection
        {
            get { return _categoryCollection; }
            set { SetProperty(ref _categoryCollection, value); }
        }
        private ObservableCollection<string> _categoryCollection = null;

        /// <summary>
        /// The collection of company entities from the database
        /// </summary>
        public ObservableCollection<object> EntityCollection 
        {
            get { return _entityCollection; }
            set { SetProperty(ref _entityCollection, value); } 
        }
        private ObservableCollection<object> _entityCollection = null;

        /// <summary>
        /// The collection of company entities from the database
        /// </summary>
        public ObservableCollection<Company> CompanyCollection
        {
            get { return _companyCollection; }
            set { SetProperty(ref _companyCollection, value); }
        }
        private ObservableCollection<Company> _companyCollection = null;

        /// <summary>
        /// The collection of client entities from the database
        /// </summary>
        public ObservableCollection<Client> ClientCollection
        {
            get { return _clientCollection; }
            set { SetProperty(ref _clientCollection, value); }
        }
        private ObservableCollection<Client> _clientCollection = null;

        /// <summary>
        /// The collection of validation rule data from the database
        /// </summary>
        public ObservableCollection<ValidationRulesData> RuleDataCollection
        {
            get { return _ruleDataCollection; }
            set { SetProperty(ref _ruleDataCollection, value); }
        }
        private ObservableCollection<ValidationRulesData> _ruleDataCollection = null;

        /// <summary>
        /// The collection of operators types from the string, numeric
        /// and date OperatorType enum's
        /// </summary>
        public ObservableCollection<string> OperatorCollection
        {
            get { return _operatorCollection; }
            set { SetProperty(ref _operatorCollection, value); }
        }
        private ObservableCollection<string> _operatorCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected selected category
        /// </summary>
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                SetProperty(ref _selectedCategory, value);

                if (value != null && value != "-- Please Select --")
                {
                    Task.Run(() => ReadValidationEntitesAsync());
                    Task.Run(() => ReadValidationdRulesDataAsync());
                    Task.Run(() => ReadValidationRulesAsync());
                }
                else
                {
                    EntityCollection = null;
                    RuleDataCollection = null;
                    OperatorCollection = null;
                    SelectedValidationValue = string.Empty;
                }
            }
        }
        private string _selectedCategory = string.Empty;

        /// <summary>
        /// The selected entity
        /// </summary>
        public object SelectedEntity
        {
            get { return _selectedEntity; }
            set { SetProperty(ref _selectedEntity, value); }
        }
        private object _selectedEntity = null;

        /// <summary>
        /// The selected validation rule data
        /// </summary>
        public ValidationRulesData SelectedRuleData
        {
            get { return _selectedRuleData; }
            set
            {
                SetProperty(ref _selectedRuleData, value);

                if (value != null)
                    ReadDataTypeOperators();
            }
        }
        private ValidationRulesData _selectedRuleData = null;

        /// <summary>
        /// The selected operator
        /// </summary>
        public string SelectedOperator
        {
            get { return _selectedOperator; }
            set { SetProperty(ref _selectedOperator, value); }
        }
        private string _selectedOperator;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCategory
        {
            get { return _validCategory; }
            set { SetProperty(ref _validCategory, value); }
        }
        private Brush _validCategory = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidEntity
        {
            get { return _validEntity; }
            set { SetProperty(ref _validEntity, value); }
        }
        private Brush _validEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidRuleData
        {
            get { return _validRuleData; }
            set { SetProperty(ref _validRuleData, value); }
        }
        private Brush _validRuleData = Brushes.Red;

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
                    case "SelectedCategory":
                        ValidCategory = string.IsNullOrEmpty(SelectedCategory) || SelectedCategory == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedEntity":
                        if (SelectedCategory != null && SelectedCategory != "-- Please Select --")
                            switch (((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)))
                            {
                                case ValidationRuleGroup.Client:
                                    ValidEntity = SelectedEntity == null || ((Client)SelectedEntity).pkClientID < 1 ? Brushes.Red : Brushes.Silver; break;
                                case ValidationRuleGroup.Company:
                                    ValidEntity = SelectedEntity == null || ((Company)SelectedEntity).pkCompanyID < 1 ? Brushes.Red : Brushes.Silver; break;
                            }
                        else
                            ValidEntity = Brushes.Red;
                        break;
                    case "SelectedRuleData":
                        ValidRuleData = SelectedRuleData == null || SelectedRuleData.pkValidationRulesDataID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedOperator":
                        ValidOperator = string.IsNullOrEmpty(SelectedOperator) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewBillingCFViewModel(IEventAggregator eventAggregator)
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
            _model = new ValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedCategory);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => ValidCategory)
                                                                          .ObservesProperty(() => ValidEntity)
                                                                          .ObservesProperty(() => ValidRuleData)
                                                                          .ObservesProperty(() => ValidOperator);

            // Load the view data
            ReadBillingProcesses();
            ReadValidationRuleGroups();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedEntity = null;
            SelectedRuleData = null;
            SelectedOperator = null;
            SelectedValidationRule = new ValidationRule();
            SelectedValidationValue = string.Empty;
        }

        /// <summary>
        /// Load all the billing processes from the  
        /// billing process enum's
        /// </summary>
        private void ReadBillingProcesses()
        {
            try
            {
                BillingProcessCollection = new ObservableCollection<BillingProcess>();

                foreach (BillingProcess process in Enum.GetValues(typeof(BillingProcess)))
                {
                    BillingProcessCollection.Add(process);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        private async Task ReadValidationRulesAsync()
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadValidationRules((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the validation rule groups from the database
        /// </summary>
        private void ReadValidationRuleGroups()
        {
            try
            {
                CategoryCollection = new ObservableCollection<string>();

                foreach (ValidationRuleGroup source in Enum.GetValues(typeof(ValidationRuleGroup)))
                {
                    if (source == ValidationRuleGroup.None)
                        CategoryCollection.Add("-- Please Select --");
                    else
                        CategoryCollection.Add(source.ToString());
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the entities based on the selected rule group from the database
        /// </summary>
        private void ReadValidationEntitesAsync()
        {
            try
            {
                ClientCollection = null;
                CompanyCollection = null;
                EntityLabel = string.Format("{0}:", SelectedCategory);

                switch (((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)))
                {
                    case ValidationRuleGroup.Client:
                        EntityDisplayName = "PrimaryCellNumber";
                        EntityCollection = new ObservableCollection<object>(new ClientModel(_eventAggregator).ReadClients(true));
                        break;
                    case ValidationRuleGroup.Company:
                        EntityDisplayName = "CompanyName";
                        EntityCollection = new ObservableCollection<object>(new CompanyModel(_eventAggregator).ReadCompanies(true));
                        break;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the validation rules data based on the selected rule group from the database
        /// </summary>
        private void ReadValidationdRulesDataAsync()
        {
            try
            {
                OperatorCollection = null;
                short groupID = ((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)).Value();
                RuleDataCollection = new ObservableCollection<ValidationRulesData>(new ValidationRulesDataModel(_eventAggregator).ReadValidationRulesData(true)
                                                                                                                                 .Where(p => p.enValidationRuleGroup == groupID));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the data type operators from the string, 
        /// numeric and date OperatorType enum's
        /// </summary>
        private void ReadDataTypeOperators()
        {
            try
            {
                if (SelectedRuleData.pkValidationRulesDataID > 0)
                {
                    OperatorCollection = new ObservableCollection<string>();

                    switch ((ValidationDataType)SelectedRuleData.enValidationDataType)
                    {
                        case ValidationDataType.String:
                            foreach (StringOperator source in Enum.GetValues(typeof(StringOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case ValidationDataType.Bool:
                            OperatorCollection.Add("Equal");
                            break;
                        default:
                            foreach (NumericOperator source in Enum.GetValues(typeof(NumericOperator)))
                            {
                                if (source != NumericOperator.None)
                                    OperatorCollection.Add(source.ToString());
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
            return SelectedCategory != "-- Please Select --";
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            SelectedCategory = "-- Please Select --";
            ValidationRuleCollection = null;
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteAdd()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value());
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            SelectedValidationRule = new ValidationRule();
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return ValidCategory == Brushes.Silver && ValidEntity == Brushes.Silver && ValidRuleData == Brushes.Silver && ValidOperator == Brushes.Silver;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;
            ValidationDataType dataType = (ValidationDataType)SelectedRuleData.enValidationDataType;

            if (SelectedValidationRule == null)
                SelectedValidationRule = new ValidationRule();

            SelectedValidationRule.enValidationRuleGroup = ((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)).Value();
            SelectedValidationRule.fkValidationRulesDataID = SelectedRuleData.pkValidationRulesDataID;
            SelectedValidationRule.ValidationDataValue = SelectedValidationValue.ToUpper();

            switch (((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)))
            {
                case ValidationRuleGroup.Client:
                    SelectedValidationRule.EntityID = ((Client)SelectedEntity).pkClientID;
                    break;
                case ValidationRuleGroup.Company:
                    SelectedValidationRule.EntityID = ((Company)SelectedEntity).pkCompanyID;
                    break;
            }

            SelectedValidationRule.enStringCompareType = SelectedValidationRule.enDateCompareType = SelectedValidationRule.enNumericCompareType = 0;
            if (dataType == ValidationDataType.String)
                SelectedValidationRule.enStringCompareType = ((StringOperator)Enum.Parse(typeof(StringOperator), SelectedOperator)).Value();
            else if (dataType == ValidationDataType.Date)
                SelectedValidationRule.enDateCompareType = ((DateCompareValue)Enum.Parse(typeof(DateCompareValue), SelectedOperator)).Value();
            else
                SelectedValidationRule.enNumericCompareType = ((NumericOperator)Enum.Parse(typeof(NumericOperator), SelectedOperator)).Value();

            SelectedValidationRule.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedValidationRule.ModifiedDate = DateTime.Now;

            if (SelectedValidationRule.pkValidationRuleID == 0)
                result = _model.CreateValidationRule(SelectedValidationRule);
            else
                result = _model.UpdateValidationRule(SelectedValidationRule);

            if (result)
            {
                InitialiseViewControls();
                await ReadValidationRulesAsync();
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteMaintenace()
        {
            return true;
        }

        #endregion

        #endregion
    }
}
