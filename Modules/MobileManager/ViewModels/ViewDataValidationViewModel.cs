using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataValidationViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        DataValidationProcess _validationProcess = DataValidationProcess.None;
        private string _billingPeriod = MobileManagerEnvironment.BillingPeriod;

        #region Commands

        public DelegateCommand StartValidationCommand { get; set; }
        public DelegateCommand StopValidationCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }
        public DelegateCommand ApplyRuleFixCommand { get; set; }
        public DelegateCommand ManualFixCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Indicate if the header must be shown
        /// </summary>
        public Visibility ShowDataValidationHeader
        {
            get { return _showHeader; }
            set { SetProperty(ref _showHeader, value); }
        }
        private Visibility _showHeader = Visibility.Visible;

        /// <summary>
        /// Indicate if the validation has been started
        /// </summary>
        public bool ValidationStarted
        {
            get { return _validationStarted; }
            set { SetProperty(ref _validationStarted, value); }
        }
        private bool _validationStarted = false;

        /// <summary>
        /// The validation group progessbar description
        /// </summary>
        public string ValidationGroupDescription
        {
            get { return _validationGroupDescription; }
            set { SetProperty(ref _validationGroupDescription, value); }
        }
        private string _validationGroupDescription;

        /// <summary>
        /// The validation group progessbar value
        /// </summary>
        public int ValidationGroupProgress
        {
            get { return _validationGroupProgress; }
            set { SetProperty(ref _validationGroupProgress, value); }
        }
        private int _validationGroupProgress;

        /// <summary>
        /// The validation entity progessbar description
        /// </summary>
        public string ValidationEntityDescription
        {
            get { return _validationEntityDescription; }
            set { SetProperty(ref _validationEntityDescription, value); }
        }
        private string _validationEntityDescription;

        /// <summary>
        /// The validation entity progessbar value
        /// </summary>
        public int ValidationEntityProgress
        {
            get { return _validationEntityProgress; }
            set { SetProperty(ref _validationEntityProgress, value); }
        }
        private int _validationEntityProgress;

        /// <summary>
        /// The validation data rule progessbar description
        /// </summary>
        public string ValidationDataRuleDescription
        {
            get { return _validationDataRuleDescription; }
            set { SetProperty(ref _validationDataRuleDescription, value); }
        }
        private string _validationDataRuleDescription;

        /// <summary>
        /// The validation data rule progessbar value
        /// </summary>
        public int ValidationDataRuleProgress
        {
            get { return _validationDataRuleProgress; }
            set { SetProperty(ref _validationDataRuleProgress, value); }
        }
        private int _validationDataRuleProgress;

        /// <summary>
        /// The validation rule entity progessbar description
        /// </summary>
        public string ValidationRuleEntityDescription
        {
            get { return _validationRuleEntityDescription; }
            set { SetProperty(ref _validationRuleEntityDescription, value); }
        }
        private string _validationRuleEntityDescription;

        /// <summary>
        /// The validation rule entity progessbar value
        /// </summary>
        public int ValidationRuleEntityProgress
        {
            get { return _validationRuleEntityProgress; }
            set { SetProperty(ref _validationRuleEntityProgress, value); }
        }
        private int _validationRuleEntityProgress;

        /// <summary>
        /// The instruction to display on the validation page
        /// </summary>
        public string ValidationPageInstruction
        {
            get { return _validationPageInstruction; }
            set { SetProperty(ref _validationPageInstruction, value); }
        }
        private string _validationPageInstruction = string.Empty;

        /// <summary>
        /// The number of validation groups
        /// </summary>
        public int ValidationGroupCount
        {
            get { return _validationGroupCount; }
            set { SetProperty(ref _validationGroupCount, value); }
        }
        private int _validationGroupCount;

        /// <summary>
        /// The number of entities tpvalidate
        /// </summary>
        public int ValidationEntityCount
        {
            get { return _validationEntityCount; }
            set { SetProperty(ref _validationEntityCount, value); }
        }
        private int _validationEntityCount;

        /// <summary>
        /// The number of validation rules
        /// </summary>
        public int ValidationDataRuleCount
        {
            get { return _validationDataRuleCount; }
            set { SetProperty(ref _validationDataRuleCount, value); }
        }
        private int _validationDataRuleCount;

        /// <summary>
        /// The number of validation rules
        /// </summary>
        public int ValidationRuleEntityCount
        {
            get { return _validationRuleEntityCount; }
            set { SetProperty(ref _validationRuleEntityCount, value); }
        }
        private int _validationRuleEntityCount;

        /// <summary>
        /// The number of validation groups that passed validation
        /// </summary>
        public int ValidationGroupsPassed
        {
            get { return _validationGroupsPassed; }
            set { SetProperty(ref _validationGroupsPassed, value); }
        }
        private int _validationGroupsPassed;

        /// <summary>
        /// The number of validation groups that failed validation
        /// </summary>
        public int ValidationGroupsFailed
        {
            get { return _validationGroupsFailed; }
            set { SetProperty(ref _validationGroupsFailed, value); }
        }
        private int _validationGroupsFailed;

        /// <summary>
        /// The number of validation entities that passed validation
        /// </summary>
        public int ValidationEntitiesPassed
        {
            get { return _validationEntitiesPassed; }
            set { SetProperty(ref _validationEntitiesPassed, value); }
        }
        private int _validationEntitiesPassed;

        /// <summary>
        /// The number of validation entities that failed validation
        /// </summary>
        public int ValidationEntitiesFailed
        {
            get { return _validationEntitiesFailed; }
            set { SetProperty(ref _validationEntitiesFailed, value); }
        }
        private int _validationEntitiesFailed;

        /// <summary>
        /// The number of validation data rules that passed validation
        /// </summary>
        public int ValidationDataRulesPassed
        {
            get { return _validationDataRulesPassed; }
            set { SetProperty(ref _validationDataRulesPassed, value); }
        }
        private int _validationDataRulesPassed;

        /// <summary>
        /// The number of validation data rules that failed validation
        /// </summary>
        public int ValidationDataRulesFailed
        {
            get { return _validationDataRulesFailed; }
            set { SetProperty(ref _validationDataRulesFailed, value); }
        }
        private int _validationDataRulesFailed;

        /// <summary>
        /// The number of validation data rules that passed validation
        /// </summary>
        public int ValidationRuleEntitiesPassed
        {
            get { return _validationRuleEntitiesPassed; }
            set { SetProperty(ref _validationRuleEntitiesPassed, value); }
        }
        private int _validationRuleEntitiesPassed;

        /// <summary>
        /// The number of validation data rules that failed validation
        /// </summary>
        public int ValidationRuleEntitiesFailed
        {
            get { return _validationRuleEntitiesFailed; }
            set { SetProperty(ref _validationRuleEntitiesFailed, value); }
        }
        private int _validationRuleEntitiesFailed;

        /// <summary>
        /// The selected exceptions
        /// </summary>
        public string SelectedExceptions
        {
            get { return _selectedExceptions; }
            set
            {
                SetProperty(ref _selectedExceptions, value);
                PopulateSelectedExceptions(value);
            }
        }
        private string _selectedExceptions;

        /// <summary>
        /// Indicate if all exceptions is slected
        /// </summary>
        public bool SelectAllExceptions
        {
            get { return _selectAllExceptions; }
            set { SetProperty(ref _selectAllExceptions, value); }
        }
        private bool _selectAllExceptions = false;

        /// <summary>
        /// The current selected exception
        /// </summary>
        private DataValidationException SelectedException
        {
            get { return _selectedException; }
            set { SetProperty(ref _selectedException, value); }
        }
        private DataValidationException _selectedException = null;

        /// <summary>
        /// Indicate if the current billing process completed
        /// </summary>
        public bool BillingProcessCompleted
        {
            get { return _billingProcessCompleted; }
            set { SetProperty(ref _billingProcessCompleted, value); }
        }
        private bool _billingProcessCompleted = false;

        /// <summary>
        /// The selected exceptions to fix
        /// </summary>
        private ObservableCollection<DataValidationException> ExceptionsToFix
        {
            get { return _exceptionsToFix; }
            set { SetProperty(ref _exceptionsToFix, value); }
        }
        private ObservableCollection<DataValidationException> _exceptionsToFix = null;

        /// <summary>
        /// The collection of validation groups from the DataValidationEntity enum
        /// </summary>
        public ObservableCollection<string> ValidationGroupCollection
        {
            get { return _categoryCollection; }
            set { SetProperty(ref _categoryCollection, value); }
        }
        private ObservableCollection<string> _categoryCollection = null;

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<DataValidationRule> ValidationRuleCollection
        {
            get { return _validationRuleCollection; }
            set { SetProperty(ref _validationRuleCollection, value); }
        }
        private ObservableCollection<DataValidationRule> _validationRuleCollection = null;

        /// <summary>
        /// The collection of data validation exception Info
        /// </summary>
        public ObservableCollection<DataValidationException> ValidationErrorCollection
        {
            get { return _validationErrorCollection; }
            set { SetProperty(ref _validationErrorCollection, value); }
        }
        private ObservableCollection<DataValidationException> _validationErrorCollection = null;

        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        #endregion

        #region Input Validation

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

        public object Dispatcher { get; private set; }

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
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event gets received to update the progressbar
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void ProgressBarInfo_Event(object sender)
        {
            Application.Current.Dispatcher.Invoke(() => { UpdateProgressBarValues(sender); });
        }

        /// <summary>
        /// This event gets received to display the validation result info
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void DataValiationResult_Event(object sender)
        {
            Application.Current.Dispatcher.Invoke(() => { DisplayDataValidationResults(sender); });
        }

        /// <summary>
        /// This event gets received to enable the next button 
        /// when the process completed
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingProcessCompleted_Event(BillingExecutionState sender)
        {
            if (sender == BillingExecutionState.InternalDataValidation)
                Application.Current.Dispatcher.Invoke(() => { BillingProcessCompleted = true; });
        }

        /// <summary>
        /// This event show or hide the control header
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void ShowDataValidationHeader_Event(bool sender)
        {
            ShowDataValidationHeader = sender ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDataValidationViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseDataValidationView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDataValidationView()
        {
            _model = new DataValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands 
            StartValidationCommand = new DelegateCommand(ExecuteStartValidation, CanStartValidation).ObservesProperty(() => ValidationRuleCollection)
                                                                                                    .ObservesProperty(() => BillingProcessCompleted);
            StopValidationCommand = new DelegateCommand(ExecuteStopValidation, CanStopValidation).ObservesProperty(() => ValidationStarted)
                                                                                                 .ObservesProperty(() => ValidationRuleEntitiesFailed);
            ExportCommand = new DelegateCommand(ExecuteExport, CanExport).ObservesProperty(() => ExceptionsToFix);
            ApplyRuleFixCommand = new DelegateCommand(ExecuteApplyRuleFix, CanApplyRuleFix).ObservesProperty(() => ExceptionsToFix);
            ManualFixCommand = new DelegateCommand(ExecuteManualFix, CanManualFix).ObservesProperty(() => ExceptionsToFix);
            _validationProcess = MobileManagerEnvironment.SelectedProcessMenu == ProcessMenuOption.SystemTools ? DataValidationProcess.System :
                                                                                                                 DataValidationProcess.SystemBilling;

            // Subscribe to this event to update the progressbar
            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Subscribe(ProgressBarInfo_Event, true);

            // Subscribe to this event to display the data validation errors
            _eventAggregator.GetEvent<DataValiationResultEvent>().Subscribe(DataValiationResult_Event, true);

            // Subscribe to this event to display or hide the data validation control header
            _eventAggregator.GetEvent<DataValiationHeaderEvent>().Subscribe(ShowDataValidationHeader_Event, true);

            // Subscribe to this event to lock the completed process
            // and enable functionality to move to the process completed
            _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Subscribe(BillingProcessCompleted_Event, true);

            // Load the view data
            // Load the view data
            ReadDataValidationGroups();
            await ReadValidationRuleExceptionsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ValidationPageInstruction = string.Format("Please start the process to validate the {0} based on the rules configured for each data validation group.", _validationProcess.ToString().ToLower());
            ValidationGroupDescription = string.Format("Validating data group - {0} of {1}", 0, ValidationGroupCollection != null ? ValidationGroupCollection.Count : 0);
            ValidationEntityDescription = string.Format("Validating entity - {0} of {1}", 0, 0);
            ValidationDataRuleDescription = string.Format("Validating data rule - {0} of {1}", 0, 0);
            ValidationRuleEntityDescription = string.Format("Validating data rule entity - {0} of {1}", 0, 0);
            ValidationGroupProgress = ValidationEntityProgress = ValidationDataRuleProgress = ValidationRuleEntityProgress = 0;
            ValidationGroupCount = ValidationEntityCount = ValidationDataRuleCount = ValidationRuleEntityCount = 0;
            ValidationGroupsPassed = ValidationEntitiesPassed = ValidationDataRulesPassed = ValidationRuleEntitiesPassed = 0;
            ValidationGroupsFailed = ValidationEntitiesFailed = ValidationDataRulesFailed = ValidationRuleEntitiesFailed = 0;
            ValidationErrorCollection = null;
            SelectAllExceptions = false;
        }

        /// <summary>
        /// Load all the validation entity groups from the DataValidationGroupName enum
        /// </summary>
        private void ReadDataValidationGroups()
        {
            try
            {
                ValidationGroupCollection = new ObservableCollection<string>();

                foreach (DataValidationGroupName source in Enum.GetValues(typeof(DataValidationGroupName)))
                {
                    if (source != DataValidationGroupName.None && source != DataValidationGroupName.ExternalData)
                        ValidationGroupCollection.Add(source.ToString());
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationGroups",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        /// <param name="validationGroup">The valiation group to read the rules for.</param>
        private async Task ReadValidationRulesAsync(string validationGroup)
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadDataValidationRules(_validationProcess, (DataValidationGroupName)Enum.Parse(typeof(DataValidationGroupName), validationGroup)));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadValidationRulesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the validation rule exceptions if any from the database
        /// </summary>
        private async Task ReadValidationRuleExceptionsAsync()
        {
            try
            {
                ValidationErrorCollection = await Task.Run(() => new DataValidationExceptionModel(_eventAggregator).ReadDataValidationExceptions(_billingPeriod, _validationProcess.Value()));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadValidationRuleExceptionsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Add the data validation error to the validation exception listview
        /// </summary>
        /// <param name="resultInfo">The data validation result info.</param>
        private void DisplayDataValidationResults(object validationResultInfo)
        {
            try
            {
                if (ValidationErrorCollection == null)
                    ValidationErrorCollection = new ObservableCollection<DataValidationException>();

                DataValidationException resultInfo = (DataValidationException)validationResultInfo;

                if (resultInfo.Result == true)
                {
                    ValidationRuleEntitiesPassed++;
                }
                else
                {
                    ValidationRuleEntitiesFailed++;
                    ValidationErrorCollection.Add(resultInfo);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "DisplayDataValidationResults",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Update the progressbar values
        /// </summary>
        /// <param name="progressBarInfo">The progressbar info.</param>
        private void UpdateProgressBarValues(object progressBarInfo)
        {
            try
            {
                ProgressBarInfo progressInfo = (ProgressBarInfo)progressBarInfo;
                ValidationRuleEntityCount = progressInfo.MaxValue;
                ValidationRuleEntityProgress = progressInfo.CurrentValue;
                ValidationRuleEntityDescription = string.Format("Validating data rule entity - {0} of {1}", ValidationRuleEntityProgress,
                                                                                                            progressInfo.MaxValue);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateProgressBarValues",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Populate the selected exceptions to fix collection
        /// </summary>
        /// <param name="selectedExceptions">The Xceed CheckListBox selected items string.</param>
        private void PopulateSelectedExceptions(string selectedExceptions)
        {
            try
            {
                ObservableCollection<DataValidationException> resultInfosCollection = new ObservableCollection<DataValidationException>();
                DataValidationException resultInfo = null;

                // The Xceed CheckListBox return all the selected items in
                // a comma delimeted string, so get the number of selected 
                // items we need to split the string
                string[] exceptionsToFix = selectedExceptions.Split(',');

                // Find the last selected exception based on the exception message
                if (ValidationErrorCollection != null)
                {
                    SelectedException = ValidationErrorCollection.Where(p => p.Message == exceptionsToFix.Last()).FirstOrDefault();

                    // Convert all the string values to DataValidationResultInfo
                    // and populate the ValidationErrorCollection
                    foreach (string exception in exceptionsToFix)
                    {
                        resultInfo = ValidationErrorCollection.Where(p => p.Message == exception).FirstOrDefault();

                        if (resultInfo != null)
                        {
                            resultInfosCollection.Add(resultInfo);
                        }
                    }
                }

                ExceptionsToFix = new ObservableCollection<DataValidationException>(resultInfosCollection);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "PopulateSelectedExceptions",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Create a new billing process history entry
        /// </summary>
        private async Task CreateBillingProcessHistoryAsync()
        {
            try
            {
                bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.InternalDataValidation));

                // Publish this event to update the billing process history on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateBillingProcessHistoryAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Create a new billing process history entry
        /// </summary>
        /// <param name="billingProcess">The billing process to complete.</param>/// 
        private async Task CompleteBillingProcessHistoryAsync(BillingExecutionState billingProcess)
        {
            try
            {
                bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CompleteBillingProcessHistory(billingProcess, true));

                if (result)
                {
                    // Publish this event to update the billing process history on the wizard's Info content
                    _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);

                    // Publish this event to lock the completed process and enable 
                    // functinality to move to the next process
                    if (billingProcess == BillingExecutionState.InternalDataValidation)
                        _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Publish(BillingExecutionState.InternalDataValidation);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CompleteBillingProcessHistoryAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Save all the data rule exceptions to the database
        /// </summary>
        private async Task CreateValidationRuleExceptionsAsync()
        {
            try
            {
                bool result = await Task.Run(() => new DataValidationExceptionModel(_eventAggregator).CreateDataValidationExceptions(ValidationErrorCollection));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateValidationRuleExceptionsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanStartValidation()
        {
            return _validationProcess != DataValidationProcess.None && ValidationGroupCollection != null &&
                   ValidationGroupCollection.Count > 0 && BillingProcessCompleted == false ? true : false;
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private async void ExecuteStartValidation()
        {
            string groupDescription = string.Empty;
            int entitiesFailed = 0;
            InitialiseViewControls();
            ValidationStarted = true;

            try
            {
                // Update billing related values
                if (_validationProcess == DataValidationProcess.SystemBilling)
                {
                    // Set the previous data validation process as complete
                    await CompleteBillingProcessHistoryAsync(BillingExecutionState.Started);

                    // Create a new history entry everytime the process get started
                    await CreateBillingProcessHistoryAsync();

                    // Update the process progress values on the wizard's Info content
                    _eventAggregator.GetEvent<BillingProcessEvent>().Publish(BillingExecutionState.InternalDataValidation);

                    // Disable the next buttton when the process gets started
                    _eventAggregator.GetEvent<BillingProcessStartedEvent>().Publish(BillingExecutionState.InternalDataValidation);
                }

                // Set the group progressbar max value
                ValidationGroupCount = ValidationGroupCollection.Count;

                foreach (string group in ValidationGroupCollection)
                {
                    groupDescription = EnumHelper.GetDescriptionFromEnum((DataValidationGroupName)Enum.Parse(typeof(DataValidationGroupName), group));
                    entitiesFailed = 0;

                    // Read the validation rules for the specified
                    // group from the database
                    await ReadValidationRulesAsync(group);

                    // Set the validation group progresssbar description
                    ValidationGroupDescription = string.Format("Validating {0} - {1} of {2}", groupDescription.ToUpper(),
                                                                                              ++ValidationGroupProgress,
                                                                                              ValidationGroupCollection.Count);
                    if (ValidationRuleCollection != null && ValidationRuleCollection.Count > 0)
                    {
                        int entityCount = ValidationRuleCollection.GroupBy(p => p.DataValidationEntityID).Count();
                        int entityID = 0;

                        // Set the entity and data rule progressbars max values
                        ValidationEntityCount = entityCount;
                        ValidationDataRuleCount = ValidationRuleCollection.Count;

                        foreach (DataValidationRule rule in ValidationRuleCollection)
                        {
                            // Allow the user to stop the validation
                            if (!ValidationStarted)
                                break;

                            // Set the data rule progresssbar description
                            ValidationDataRuleDescription = string.Format("Validating {0} - {1} of {2}", rule.PropertyDescription.ToUpper(),
                                                                                                         ++ValidationDataRuleProgress,
                                                                                                         ValidationRuleCollection.Count);

                            // Validate the data rule and update
                            // the progress values accodingly
                            if (await Task.Run(() => _model.ValidateDataValidationRule(rule)))
                                ++ValidationDataRulesPassed;
                            else
                                ++ValidationDataRulesFailed;

                            // Update progress values when the entity change
                            if (entityID != rule.DataValidationEntityID)
                            {
                                entityID = rule.DataValidationEntityID;

                                // Set the validation entity progresssbar description
                                ValidationEntityDescription = string.Format("Validating {0} - {1} of {2}", rule.EntityDescription.ToUpper(),
                                                                                                           ++ValidationEntityProgress,
                                                                                                           entityCount);
                                // Update the entity progress values
                                if (ValidationDataRulesFailed == 0)
                                    ++ValidationEntitiesPassed;
                                else
                                    ++ValidationEntitiesFailed;

                                entitiesFailed = ValidationEntitiesFailed;
                            }
                        }
                    }

                    // Update the validation group 
                    // progress values accodingly
                    if (ValidationRuleCollection == null || ValidationRuleCollection.Count == 0 || entitiesFailed == 0)
                        ++ValidationGroupsPassed;
                    else
                        ++ValidationGroupsFailed;

                    // Allow the user to stop the validation
                    if (!ValidationStarted)
                        break;
                }

                // If NO validations exceptions found the set 
                // the data validation process as complete
                // else save the exceptions to the database
                if (ValidationRuleCollection != null && ValidationErrorCollection != null && ValidationRuleCollection.Count > 0)
                {
                    if (ValidationErrorCollection.Count == 0)
                        await CompleteBillingProcessHistoryAsync(BillingExecutionState.InternalDataValidation);
                    else
                        await CreateValidationRuleExceptionsAsync();
                }

                // Disable the stop button
                ValidationStarted = false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteStartValidation",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanStopValidation()
        {
            return ValidationStarted;
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state 
        /// </summary>
        /// <returns></returns>
        private void ExecuteStopValidation()
        {
            ValidationStarted = false;
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExport()
        {
            return ExceptionsToFix != null && ExceptionsToFix.Count > 0;
        }

        /// <summary>
        /// Execute when the export to excel command button is clicked 
        /// </summary>
        private void ExecuteExport()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                dialog.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result.ToString() == "OK")
                {
                    MSOfficeHelper officeHelper = new MSOfficeHelper();
                    DataTable dt = new DataTable();
                    DataRow dataRow = null;
                    dt.Columns.Add("Exception Description");
                    string fileName = string.Format("{0}\\ValidationExceptions - {1}.xlsx", dialog.SelectedPath, DateTime.Now.ToShortDateString());

                    // Add all the exceptions to a data table              
                    foreach (DataValidationException exception in ExceptionsToFix)
                    {
                        dataRow = dt.NewRow();
                        dt.Rows.Add(exception.Message);
                    }

                    officeHelper.ExportDataTableToExcel(dt, fileName);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteExport",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanApplyRuleFix()
        {
            return ExceptionsToFix != null && ExceptionsToFix.Any(p => p.CanApplyRule == true) && !ExceptionsToFix.Any(p => p.CanApplyRule == false);
        }

        /// <summary>
        /// Execute when the apply rule command button is clicked 
        /// </summary>
        private async void ExecuteApplyRuleFix()
        {
            try
            {
                // Apply the data rule to each exception and remove the fixed
                // exceptions to the exceptions collection                  
                foreach (DataValidationException exception in ExceptionsToFix)
                {
                    if (await Task.Run(() => _model.ApplyDataRule(exception)))
                        ValidationErrorCollection.Remove(exception);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteApplyRuleFix",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanManualFix()
        {
            return ExceptionsToFix != null && ExceptionsToFix.Count == 1 && !ExceptionsToFix.Any(p => p.CanApplyRule == true);
        }

        /// <summary>
        /// Execute when the manual fix command button is clicked 
        /// </summary>
        private void ExecuteManualFix()
        {
            try
            {
                if (_selectedException != null)
                    _eventAggregator.GetEvent<SearchResultEvent>().Publish(_selectedException.DataValidationEntityID);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteManualFix",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}
