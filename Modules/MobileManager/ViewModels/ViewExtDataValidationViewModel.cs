using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Events;
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
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewExtDataValidationViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private string _billingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year);

        #region Commands

        public DelegateCommand StartValidationCommand { get; set; }
        public DelegateCommand StopValidationCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }
        public DelegateCommand ExceptExceptionCommand { get; set; }

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
        
        /// <summary>
        /// The collection of data validation exception Info
        /// </summary>
        public DataTable ImportedDataCollection
        {
            get { return _importedDataCollection; }
            set { SetProperty(ref _importedDataCollection, value); }
        }
        private DataTable _importedDataCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of external billing entries
        /// </summary>
        public ObservableCollection<ExternalBillingData> ExternalDataCollection
        {
            get { return _externalDataCollection; }
            set { SetProperty(ref _externalDataCollection, value); }
        }
        private ObservableCollection<ExternalBillingData> _externalDataCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected external billing file
        /// </summary>
        public ExternalBillingData SelectedExternalData
        {
            get { return _selectedExternalData; }
            set
            {
                SetProperty(ref _selectedExternalData, value);
                InitialiseViewControls();
                if (value != null && value.pkExternalBillingDataID > 0)
                     Task.Run(() => ReadImportedExternalDataAsync());
            }
        }
        private ExternalBillingData _selectedExternalData = null;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidExternalData
        {
            get { return _validExternalData; }
            set { SetProperty(ref _validExternalData, value); }
        }
        private Brush _validExternalData = Brushes.Red;

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
                    case "SelectedExternalData":
                        ValidExternalData = SelectedExternalData != null && SelectedExternalData.pkExternalBillingDataID < 1 ? Brushes.Red : Brushes.Silver; break;
                }
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
            if (sender == BillingExecutionState.ExternalDataValidation)
                BillingProcessCompleted = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewExtDataValidationViewModel(IEventAggregator eventAggregator)
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
                                                                                                    .ObservesProperty(() => SelectedExternalData)
                                                                                                    .ObservesProperty(() => BillingProcessCompleted);
            StopValidationCommand = new DelegateCommand(ExecuteStopValidation, CanStopValidation).ObservesProperty(() => ValidationStarted)
                                                                                                 .ObservesProperty(() => ValidationRuleEntitiesFailed);
            ExportCommand = new DelegateCommand(ExecuteExport, CanExport).ObservesProperty(() => ExceptionsToFix);
            ExceptExceptionCommand = new DelegateCommand(ExecuteApplyRuleFix, CanApplyRuleFix).ObservesProperty(() => ExceptionsToFix);

            // Subscribe to this event to update the progressbar
            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Subscribe(ProgressBarInfo_Event, true);

            // Subscribe to this event to display the data validation errors
            _eventAggregator.GetEvent<DataValiationResultEvent>().Subscribe(DataValiationResult_Event, true);

            // Subscribe to this event to lock the completed process
            // and enable functionality to move to the process completed
            _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Subscribe(BillingProcessCompleted_Event, true);

            // Load the view data
            await ReadExternalBillingDataAsync();
            await ReadValidationRulesAsync();
            await ReadValidationRuleExceptionsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ValidationPageInstruction = "Please start the process to validate the external billing data based on data rules configured for each external data provider.";
            ValidationDataRuleDescription = string.Format("Validating data rule - {0} of {1}", 0, 0);
            ValidationRuleEntityDescription = string.Format("Validating data rule entity - {0} of {1}", 0, 0);
            ValidationDataRuleProgress = ValidationRuleEntityProgress = 0;
            ValidationDataRuleCount = ValidationRuleEntityCount = 0;
            ValidationDataRulesPassed = ValidationRuleEntitiesPassed = 0;
            ValidationDataRulesFailed = ValidationRuleEntitiesFailed = 0;
            ValidationErrorCollection = null;
            ImportedDataCollection = null;
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        /// <param name="validationGroup">The valiation group to read the rules for.</param>
        private async Task ReadValidationRulesAsync()
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadDataValidationRules(DataValidationProcess.ExternalBilling, DataValidationGroupName.ExternalData));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
                ValidationErrorCollection = await Task.Run(() => new DataValidationExceptionModel(_eventAggregator).ReadDataValidationExceptions(_billingPeriod, DataValidationProcess.ExternalBilling.Value()));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadValidationRuleExceptionsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the imported external data for the selected billing file from the database
        /// </summary>
        private void ReadImportedExternalDataAsync()
        {
            try
            {
                ImportedDataCollection = new ExternalBillingDataModel(_eventAggregator).ReadExternalBillingData(SelectedExternalData.TableName);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
                bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.ExternalDataValidation));

                // Publish this event to update the billing process history on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateBillingProcessHistoryAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Create a new billing process history entry
        /// </summary>
        /// <param name="billingProcess">The billing process to complete.</param> 
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
                    if (billingProcess == BillingExecutionState.ExternalDataValidation)
                        _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Publish(BillingExecutionState.ExternalDataValidation);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateValidationRuleExceptionsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the external billing data entries from the database
        /// </summary>
        private async Task ReadExternalBillingDataAsync()
        {
            try
            {
                ExternalDataCollection = await Task.Run(() => new ExternalBillingDataModel(_eventAggregator).ReadExternalData(MobileManagerEnvironment.BillingPeriod));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadExternalBillingData",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanStartValidation()
        {
            return BillingProcessCompleted == false && SelectedExternalData != null && SelectedExternalData.pkExternalBillingDataID > 0 ? true : false;
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private async void ExecuteStartValidation()
        {
            string groupDescription = string.Empty;
            InitialiseViewControls();
            ValidationStarted = true;

            try
            {
                // Set the previous data validation process as complete
                await CompleteBillingProcessHistoryAsync(BillingExecutionState.Started);

                // Create a new history entry everytime the process get started
                await CreateBillingProcessHistoryAsync();

                // Update the process progress values on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessEvent>().Publish(BillingExecutionState.ExternalDataValidation);

                // Disable the next buttton when the process gets started
                _eventAggregator.GetEvent<BillingProcessStartedEvent>().Publish(BillingExecutionState.ExternalDataValidation);

                // Read the validation rules for the specified
                // group from the database
                await ReadValidationRulesAsync();

                if (ValidationRuleCollection != null && ValidationRuleCollection.Count > 0)
                {
                    int entityCount = ValidationRuleCollection.GroupBy(p => p.DataValidationEntityID).Count();

                    // Set the entity and data rule progressbars max values
                    ValidationDataRuleCount = ValidationRuleCollection.Count;

                    foreach (DataValidationRule rule in ValidationRuleCollection)
                    {
                        // Allow the user to stop the validation
                        if (!ValidationStarted)
                            break;
                        
                        // Set the data rule progresssbar description
                        ValidationDataRuleDescription = string.Format("Validating data rule {0} - {1} of {2}", rule.PropertyDescription.ToUpper(),
                                                                                                               ++ValidationDataRuleProgress,
                                                                                                               ValidationRuleCollection.Count);

                        // Validate the data rule and update
                        // the progress values accodingly
                        if (await Task.Run(() => _model.ValidateDataValidationRule(rule)))
                            ++ValidationDataRulesPassed;
                        else
                            ++ValidationDataRulesFailed;

                    }
                }

                    //// Allow the user to stop the validation
                    //if (!ValidationStarted)
                    //    break;

                // If NO validations exceptions found the set 
                // the data validation process as complete
                // else save the exceptions to the database
                if (ValidationRuleCollection != null && ValidationErrorCollection != null && ValidationRuleCollection.Count > 0)
                {
                    if (ValidationErrorCollection.Count == 0)
                        await CompleteBillingProcessHistoryAsync(BillingExecutionState.ExternalDataValidation);
                    else
                        await CreateValidationRuleExceptionsAsync();
                }

                // Disable the stop button
                ValidationStarted = false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
            return ValidationStarted && ValidationRuleEntitiesFailed > 0;
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
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
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
                                .Publish(new ApplicationMessage("ViewExtDataValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteApplyRuleFix",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}
