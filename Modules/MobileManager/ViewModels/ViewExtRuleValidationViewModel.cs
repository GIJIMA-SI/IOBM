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
    public class ViewExtRuleValidationViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private BillingProcessHistory _currentProcessHistory = null;
        private string _billingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year);

        #region Commands

        public DelegateCommand StartValidationCommand { get; set; }
        public DelegateCommand StopValidationCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }

        #endregion

        #region Properties

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
        /// Indicate if the billing run can be started
        /// </summary>
        public bool CanStartBillingProcess
        {
            get { return _canStartBillingProcess; }
            set { SetProperty(ref _canStartBillingProcess, value); }
        }
        private bool _canStartBillingProcess = false;

        /// <summary>
        /// The validation data file progessbar description
        /// </summary>
        public string ValidationDataFileDescription
        {
            get { return _validationDataFileDescription; }
            set { SetProperty(ref _validationDataFileDescription, value); }
        }
        private string _validationDataFileDescription;

        /// <summary>
        /// The validation data file progessbar value
        /// </summary>
        public int ValidationDataFileProgress
        {
            get { return _validationDataFileProgress; }
            set { SetProperty(ref _validationDataFileProgress, value); }
        }
        private int _validationDataFileProgress;

        /// <summary>
        /// The validation rule progessbar description
        /// </summary>
        public string ValidationRuleDescription
        {
            get { return _validationRuleDescription; }
            set { SetProperty(ref _validationRuleDescription, value); }
        }
        private string _validationRuleDescription;

        /// <summary>
        /// The validation rule progessbar value
        /// </summary>
        public int ValidationRuleProgress
        {
            get { return _validationRuleProgress; }
            set { SetProperty(ref _validationRuleProgress, value); }
        }
        private int _validationRuleProgress;

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
        /// The number of validation data files
        /// </summary>
        public int ValidationDataFileCount
        {
            get { return _validationDataFileCount; }
            set { SetProperty(ref _validationDataFileCount, value); }
        }
        private int _validationDataFileCount;

        /// <summary>
        /// The number of validation rules
        /// </summary>
        public int ValidationRuleCount
        {
            get { return _validationRuleCount; }
            set { SetProperty(ref _validationRuleCount, value); }
        }
        private int _validationRuleCount;

        /// <summary>
        /// The number of validation data files that passed validation
        /// </summary>
        public int ValidationDataFilesPassed
        {
            get { return _validationDataFilesPassed; }
            set { SetProperty(ref _validationDataFilesPassed, value); }
        }
        private int _validationDataFilesPassed;

        /// <summary>
        /// The number of validation data files that failed validation
        /// </summary>
        public int ValidationDataFilesFailed
        {
            get { return _validationDataFilesFailed; }
            set { SetProperty(ref _validationDataFilesFailed, value); }
        }
        private int _validationDataFilesFailed;

        /// <summary>
        /// The number of validation data rules that passed validation
        /// </summary>
        public int ValidationRulesPassed
        {
            get { return _validationRulesPassed; }
            set { SetProperty(ref _validationRulesPassed, value); }
        }
        private int _validationRulesPassed;

        /// <summary>
        /// The number of validation data rules that failed validation
        /// </summary>
        public int ValidationRulesFailed
        {
            get { return _validationRulesFailed; }
            set { SetProperty(ref _validationRulesFailed, value); }
        }
        private int _validationRulesFailed;

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
        /// The collection of external billing entries
        /// </summary>
        public ObservableCollection<ExternalBillingData> ExternalDataCollection
        {
            get { return _externalDataCollection; }
            set { SetProperty(ref _externalDataCollection, value); }
        }
        private ObservableCollection<ExternalBillingData> _externalDataCollection = null;

        /// <summary>
        /// The collection of validation properties from the database
        /// </summary>
        public ObservableCollection<DataValidationProperty> ValidationPropertyCollection
        {
            get { return _validationPropertyCollection; }
            set { SetProperty(ref _validationPropertyCollection, value); }
        }
        private ObservableCollection<DataValidationProperty> _validationPropertyCollection = null;

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
        /// This event gets received to disable the next button 
        /// when the process is not completed
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingCurrentHistory_Event(object sender)
        {
            _currentProcessHistory = (BillingProcessHistory)sender;

            if ((BillingExecutionState)_currentProcessHistory.fkBillingProcessID == BillingExecutionState.ExternalDataRuleValidation &&
                _currentProcessHistory.ProcessResult != null)
                CanStartBillingProcess = false;
            else
                CanStartBillingProcess = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewExtRuleValidationViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseExtRuleValidationView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseExtRuleValidationView()
        {
            _model = new DataValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands 
            StartValidationCommand = new DelegateCommand(ExecuteStartValidation, CanStartValidation).ObservesProperty(() => ExternalDataCollection)
                                                                                                    .ObservesProperty(() => CanStartBillingProcess);
            StopValidationCommand = new DelegateCommand(ExecuteStopValidation, CanStopValidation);
            ExportCommand = new DelegateCommand(ExecuteExport, CanExport).ObservesProperty(() => ExceptionsToFix);

            // Subscribe to this event to update the progressbar
            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Subscribe(ProgressBarInfo_Event, true);

            // Subscribe to this event to display the data validation errors
            _eventAggregator.GetEvent<DataValiationResultEvent>().Subscribe(DataValiationResult_Event, true);

            // Subscribe to this event to disable functionality start the billing process if its already started
            _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Subscribe(BillingCurrentHistory_Event, true);

            // Load the view data
            await ReadExternalBillingDataAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ValidationPageInstruction = "Please start the process to validate the external billing data rules based on the external data file properties.";
            ValidationDataFileDescription = string.Format("Validating data file - {0} of {1}", 0, 0);
            ValidationRuleDescription = string.Format("Validating data rule - {0} of {1}", 0, 0);
            ValidationDataFileProgress = ValidationRuleProgress = 0;
            ValidationDataFileCount = ValidationRuleCount = 0;
            ValidationDataFilesPassed = ValidationRulesPassed = 0;
            ValidationDataFilesFailed = ValidationRulesFailed = 0;
            ValidationErrorCollection = null;
        }

        /// <summary>
        /// Load all the external billing data entries from the database
        /// </summary>
        private async Task ReadExternalBillingDataAsync()
        {
            try
            {
                ExternalDataCollection = await Task.Run(() => new ExternalBillingDataModel(_eventAggregator).ReadExternalData(MobileManagerEnvironment.BillingPeriod, false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewExtRuleValidationViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadExternalBillingData",
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
                    ValidationRulesPassed++;
                }
                else
                {
                    ValidationRulesFailed++;
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
                ValidationRuleCount = progressInfo.MaxValue;
                ValidationRuleProgress = progressInfo.CurrentValue;
                ValidationRuleDescription = string.Format("Validating data rule - {0} of {1}", ValidationRuleProgress,
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
        private void CreateBillingProcessHistory()
        {
            try
            {
                bool result = new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.ExternalDataRuleValidation);

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
        private void CompleteBillingProcessHistory(BillingExecutionState billingProcess)
        {
            try
            {
                bool result = new BillingProcessModel(_eventAggregator).CompleteBillingProcessHistory(billingProcess, true);

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
        private void CreateValidationRuleExceptions()
        {
            try
            {
                bool result = new DataValidationExceptionModel(_eventAggregator).CreateDataValidationExceptions(ValidationErrorCollection);
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

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanStartValidation()
        {
            return ExternalDataCollection != null && ExternalDataCollection.Count > 0 && CanStartBillingProcess;
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private async void ExecuteStartValidation()
        {
            string groupDescription = string.Empty;
            InitialiseViewControls();
            ValidationStarted = true;
            string dataFileName = string.Empty;

            try
            {
                // Create a new history entry everytime the process get started
                CreateBillingProcessHistory();

                // Get the new current process history
                BillingProcessHistory currentProcess = new BillingProcessModel(_eventAggregator).ReadBillingProcessCurrentHistory();

                // Update the process progress values on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessEvent>().Publish(BillingExecutionState.ExternalDataRuleValidation);

                // Change the start buttton to complete when the process gets started
                _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Publish(currentProcess);

                if (ExternalDataCollection != null && ExternalDataCollection.Count > 0)
                {
                    // Set the entity and data rule progressbars max values
                    ValidationDataFileCount = ExternalDataCollection.Count;

                    foreach (ExternalBillingData dataFile in ExternalDataCollection)
                    {
                        dataFileName = dataFile.TableName.ToUpper();

                        // Allow the user to stop the validation
                        if (!ValidationStarted)
                            break;

                        // Set the data rule progresssbar description
                        ValidationDataFileDescription = string.Format("Validating data file {0} - {1} of {2}", dataFileName,
                                                                                                               ++ValidationDataFileProgress,
                                                                                                               ExternalDataCollection.Count);

                        // Validate the data rule and update
                        // the progress values accodingly
                        if (await Task.Run(() => new DataValidationPropertyModel(_eventAggregator).ValidateExtDataValidationRules(dataFile)))
                            ++ValidationDataFilesPassed;
                        else
                            ++ValidationDataFilesFailed;

                        // Allow the user to stop the validation
                        if (!ValidationStarted)
                            break;
                    }
                }

                // If NO validations exceptions found the set 
                // the data validation process as complete
                // else save the exceptions to the database
                if (ExternalDataCollection != null && ValidationErrorCollection != null && ExternalDataCollection.Count > 0)
                {
                    if (ValidationErrorCollection.Count == 0)
                        CompleteBillingProcessHistory(BillingExecutionState.ExternalDataRuleValidation);
                    else
                        CreateValidationRuleExceptions();
                }

                // Disable the stop button
                ValidationStarted = false;

                // Change the start buttton to complete when the process gets started
                _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Publish(currentProcess);
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
            return ValidationStarted && ValidationRulesFailed > 0;
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

        #endregion

        #endregion
    }
}

