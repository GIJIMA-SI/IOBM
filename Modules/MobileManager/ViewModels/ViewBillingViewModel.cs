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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewBillingViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private BillingProcessModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;

        #region Commands

        public DelegateCommand AcceptCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand BackCommand { get; set; }
        public DelegateCommand FinishCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The selected billing period
        /// </summary>
        public string SelectedBillingPeriod
        {
            get { return _selectedBillingPeriod; }
            set { SetProperty(ref _selectedBillingPeriod, value); }
        }
        private string _selectedBillingPeriod = MobileManagerEnvironment.BillingPeriod;

        /// <summary>
        /// The number of pages in the billing wizard
        /// </summary>
        public int BillingWizardPageCount
        {
            get { return _billingWizardPageCount; }
            set { SetProperty(ref _billingWizardPageCount, value); }
        }
        private int _billingWizardPageCount = 1;

        /// <summary>
        /// The number of selected billing processes
        /// </summary>
        public int BillingProcessCount
        {
            get { return _billingProcessCount; }
            set { SetProperty(ref _billingProcessCount, value); }
        }
        private int _billingProcessCount = 0;

        /// <summary>
        /// The number of pages in the billing wizard
        /// </summary>
        public int BillingWizardProgress
        {
            get { return _billingWizardProgress; }
            set { SetProperty(ref _billingWizardProgress, value); }
        }
        private int _billingWizardProgress = 1;

        /// <summary>
        /// The number of selected billing processes
        /// </summary>
        public int BillingProcessProgress
        {
            get { return _billingProcessProgress; }
            set { SetProperty(ref _billingProcessProgress, value); }
        }
        private int _billingProcessProgress = 0;

        /// <summary>
        /// The billing wizard progress description
        /// </summary>
        public string BillingWizardDescription
        {
            get { return _billingWizardDescription; }
            set { SetProperty(ref _billingWizardDescription, value); }
        }
        private string _billingWizardDescription = string.Format("Billing step - {0} of {1}", 1, 1);

        /// <summary>
        /// The billing wizard progress description
        /// </summary>
        public string BillinProcessDescription
        {
            get { return _billinProcessDescription; }
            set { SetProperty(ref _billinProcessDescription, value); }
        }
        private string _billinProcessDescription = string.Format("Executing Process - {0} of {1}", 0, 0);

        /// <summary>
        /// The page description to display on billing wizard page
        /// </summary>
        public string BillingWizardPageDescription
        {
            get { return _billingWizardPageDescription; }
            set { SetProperty(ref _billingWizardPageDescription, value); }
        }
        private string _billingWizardPageDescription = string.Empty;

        /// <summary>
        /// The instruction to display on billing wizard page
        /// </summary>
        public string BillingWizardPageInstruction
        {
            get { return _billingWizardPageInstruction; }
            set { SetProperty(ref _billingWizardPageInstruction, value); }
        }
        private string _billingWizardPageInstruction = string.Empty;

        /// <summary>
        /// The selected billing month
        /// </summary>
        public int SelectedBillingMonth
        {
            get { return _selectedBillingMonth; }
            set { SetProperty(ref _selectedBillingMonth, value); }
        }
        private int _selectedBillingMonth;

        /// <summary>
        /// The selected billing year
        /// </summary>
        public int SelectedBillingYear
        {
            get { return _selectedBillingYear; }
            set { SetProperty(ref _selectedBillingYear, value); }
        }
        private int _selectedBillingYear;

        /// <summary>
        /// Indicate if the billing run started
        /// </summary>
        public bool BillingRunStarted
        {
            get { return _billingRunStarted; }
            set { SetProperty(ref _billingRunStarted, value); }
        }
        private bool _billingRunStarted = false;

        /// <summary>
        /// Indicate if the billing start process completed
        /// </summary>
        public bool StartProcessCompleted
        {
            get { return _startProcessCompleted; }
            set { SetProperty(ref _startProcessCompleted, value); }
        }
        private bool _startProcessCompleted = false;

        /// <summary>
        /// Indicate if the billing data validation process completed
        /// </summary>
        public bool DataValidationProcessCompleted
        {
            get { return _dataValidationProcessCompleted; }
            set { SetProperty(ref _dataValidationProcessCompleted, value); }
        }
        private bool _dataValidationProcessCompleted = false;

        /// <summary>
        /// The collection of billing process history from the database
        /// </summary>
        public ObservableCollection<BillingProcessHistory> ProcessHistoryCollection
        {
            get { return _processHistoryCollection; }
            set { SetProperty(ref _processHistoryCollection, value); }
        }
        private ObservableCollection<BillingProcessHistory> _processHistoryCollection = null;

        /// <summary>
        /// The collection of billing processes from the database
        /// </summary>
        public ObservableCollection<BillingProcess> BillingProcessCollection
        {
            get { return _billingProcessCollection; }
            set { SetProperty(ref _billingProcessCollection, value); }
        }
        private ObservableCollection<BillingProcess> _billingProcessCollection = null;

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
        /// This event gets received when a process gets started
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingProcess_Event(BillingExecutionState sender)
        {
            BillingProcessProgress = sender.Value();
            BillinProcessDescription = string.Format("Executing Process - {0} of {1}", BillingProcessProgress, BillingProcessCount);
        }

        /// <summary>
        /// This event gets received to disable the next button 
        /// when the process completed
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingProcessStarted_Event(BillingExecutionState sender)
        {
            switch (sender)
            {
                case BillingExecutionState.StartBillingProcess:
                    Application.Current.Dispatcher.Invoke(() => { BillingRunStarted = true; StartProcessCompleted = false; }); break;
                case BillingExecutionState.InternalDataValidation:
                    Application.Current.Dispatcher.Invoke(() => { DataValidationProcessCompleted = false; }); break;

            }
        }

        /// <summary>
        /// This event gets received to enable the next button 
        /// when the process completed
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingProcessCompleted_Event(BillingExecutionState sender)
        {
            switch (sender)
            {
                case BillingExecutionState.StartBillingProcess:
                    Application.Current.Dispatcher.Invoke(() => { BillingRunStarted = StartProcessCompleted = true; }); break;
                case BillingExecutionState.InternalDataValidation:
                    Application.Current.Dispatcher.Invoke(() => { BillingRunStarted = DataValidationProcessCompleted = true; }); break;
                case BillingExecutionState.ExternalDataValidation:
                    Application.Current.Dispatcher.Invoke(() => { BillingRunStarted = DataValidationProcessCompleted = true; }); break;
            }
        }

        /// <summary>
        /// This event gets received to update the billing process history
        /// </summary>
        /// <param name="sender">Indicate if history was created.</param>
        private async void BillingProcessHistory_Event(bool sender)
        {
            if (sender)
                await ReadBillingProcessHistoryAsync();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewBillingViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _model = new BillingProcessModel(eventAggregator);
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseBillingView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseBillingView()
        {
            //_model = new ValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            AcceptCommand = new DelegateCommand(ExecuteAccept, CanExecuteAccept).ObservesProperty(() => BillingRunStarted)
                                                                                .ObservesProperty(() => StartProcessCompleted);
            NextCommand = new DelegateCommand(ExecuteNextPage); 
            BackCommand = new DelegateCommand(ExecutePreviousPage);

            // Subscribe to this event to update the process progress values
            _eventAggregator.GetEvent<BillingProcessEvent>().Subscribe(BillingProcess_Event, true);

            // Subscribe to this event to lock the completed process
            // and enable functionality to move to the process completed
            _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Subscribe(BillingProcessCompleted_Event, true);

            // Subscribe to this event to lock the started process
            // and disable functionality to move to the process completed
            _eventAggregator.GetEvent<BillingProcessStartedEvent>().Subscribe(BillingProcessStarted_Event, true);

            // Subscribe to this event to update the billing process history on the wizard's Info content
            _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Subscribe(BillingProcessHistory_Event, true);

            // Load the view data
            await ReadBillingProcessesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            BillingWizardPageCount = 4;
            BillingWizardProgress = 1;
            BillingProcessProgress = 0;
            SelectedBillingMonth = Convert.ToInt32(MobileManagerEnvironment.BillingPeriod.Substring(5, 2));
            SelectedBillingYear =  Convert.ToInt32(MobileManagerEnvironment.BillingPeriod.Substring(0, 4));
            BillingWizardDescription = string.Format("Billing step - {0} of {1}", 1, BillingWizardPageCount);
            BillingRunStarted = false;
        }

        /// <summary>
        /// Load all the billing processes from the database
        /// </summary>
        private async Task ReadBillingProcessesAsync()
        {
            try
            {
                BillingProcessCollection = await Task.Run(() => _model.ReadBillingProcesses());
                BillingProcessCount = BillingProcessCollection.Count;

                // Read the process history
                await ReadBillingProcessHistoryAsync();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewBillingViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadBillingProcessesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the billing process history from the database
        /// </summary>
        private async Task ReadBillingProcessHistoryAsync()
        {
            try
            {
                ProcessHistoryCollection = await Task.Run(() => _model.ReadBillingProcessHistory());

                // Get the current process history entity
                if (ProcessHistoryCollection != null && ProcessHistoryCollection.Count > 0)
                {
                    BillingProcessHistory currentProcessHistory = ProcessHistoryCollection.Where(p => p.ProcessCurrent).FirstOrDefault();

                    BillingProcessProgress = currentProcessHistory.fkBillingProcessID;
                    BillinProcessDescription = string.Format("Executing Process - {0} of {1}", BillingProcessProgress, BillingProcessCount);

                    // Lock all the completed processes
                    if (currentProcessHistory != null)
                        await Task.Run(() => SetCompletedBillingProcessesAsync(currentProcessHistory));
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewBillingViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadBillingProcessHistoryAsync",
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
                bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.StartBillingProcess));

                // Publish this event to update the billing process history on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewBillingViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateBillingProcessHistoryAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set all the billing processes already completed as completed
        /// </summary>
        /// <param name="currentProcessHistory">The current process history entity.</param>
        private void SetCompletedBillingProcessesAsync(BillingProcessHistory currentProcessHistory)
        {
            try
            {
                foreach (BillingProcess process in BillingProcessCollection)
                {
                    if (process.pkBillingProcessID < currentProcessHistory.fkBillingProcessID ||
                        (process.pkBillingProcessID == currentProcessHistory.fkBillingProcessID &&
                         currentProcessHistory.ProcessEndDate != null && currentProcessHistory.ProcessResult == true))
                    {
                        // Publish this event to lock the completed process and enable functinality to move to the next process
                        _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Publish((BillingExecutionState)process.pkBillingProcessID);
                    }
                    else if (process.pkBillingProcessID == currentProcessHistory.fkBillingProcessID &&
                             currentProcessHistory.ProcessEndDate == null && currentProcessHistory.ProcessResult == null)
                    {
                        // Publish this event to lock the started process and disable functinality to move to the next process
                        _eventAggregator.GetEvent<BillingProcessStartedEvent>().Publish((BillingExecutionState)process.pkBillingProcessID);
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewBillingViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "SetCompletedBillingProcessesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        #endregion

        #region Command Execution

        /// <summary>
        /// Execute when the next command button is clicked 
        /// </summary>
        private void ExecuteNextPage()
        {
            BillingWizardProgress++;
            BillingWizardDescription = string.Format("Billing step - {0} of {1}", BillingWizardProgress, BillingWizardPageCount);

            // Publish this event to show the data validation control header
            _eventAggregator.GetEvent<DataValiationHeaderEvent>().Publish(false);
        }

        /// <summary>
        /// Execute when the back command button is clicked 
        /// </summary>
        private void ExecutePreviousPage()
        {
            --BillingWizardProgress;
            BillingWizardDescription = string.Format("Billing step - {0} of {1}", BillingWizardProgress, BillingWizardPageCount);
        }

        /// <summary>
        /// Validate is the accecpt button is avaliable 
        /// </summary>
        /// <returns>True if valid</returns>
        private bool CanExecuteAccept()
        {
            return BillingRunStarted || StartProcessCompleted ? false : true;
        }

        /// <summary>
        /// Execute when the accept command button is clicked 
        /// </summary>
        private async void ExecuteAccept()
        {
            try
            {
                BillingRunStarted = true;
                SelectedBillingPeriod = string.Format("{0}/{1}", SelectedBillingYear, SelectedBillingMonth.ToString().PadLeft(2, '0'));

                // Create a new history entry everytime the process get started
                await CreateBillingProcessHistoryAsync();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                .Publish(new ApplicationMessage("ViewBillingViewModel",
                                                string.Format("Error! {0}, {1}.",
                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                "ExecuteAccept",
                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}
