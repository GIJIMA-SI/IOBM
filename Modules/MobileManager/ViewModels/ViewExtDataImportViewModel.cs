using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
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
using System.Windows.Media;
using System.Threading;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewExtDataImportViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private IEventAggregator _eventAggregator;
        private ExternalBillingDataModel _model = null;
        private string _defaultItem = "-- Please Select --";

        #region Commands

        public DelegateCommand StartStopImportCommand { get; set; }
        public DelegateCommand OpenFileCommand { get; set; }
        public DelegateCommand ImportCommand { get; set; }

        #endregion

        #region Properties       
        
        /// <summary>
        /// Enable disable StartStopImport button
        /// </summary>
        public bool CanStartStopImport
        {
            get { return _canStartStopImport; }
            set { SetProperty(ref _canStartStopImport, value); }
        }
        private bool _canStartStopImport = false;

        /// <summary>
        /// The StartStop button tooltip
        /// </summary>
        public string StartStopButtonToolTip
        {
            get { return string.Format("{0} the external data import process", _startStopButtonToolTip); }
            set { SetProperty(ref _startStopButtonToolTip, value); }
        }
        private string _startStopButtonToolTip = "Complete";

        /// <summary>
        /// The StartStop button text
        /// </summary>
        public string StartStopButtonText
        {
            get { return _startStopButtonText; }
            set { SetProperty(ref _startStopButtonText, value); }
        }
        private string _startStopButtonText = "Complete Imports";

        /// <summary>
        /// The StartStop button image
        /// </summary>
        public string StartStopButtonImage
        {
            get { return string.Format("/Gijima.IOBM.MobileManager;component/Assets/Images/{0}", _startStopButtonImage); }
            set { SetProperty(ref _startStopButtonImage, value); }
        }
        private string _startStopButtonImage = "stop_32.ico";

        /// <summary>
        /// The instruction to display on the data import page
        /// </summary>
        public string ImportPageInstruction
        {
            get { return _importPageInstruction; }
            set { SetProperty(ref _importPageInstruction, value); }
        }
        private string _importPageInstruction = string.Empty;

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
        /// The current process history
        /// </summary>
        public BillingProcessHistory CurrentProcessHistory
        {
            get { return _currentProcessHistory; }
            set { SetProperty(ref _currentProcessHistory, value); }
        }
        private BillingProcessHistory _currentProcessHistory = null;

        /// <summary>
        /// The selected company
        /// </summary>
        public Company SelectedCompany
        {
            get { return _selectedCompany; }
            set { SetProperty(ref _selectedCompany, value); }
        }
        private Company _selectedCompany;

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

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of data sheets from the selected Excel file
        /// </summary>
        public ObservableCollection<WorkSheetInfo> DataSheetCollection
        {
            get { return _dataSheetCollection; }
            set { SetProperty(ref _dataSheetCollection, value); }
        }
        private ObservableCollection<WorkSheetInfo> _dataSheetCollection = null;

        /// <summary>
        /// The collection of companies from the database
        /// </summary>
        public ObservableCollection<Company> CompanyCollection
        {
            get { return _companyCollection; }
            set { SetProperty(ref _companyCollection, value); }
        }
        private ObservableCollection<Company> _companyCollection = null;

        /// <summary>
        /// The collection of service providers from the database
        /// </summary>
        public ObservableCollection<ServiceProvider> DataProviderCollection
        {
            get { return _dataProviderCollection; }
            set { SetProperty(ref _dataProviderCollection, value); }
        }
        private ObservableCollection<ServiceProvider> _dataProviderCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected data import file
        /// </summary>
        public OpenFileDialog SelectedImportFile
        {
            get { return _selectedImportFile; }
            set { SetProperty(ref _selectedImportFile, value); }
        }
        private OpenFileDialog _selectedImportFile;

        /// <summary>
        /// The selected data import sheet
        /// </summary>
        public WorkSheetInfo SelectedDataSheet
        {
            get { return _selectedImportSheet; }
            set { SetProperty(ref _selectedImportSheet, value); }
        }
        private WorkSheetInfo _selectedImportSheet;

        /// <summary>
        /// The selected destination entity to import to
        /// </summary>
        public ServiceProvider SelectedDataProvider
        {
            get { return _selectedDataProvider; }
            set { SetProperty(ref _selectedDataProvider, value); }
        }
        private ServiceProvider _selectedDataProvider;

        #endregion

        #region Input Validation  

        /// <summary>
        /// Check if a valid data file was selected
        /// </summary>
        public bool ValidDataFile
        {
            get { return _validDataFile; }
            set { SetProperty(ref _validDataFile, value); }
        }
        private bool _validDataFile = false;

        /// <summary>
        /// Check if the data import was valid
        /// </summary>
        public bool ValidImportData
        {
            get { return _validDataImport; }
            set { SetProperty(ref _validDataImport, value); }
        }
        private bool _validDataImport = false;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidImportFile
        {
            get { return _validImportFile; }
            set { SetProperty(ref _validImportFile, value); }
        }
        private Brush _validImportFile = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataSheet
        {
            get { return _validDataSheet; }
            set { SetProperty(ref _validDataSheet, value); }
        }
        private Brush _validDataSheet = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataProvider
        {
            get { return _validDataProvider; }
            set { SetProperty(ref _validDataProvider, value); }
        }
        private Brush _validDataProvider = Brushes.Red;

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
                    case "SelectedImportFile":
                        ValidImportFile = string.IsNullOrEmpty(SelectedImportFile.SafeFileName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataSheet":
                        ValidDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == _defaultItem ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataProvider":
                        ValidDataProvider = SelectedDataProvider != null && SelectedDataProvider.pkServiceProviderID > 0 ? Brushes.Silver : Brushes.Red; break;
                }

                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event gets received to disable the next button 
        /// when the process is not completed
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingCurrentHistory_Event(object sender)
        {
            CurrentProcessHistory = (BillingProcessHistory)sender;
            CanStartStopImport = true;

            if ((BillingExecutionState)CurrentProcessHistory.fkBillingProcessID == BillingExecutionState.InternalDataValidation &&
                CurrentProcessHistory.ProcessResult != null)
            {
                StartStopButtonToolTip = "Start";
                StartStopButtonText = "Start Imports";
                StartStopButtonImage = "062.png";
            }
            else
            {
                CanStartStopImport = false;
                StartStopButtonToolTip = "Complete";
                StartStopButtonText = "Complete Imports";
                StartStopButtonImage = "stop_32.ico";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewExtDataImportViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _model = new ExternalBillingDataModel(eventAggregator);
            InitialiseDataImportView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDataImportView()
        {
            InitialiseViewControls();
            
            // Initialise the view commands
            StartStopImportCommand = new DelegateCommand(ExecuteStartStopImport, CanExecuteStartStopImport).ObservesProperty(() => CurrentProcessHistory);
            OpenFileCommand = new DelegateCommand(ExecuteOpenFileCommand, CanExecuteOpenFile).ObservesProperty(() => CanStartStopImport);
            ImportCommand = new DelegateCommand(ExecuteImport, CanExecuteImport).ObservesProperty(() => ValidDataSheet)
                                                                                .ObservesProperty(() => SelectedDataProvider);

            // Subscribe to this event to disable functionality start the billing process if its already started
            _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Subscribe(BillingCurrentHistory_Event, true);

            await ReadCompaniesAsync();
            await ReadServiceProvidersAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ValidDataFile = false;
            ImportPageInstruction = "Please start the process to import all the external billing data, NOTE Ensure that all the data have been imported before you complete the process.";

            // Add the default items
            DataSheetCollection = new ObservableCollection<WorkSheetInfo>();
            WorkSheetInfo defaultInfo = new WorkSheetInfo();
            defaultInfo.SheetName = _defaultItem;
            SelectedDataSheet = defaultInfo;
            DataSheetCollection.Add(defaultInfo);
            InitialiseImportControls();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseImportControls()
        {
            ImportUpdateDescription = string.Format("Importing - {0} of {1}", 0, 0);
            ImportUpdateCount = 1;
            ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
            ValidImportData = false;
            ImportedDataCollection = null;
        }

        /// <summary>
        /// Import the data from the selected workbook sheet
        /// </summary>
        private void ImportWorkSheetDataAsync()
        {
            try
            {
                MSOfficeHelper officeHelper = new MSOfficeHelper();
                InitialiseImportControls();

                if (SelectedDataSheet != null && SelectedDataSheet.WorkBookName != null)
                {
                    DataTable sheetData = null;
                    ImportUpdateDescription = string.Format("Importing - {0} of {1}", 0, 0);
                    ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
                    ImportUpdateCount = SelectedDataSheet.RowCount;
                    string sqlTableName = string.Format("External{0}", SelectedDataProvider.ServiceProviderName.ToUpper().Replace(" ", "").Replace("-", "").Trim());

                    // Add the company name to the sql table name if a company was selected
                    if (SelectedCompany != null && SelectedCompany.pkCompanyID > 0)
                        sqlTableName = string.Format("External{0}_{1}", SelectedDataProvider.ServiceProviderName.ToUpper().Replace(" ", "").Replace("-", "").Trim(),
                                                                        SelectedCompany.CompanyName.ToUpper().Replace(" ", "").Trim());

                    // Update the worksheet data
                    sheetData = officeHelper.ReadSheetDataIntoDataTable(SelectedDataSheet.WorkBookName, SelectedDataSheet.SheetName);

                    // This is to fake the progress bar for importing
                    for (int i = 1; i <= SelectedDataSheet.RowCount; i++)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ImportUpdateProgress = i;
                            ImportUpdatesPassed = i;
                            ImportUpdateDescription = string.Format("Importing - {0} of {1}", ImportUpdateProgress, SelectedDataSheet.RowCount);

                            if (i % 2 == 0)
                                Thread.Sleep(1);
                        });
                    }

                    ImportedDataCollection = sheetData;

                    // Write the imported data to the database
                    ExternalBillingData extData = new ExternalBillingData();
                    extData.TableName = sqlTableName;
                    extData.SheetName = SelectedDataSheet.SheetName;
                    extData.BillingPeriod = MobileManagerEnvironment.BillingPeriod;
                    extData.PropertyValidationPassed = false;
                    extData.DataValidationPassed = false;
                    extData.DataFileLocation = Path.GetDirectoryName(SelectedImportFile.FileName);
                    extData.DataFileName = SelectedImportFile.SafeFileName;
                    extData.ModifiedBy = SecurityHelper.LoggedInUserFullName;
                    extData.DateModified = DateTime.Now;

                    if (!_model.CreateExternalData(sqlTableName, extData, sheetData))
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("ViewDataImportExtViewModel",
                                                 "The imported data did not save.",
                                                 "ExecuteImport",
                                                 ApplicationMessage.MessageTypes.ProcessError));
                    }
                }
            }
            catch (Exception ex)
            {
                if (ExceptionsCollection == null)
                    ExceptionsCollection = new ObservableCollection<string>();

                ++ImportUpdatesFailed;
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportExtViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteImport",
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
                bool result = new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.ExternalDataImport);

                // Publish this event to update the billing process history on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);
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
        /// Create a new billing process history entry
        /// </summary>
        /// <param name="billingProcess">The billing process to complete.</param>/// 
        private void CompleteBillingProcessHistory(BillingExecutionState billingProcess)
        {
            try
            {
                bool result = new BillingProcessModel(_eventAggregator).CompleteBillingProcessHistory(billingProcess, true);

                if (result)
                    // Publish this event to update the billing process history on the wizard's Info content
                    _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);
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

        #region Lookup Data Loading

        /// <summary>
        /// Load all the companies from the database
        /// </summary>
        private async Task ReadCompaniesAsync()
        {
            try
            {
                CompanyCollection = await Task.Run(() => new CompanyModel(_eventAggregator).ReadCompanies(true, false));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportExtViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompaniesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the service providers from the database
        /// </summary>
        private async Task ReadServiceProvidersAsync()
        {
            try
            {
                DataProviderCollection = await Task.Run(() => new ServiceProviderModel(_eventAggregator).ReadServiceProviders(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportExtViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadServiceProvidersAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteStartStopImport()
        {
            return (CurrentProcessHistory != null &&
                    (BillingExecutionState)CurrentProcessHistory.fkBillingProcessID == BillingExecutionState.InternalDataValidation &&
                   CurrentProcessHistory.ProcessResult != null) ||
                   (CurrentProcessHistory != null && 
                   (BillingExecutionState)CurrentProcessHistory.fkBillingProcessID == BillingExecutionState.ExternalDataImport &&
                   CurrentProcessHistory.ProcessResult == null);
        }

        /// <summary>
        /// Execute when the start stop command button is clicked 
        /// </summary>
        private void ExecuteStartStopImport()
        {
            try
            {
                // Create or complete the process
                if (CanStartStopImport)
                    CreateBillingProcessHistory();
                else
                    CompleteBillingProcessHistory(BillingExecutionState.ExternalDataImport);

                // Get the new current process history
                BillingProcessHistory currentProcess = new BillingProcessModel(_eventAggregator).ReadBillingProcessCurrentHistory();

                // Update the process progress values on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessEvent>().Publish(BillingExecutionState.ExternalDataImport);

                // Change the start buttton to complete when the process gets started
                _eventAggregator.GetEvent<BillingCurrentHistoryEvent>().Publish(currentProcess);
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
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteOpenFile()
        {
            return !CanStartStopImport && CurrentProcessHistory != null && CurrentProcessHistory.ProcessResult == null;
        }

        /// <summary>
        /// Execute when the open file command button is clicked 
        /// </summary>
        private void ExecuteOpenFileCommand()
        {
            try
            {
                InitialiseDataImportView();
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                ValidImportData = false;

                if (result.ToString() == "OK")
                {
                    MSOfficeHelper officeHelper = new MSOfficeHelper();
                    SelectedImportFile = dialog;

                    // Get all the excel workbook sheets
                    List<WorkSheetInfo> workSheets = officeHelper.ReadWorkBookInfoFromExcel(dialog.FileName).ToList();

                    // Add all the workbook sheets
                    foreach (WorkSheetInfo sheetInfo in workSheets)
                    {
                        DataSheetCollection.Add(sheetInfo);
                    }

                    SelectedDataSheet = DataSheetCollection[0];
                    ValidDataFile = true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                .Publish(new ApplicationMessage("ViewDataImportExtViewModel",
                                                string.Format("Error! {0}, {1}.",
                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                "ExecuteOpenFileCommand",
                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteImport()
        {
            return ValidDataSheet == Brushes.Silver && SelectedDataProvider != null && SelectedDataProvider.pkServiceProviderID > 0;
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private void ExecuteImport()
        {
            try
            {
                Task.Run(() => ImportWorkSheetDataAsync());
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportExtViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteImport",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}

