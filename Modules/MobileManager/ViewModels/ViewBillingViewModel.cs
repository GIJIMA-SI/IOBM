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
        private BillingProcessHistory _currentProcessHistory = null;

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
        /// Indicate if the billing run can be started
        /// </summary>
        public bool CanStartBillingProcess
        {
            get { return _canStartBillingProcess; }
            set { SetProperty(ref _canStartBillingProcess, value); }
        }
        private bool _canStartBillingProcess = false;

        /// <summary>
        /// Indicate if the billing run started
        /// </summary>
        public bool CanMoveNext
        {
            get { return _canMoveNext; }
            set { SetProperty(ref _canMoveNext, value); }
        }
        private bool _canMoveNext = false;

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
        /// This event gets received to disable the stratt button 
        /// when the process is already started
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingCurrentHistory_Event(object sender)
        {
            CanStartBillingProcess = false;

            if ((BillingExecutionState)_currentProcessHistory.fkBillingProcessID == BillingExecutionState.CloseBillingProcess &&
                _currentProcessHistory.ProcessEndDate != null && _currentProcessHistory.ProcessResult != null)
                CanStartBillingProcess = true;

            // Only allow to move next page if the current page process has been completed
            if ((BillingExecutionState)_currentProcessHistory.fkBillingProcessID == BillingExecutionState.CloseBillingProcess ||
                ((BillingExecutionState)_currentProcessHistory.fkBillingProcessID != BillingExecutionState.StartBillingProcess &&
                 _currentProcessHistory.fkBillingProcessID == BillingWizardProgress && _currentProcessHistory.ProcessResult == null) ||
                BillingWizardProgress > BillingProcessProgress)
                CanMoveNext = false;
            else
                CanMoveNext = true;
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
            AcceptCommand = new DelegateCommand(ExecuteAccept, CanExecuteAccept).ObservesProperty(() => CanStartBillingProcess);
            NextCommand = new DelegateCommand(ExecuteNextPage); 
            BackCommand = new DelegateCommand(ExecutePreviousPage);

            // Subscribe to this event to update the process progress values
            _eventAggregator.GetEvent<BillingProcessEvent>().Subscribe(BillingProcess_Event, true);

            // Subscribe to this event to disable functionality start the billing process if its already started
            _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Subscribe(BillingCurrentHistory_Event, true);        

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
            BillingWizardProgress = 1;
            BillingProcessProgress = 0;
            SelectedBillingPeriod = MobileManagerEnvironment.BillingPeriod;
            SelectedBillingMonth = Convert.ToInt32(MobileManagerEnvironment.BillingPeriod.Substring(5, 2));
            SelectedBillingYear =  Convert.ToInt32(MobileManagerEnvironment.BillingPeriod.Substring(0, 4));
        }

        /// <summary>
        /// Load all the billing processes from the database
        /// </summary>
        private async Task ReadBillingProcessesAsync()
        {
            try
            {
                BillingProcessCollection = await Task.Run(() => _model.ReadBillingProcesses());
                BillingProcessCount = BillingWizardPageCount = BillingProcessCollection.Count;
                BillingWizardDescription = string.Format("Billing step - {0} of {1}", 1, BillingWizardPageCount);

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
                    _currentProcessHistory = ProcessHistoryCollection.Where(p => p.ProcessCurrent).FirstOrDefault();

                    BillingProcessProgress = _currentProcessHistory.fkBillingProcessID;
                    BillinProcessDescription = string.Format("Executing Process - {0} of {1}", BillingProcessProgress, BillingProcessCount);

                    // Set the move next page button
                    if ((BillingExecutionState)_currentProcessHistory.fkBillingProcessID == BillingExecutionState.CloseBillingProcess)
                        CanMoveNext = false;
                    else
                        CanMoveNext = true;

                    // Publish this event to lock the started process
                    if (_currentProcessHistory != null)
                        _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Publish(_currentProcessHistory);
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
        private async Task CreateNewBillingProcessHistoryAsync()
        {
            try
            {
                await Task.Run(() => new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.StartBillingProcess));
                InitialiseBillingView();

                // Publish the event to update the billing period on the UI
                _eventAggregator.GetEvent<BillingPeriodEvent>().Publish(null);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewBillingViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateNewBillingProcessHistoryAsync",
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
            CanMoveNext = false;

            // Publish this event to show the data validation control header
            _eventAggregator.GetEvent<DataValiationHeaderEvent>().Publish(false);

            // Publish this event to lock the started process
            if (_currentProcessHistory != null)
                _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Publish(_currentProcessHistory);
        }

        /// <summary>
        /// Execute when the back command button is clicked 
        /// </summary>
        private void ExecutePreviousPage()
        {
            --BillingWizardProgress;
            BillingWizardDescription = string.Format("Billing step - {0} of {1}", BillingWizardProgress, BillingWizardPageCount);
            CanMoveNext = true;
        }

        /// <summary>
        /// Validate is the accecpt button is avaliable 
        /// </summary>
        /// <returns>True if valid</returns>
        private bool CanExecuteAccept()
        {
            return CanStartBillingProcess ? true : false;
        }

        /// <summary>
        /// Execute when the accept command button is clicked 
        /// </summary>
        private async void ExecuteAccept()
        {
            try
            {
                // Create a new history entry everytime the process get started
                await CreateNewBillingProcessHistoryAsync();

                // Publish this event to lock the started process and disable functinality to move to the next process
                CanStartBillingProcess = CanMoveNext = false;
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
