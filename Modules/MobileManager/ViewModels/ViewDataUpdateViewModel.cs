using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Helpers;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataUpdateViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataUpdateRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private IEnumerable<DataUpdateRule> _importRules = null;
        private DataUpdateRule _importRule = null;
        private MSOfficeHelper _officeHelper = null;

        #region Commands

        public DelegateCommand OpenFileCommand { get; set; }
        public DelegateCommand UpdateCommand { get; set; }

        #endregion

        #region Properties       

        /// <summary>
        /// The data update exception description
        /// </summary>
        public string ExceptionDescription
        {
            get { return _exceptionDescription; }
            set { SetProperty(ref _exceptionDescription, value); }
        }
        private string _exceptionDescription;

        /// <summary>
        /// The data update progessbar description
        /// </summary>
        public string ImportUpdateDescription
        {
            get { return _importUpdateDescription; }
            set { SetProperty(ref _importUpdateDescription, value); }
        }
        private string _importUpdateDescription;

        /// <summary>
        /// The data update progessbar value
        /// </summary>
        public int ImportUpdateProgress
        {
            get { return _ImportUpdateProgress; }
            set { SetProperty(ref _ImportUpdateProgress, value); }
        }
        private int _ImportUpdateProgress;

        /// <summary>
        /// The number of update data items
        /// </summary>
        public int ImportUpdateCount
        {
            get { return _importUpdateCount; }
            set { SetProperty(ref _importUpdateCount, value); }
        }
        private int _importUpdateCount;

        /// <summary>
        /// The number of updated data that passed validation
        /// </summary>
        public int ImportUpdatesPassed
        {
            get { return _importUpdatesPassed; }
            set { SetProperty(ref _importUpdatesPassed, value); }
        }
        private int _importUpdatesPassed;

        /// <summary>
        /// The number of updated data that failed
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
        /// The collection of data update exceptions
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
        /// The collection of data sheet columns from the selected Excel sheet
        /// </summary>
        public ObservableCollection<string> SearchColumnCollection
        {
            get { return _searchColumnCollection; }
            set { SetProperty(ref _searchColumnCollection, value); }
        }
        private ObservableCollection<string> _searchColumnCollection = null;

        /// <summary>
        /// The collection of data sheet columns from the selected Excel sheet
        /// </summary>
        public ObservableCollection<string> SourceColumnCollection
        {
            get { return _sourceColumnCollection; }
            set { SetProperty(ref _sourceColumnCollection, value); }
        }
        private ObservableCollection<string> _sourceColumnCollection = null;

        /// <summary>
        /// The collection of entity columns to search on
        /// </summary>
        public ObservableCollection<string> SearchEntityCollection
        {
            get { return _searchEntityCollection; }
            set { SetProperty(ref _searchEntityCollection, value); }
        }
        private ObservableCollection<string> _searchEntityCollection = null;

        /// <summary>
        /// The collection of data columns to update to from the DataUpdateColumn enum
        /// </summary>
        public ObservableCollection<string> DestinationColumnCollection
        {
            get { return _destinationColumnCollection; }
            set { SetProperty(ref _destinationColumnCollection, value); }
        }
        private ObservableCollection<string> _destinationColumnCollection = null;

        /// <summary>
        /// The collection of data entities to update to from the DataUpdateEntities enum
        /// </summary>
        public ObservableCollection<string> DestinationEntityCollection
        {
            get { return _destinationEntityCollection; }
            set { SetProperty(ref _destinationEntityCollection, value); }
        }
        private ObservableCollection<string> _destinationEntityCollection = null;

        /// <summary>
        /// The collection of company group entities from the database
        /// </summary>
        public ObservableCollection<CompanyGroup> DestinationCompanyCollection
        {
            get { return _destinationCompanyCollection; }
            set { SetProperty(ref _destinationCompanyCollection, value); }
        }
        private ObservableCollection<CompanyGroup> _destinationCompanyCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected data import file
        /// </summary>
        public string SelectedImportFile
        {
            get { return _selectedImportFile; }
            set { SetProperty(ref _selectedImportFile, value); }
        }
        private string _selectedImportFile = string.Empty;

        /// <summary>
        /// The selected data import sheet
        /// </summary>
        public WorkSheetInfo SelectedDataSheet
        {
            get { return _selectedImportSheet; }
            set
            {
                SetProperty(ref _selectedImportSheet, value);
                Task.Run(() => ImportWorkSheetDataAsync());               
            }
        }
        private WorkSheetInfo _selectedImportSheet;

        /// <summary>
        /// The selected column to search on
        /// </summary>
        public string SelectedSearchColumn
        {
            get { return _selectedSearchColumn; }
            set { SetProperty(ref _selectedSearchColumn, value); }
        }
        private string _selectedSearchColumn;

        /// <summary>
        /// The selected entity to search on
        /// </summary>
        public string SelectedSearchEntity
        {
            get { return _selectedSearchEntity; }
            set
            {
                SetProperty(ref _selectedSearchEntity, value);
                EnableDisableCompanySelection();
            }
        }
        private string _selectedSearchEntity;

        /// <summary>
        /// The selected source column to update from
        /// </summary>
        public string SelectedSourceColumn
        {
            get { return _selectedSourceColumn; }
            set { SetProperty(ref _selectedSourceColumn, value); }
        }
        private string _selectedSourceColumn;

        /// <summary>
        /// The selected destination column to update to
        /// </summary>
        public string SelectedDestinationColumn
        {
            get { return _selectedDestinationColumn; }
            set
            {
                SetProperty(ref _selectedDestinationColumn, value);
                FilterDestinationEntities();
            }
        }
        private string _selectedDestinationColumn;

        /// <summary>
        /// The selected destination entity to update to
        /// </summary>
        public string SelectedDestinationEntity
        {
            get { return _selectedDestinationEntity; }
            set
            {
                SetProperty(ref _selectedDestinationEntity, value);
                FilterDestinationSearchEntities();
            }
        }
        private string _selectedDestinationEntity;
        
        /// <summary>
        /// The selected destination company to update to
        /// </summary>
        public CompanyGroup SelectedDestinationCompany
        {
            get { return _selectedDestinationCompany; }
            set {SetProperty(ref _selectedDestinationCompany, value);}
        }
        private CompanyGroup _selectedDestinationCompany;

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
        /// Check if the data update was valid
        /// </summary>
        public bool ValidDataImport
        {
            get { return _validDataImport; }
            set { SetProperty(ref _validDataImport, value); }
        }
        private bool _validDataImport = false;

        /// <summary>
        /// Check if a valid data file was selected
        /// </summary>
        public bool ValidSelectedDataSheet
        {
            get { return _validSelectedDataSheet; }
            set { SetProperty(ref _validSelectedDataSheet, value); }
        }
        private bool _validSelectedDataSheet = false;

        /// <summary>
        /// Check if a valid data file was selected
        /// </summary>
        public bool ValidSearchEntities
        {
            get { return _validSearchEntities; }
            set { SetProperty(ref _validSearchEntities, value); }
        }
        private bool _validSearchEntities = false;
        
        /// <summary>
        /// Check if a valid destination company was selected
        /// only if the destination entity is linked to a company
        /// </summary>
        public bool DestinationCompanyLinked
        {
            get { return _destinationCompanyLinked; }
            set { SetProperty(ref _destinationCompanyLinked, value); }
        }
        private bool _destinationCompanyLinked = false;

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
        public Brush ValidSourceColumn
        {
            get { return _validSourceColumn; }
            set { SetProperty(ref _validSourceColumn, value); }
        }
        private Brush _validSourceColumn = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSearchColumn
        {
            get { return _validSearchColumn; }
            set { SetProperty(ref _validSearchColumn, value); }
        }
        private Brush _validSearchColumn = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDestinationColumn
        {
            get { return _validDestinationColumn; }
            set { SetProperty(ref _validDestinationColumn, value); }
        }
        private Brush _validDestinationColumn = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDestinationEntity
        {
            get { return _validDestinationEntity; }
            set { SetProperty(ref _validDestinationEntity, value); }
        }
        private Brush _validDestinationEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSearchEntity
        {
            get { return _validSearchEntity; }
            set { SetProperty(ref _validSearchEntity, value); }
        }
        private Brush _validSearchEntity = Brushes.Red;
        
        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDestinationCompany
        {
            get { return _validDestinationCompany; }
            set { SetProperty(ref _validDestinationCompany, value); }
        }
        private Brush _validDestinationCompany = Brushes.Red;

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
                        ValidImportFile = string.IsNullOrEmpty(SelectedImportFile) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataSheet":
                        ValidDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == "-- Please Select --" ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == "-- Please Select --" ? false : true; break;
                    case "SelectedSearchColumn":
                        ValidSearchColumn = string.IsNullOrEmpty(SelectedSearchColumn) || SelectedSearchColumn == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedSourceColumn":
                        ValidSourceColumn = string.IsNullOrEmpty(SelectedSourceColumn) || SelectedSourceColumn == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDestinationColumn":
                        ValidDestinationColumn = string.IsNullOrEmpty(SelectedDestinationColumn) || SelectedDestinationColumn == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDestinationEntity":
                        ValidDestinationEntity = string.IsNullOrEmpty(SelectedDestinationEntity) || SelectedDestinationEntity == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedSearchEntity":
                        ValidSearchEntity = string.IsNullOrEmpty(SelectedSearchEntity) || SelectedSearchEntity == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDestinationCompany":
                        ValidDestinationCompany = SelectedDestinationCompany == null || SelectedDestinationCompany.pkCompanyGroupID == 0 ? Brushes.Red : Brushes.Silver; break;
                }

                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDataUpdateViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseDataUpdateView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDataUpdateView()
        {
            _model = new DataUpdateRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            OpenFileCommand = new DelegateCommand(ExecuteOpenFileCommand);
            UpdateCommand = new DelegateCommand(ExecuteUpdate, CanUpdate).ObservesProperty(() => SelectedDataSheet)
                                                                         .ObservesProperty(() => SelectedSearchColumn)
                                                                         .ObservesProperty(() => SelectedSourceColumn)
                                                                         .ObservesProperty(() => SelectedDestinationColumn)
                                                                         .ObservesProperty(() => SelectedDestinationEntity)
                                                                         .ObservesProperty(() => SelectedSearchEntity)
                                                                         .ObservesProperty(() => SelectedDestinationCompany);
            // Load the view data
            ReadDataDestinationColumns();
            await ReadDataUpdateRulesAsync();
            await ReadCompanyGroupsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ImportUpdateDescription = string.Format("Importing - {0} of {1}", 0, 0);
            ExceptionDescription = "Data Import Exceptions";
            SelectedImportFile = string.Empty;
            ImportUpdateCount = 1;
            ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
            ValidDataFile = ValidDataImport = DestinationCompanyLinked = false;

            // Add the default items
            DataSheetCollection = new ObservableCollection<WorkSheetInfo>();
            WorkSheetInfo defaultInfo = new WorkSheetInfo();
            defaultInfo.SheetName = EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None);
            DataSheetCollection.Add(defaultInfo);
            SearchColumnCollection = new ObservableCollection<string>();
            SearchColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None));
            SourceColumnCollection = new ObservableCollection<string>();
            SourceColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None));
            DestinationEntityCollection = new ObservableCollection<string>();
            DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None));
            SearchEntityCollection = new ObservableCollection<string>();
            SearchEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None));
            SelectedDestinationEntity = EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None);
        }

        /// <summary>
        /// Read all the active update rule data from the database
        /// </summary>
        private async Task ReadDataUpdateRulesAsync()
        {
            try
            {
                _importRules = await Task.Run(() => _model.ReadDataUpdateRules(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataUpdateRulesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Import the data from the selected workbook sheet
        /// </summary>
        private void ImportWorkSheetDataAsync()
        {
            try
            {
                if (SelectedDataSheet != null && SelectedDataSheet.WorkBookName != null)
                {
                    DataTable sheetData = null;
                    ExceptionDescription = "Data Import Exceptions";
                    ImportUpdateDescription = string.Format("Importing - {0} of {1}", 0, 0);
                    ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
                    ImportUpdateCount = SelectedDataSheet.RowCount;

                    if (_officeHelper == null)
                        _officeHelper = new MSOfficeHelper();

                    // Update the worksheet data
                    sheetData = _officeHelper.ReadSheetDataIntoDataTable(SelectedDataSheet.WorkBookName, SelectedDataSheet.SheetName);

                    // This is to fake the progress bar for importing
                    for (int i = 1; i <= SelectedDataSheet.RowCount; i++)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ImportUpdateProgress = i;
                            ImportUpdatesPassed = i;
                            ImportUpdateDescription = string.Format("Importing - {0} of {1}", ImportUpdateProgress, SelectedDataSheet.RowCount);

                            if (i % 2 == 0)
                                Thread.Sleep(1);
                        });
                    }

                    ImportedDataCollection = sheetData;
                    ValidDataImport = true;

                    // Read the data columns of the selected worksheet
                    ReadDataSourceColumns();
                }
            }
            catch (Exception ex)
            {
                if (ExceptionsCollection == null)
                    ExceptionsCollection = new ObservableCollection<string>();

                ++ImportUpdatesFailed;
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ImportWorkSheetDataAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Filter the update destinations based on selected update rule source
        /// </summary>
        private void FilterDestinationEntities()
        {
            try
            {
                if (_importRules != null)
                {
                    DestinationEntityCollection.Clear();

                    // Convert enum description back to the enum
                    int destinationColumnID = EnumHelper.GetEnumFromDescription<DataUpdateColumn>(SelectedDestinationColumn).Value();

                    // Filter the update destination data based on the update source
                    IEnumerable<DataUpdateRule> destinations = _importRules.Where(p => p.enDataUpdateColumn == destinationColumnID).ToList();

                    // Add the default enum description
                    DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateEntity.None));

                    // Add all the update destination enum decriptions to the collection
                    foreach (DataUpdateRule rule in destinations)
                    {
                        DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum((DataUpdateEntity)rule.enDataUpdateEntity));
                    }

                    // Set the default value;
                    SelectedDestinationEntity = EnumHelper.GetDescriptionFromEnum(DataUpdateEntity.None);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "FilterDestinationEntities",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Filter the update destination search entities based on selected destination entity
        /// </summary>
        private void FilterDestinationSearchEntities()
        {
            try
            {
                if (_importRules != null && SelectedDestinationColumn != null && SelectedDestinationEntity != null)
                {
                    SearchEntityCollection.Clear();

                    // Convert enum description back to the enum
                    int destinationColumnID = EnumHelper.GetEnumFromDescription<DataUpdateColumn>(SelectedDestinationColumn).Value();
                    int destinationEntityID = EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value();

                    // Filter the update destination data based on the update source
                    _importRule = _importRules.Where(p => p.enDataUpdateColumn == destinationColumnID && 
                                                          p.enDataUpdateEntity == destinationEntityID).FirstOrDefault();

                    // Add the default enum description
                    SearchEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateEntity.None));

                    if (_importRule != null)
                    {
                        string[] searchEntities = _importRule.SearchEntities.Split(';');

                        // Add all the update destination enum decriptions to the collection
                        foreach (string searchEntity in searchEntities)
                        {
                            SearchEntityCollection.Add(searchEntity);
                        }
                    }

                    // Set the default value;
                    SelectedSearchEntity = EnumHelper.GetDescriptionFromEnum(DataUpdateEntity.None);
                    ValidSearchEntities = SearchEntityCollection.Count > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "FilterDestinationSearchEntities",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Enable/Disable the company combobox if the seleted
        /// destination filed is linked to a company
        /// </summary>
        private void EnableDisableCompanySelection()
        {
            try
            {
                DestinationCompanyLinked = false;
                ValidDestinationCompany = Brushes.Red;
                SearchEntity searchEntity = SearchEntity.Other;

                if (DestinationCompanyCollection != null)
                    SelectedDestinationCompany = DestinationCompanyCollection.Where(p => p.pkCompanyGroupID == 0).FirstOrDefault(); 

                if (SelectedDestinationEntity != null)
                {
                    if (SelectedSearchEntity != null && SelectedSearchEntity != "-- Please Select --")
                        // Convert enum description back to the enum
                        searchEntity = ((SearchEntity)Enum.Parse(typeof(SearchEntity), SelectedSearchEntity));

                    // Enable/Disable the company combobox if the seleted
                    // search entity is not not unique accros companies
                    if (searchEntity == SearchEntity.EmployeeNumber)
                    {
                        DestinationCompanyLinked = true;
                    }
                    else
                    {
                        DestinationCompanyLinked = false;
                        ValidDestinationCompany = Brushes.Silver;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "EnableDisableCompanySelection",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Populate the data source column combobox from the selected data sheet
        /// </summary>
        private void ReadDataSourceColumns()
        {
            if (SelectedDataSheet != null && SelectedDataSheet.ColumnNames != null)
            {
                SearchColumnCollection = new ObservableCollection<string>();
                SearchColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None));
                SourceColumnCollection = new ObservableCollection<string>();
                SourceColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None));

                foreach (string columnName in SelectedDataSheet.ColumnNames)
                {
                    SearchColumnCollection.Add(columnName);
                    SourceColumnCollection.Add(columnName);
                }

                SelectedSearchColumn = EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None);
                SelectedSourceColumn = EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None);
            }
        }

        /// <summary>
        /// Populate the data destination column combobox from the DataImportColumns enum
        /// </summary>
        private void ReadDataDestinationColumns()
        {
            DestinationColumnCollection = new ObservableCollection<string>();

            foreach (DataUpdateColumn source in Enum.GetValues(typeof(DataUpdateColumn)))
            {               
                DestinationColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
            }
        }

        /// <summary>
        /// Load all the company groups from the database
        /// </summary>
        private async Task ReadCompanyGroupsAsync()
        {
            try
            {
                DestinationCompanyCollection = await Task.Run(() => new CompanyModel(_eventAggregator).ReadCompanyGroups(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompanyGroupsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Execute when the open file command button is clicked 
        /// </summary>
        private void ExecuteOpenFileCommand()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                ValidDataImport = false;

                if (result.ToString() == "OK")
                {
                    MSOfficeHelper officeHelper = new MSOfficeHelper();
                    SelectedImportFile = dialog.FileName;

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
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
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
        private bool CanUpdate()
        {
            bool result = false;

            result = SelectedDataSheet != null && SelectedDataSheet.SheetName != "-- Please Select --" &&
                     !string.IsNullOrEmpty(SelectedSearchColumn) && SelectedSearchColumn != EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None) &&
                     !string.IsNullOrEmpty(SelectedSourceColumn) && SelectedSourceColumn != EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None) &&
                     !string.IsNullOrEmpty(SelectedDestinationColumn) && SelectedDestinationColumn != EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None) &&
                     !string.IsNullOrEmpty(SelectedDestinationEntity) && SelectedDestinationEntity != EnumHelper.GetDescriptionFromEnum(DataUpdateEntity.None) &&
                     !string.IsNullOrEmpty(SelectedSearchEntity) && SelectedSearchEntity != EnumHelper.GetDescriptionFromEnum(DataUpdateEntity.None);

            if (result && DestinationCompanyLinked)
                result = SelectedDestinationCompany != null && SelectedDestinationCompany.pkCompanyGroupID > 0;

            return result;
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private async void ExecuteUpdate()
        {
            try
            {
                ExceptionDescription = "Data Update Exceptions";
                ImportUpdateDescription = string.Format("Updating {0} - {1} of {2}", "", 0, 0);
                ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
                ImportUpdateCount = ImportedDataCollection.Rows.Count;
                string errorMessage = string.Empty;
                string searchCriteria = string.Empty;
                object updateValue = null;
                bool result = true;
                int rowIdx = 1;

                // Convert enum description back to the enum
                string destinationColumn = EnumHelper.GetEnumFromDescription<DataUpdateColumn>(SelectedDestinationColumn).ToString();
                int destinationEntityID = EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value();
                SearchEntity searchEntity = ((SearchEntity)Enum.Parse(typeof(SearchEntity), SelectedSearchEntity));
                SelectedDestinationCompany = DestinationCompanyLinked && SelectedDestinationCompany.pkCompanyGroupID > 0 ? SelectedDestinationCompany : null;

                foreach (DataRow row in ImportedDataCollection.Rows)
                {
                    ImportUpdateDescription = string.Format("Updating {0} - {1} of {2}", "", ++ImportUpdateProgress, ImportUpdateCount);
                    searchCriteria = row[SelectedSearchColumn] as string;
                    updateValue = row[SelectedSourceColumn];
                    rowIdx = ImportedDataCollection.Rows.IndexOf(row);

                    // If the search field is on Cell Number
                    // convert search field to a valid cell number
                    if (searchEntity == SearchEntity.PrimaryCellNumber)
                        searchCriteria = UIDataConvertionHelper.ConvertStringToCellNumber(searchCriteria);

                    if (updateValue != null && updateValue.ToString().Length > 0)
                    {
                        // Update the related entity data
                        switch ((DataBaseEntity)_importRule.enDataBaseEntity)
                        {
                            case DataBaseEntity.Client:
                                result = await Task.Run(() => new ClientModel(_eventAggregator).UpdateClient(searchEntity,
                                                                                                             searchCriteria,
                                                                                                             destinationColumn,
                                                                                                             updateValue,
                                                                                                             SelectedDestinationCompany,
                                                                                                             out errorMessage)); break;
                            case DataBaseEntity.Company:
                                result = await Task.Run(() => new CompanyModel(_eventAggregator).UpdateCompany(searchCriteria,
                                                                                                               destinationColumn,
                                                                                                               updateValue,
                                                                                                               SelectedDestinationCompany,
                                                                                                               out errorMessage)); break;
                            case DataBaseEntity.PackageSetup:
                                result = await Task.Run(() => new PackageSetupModel(_eventAggregator).UpdatePackageSetup(searchEntity,
                                                                                                                         searchCriteria,
                                                                                                                         destinationColumn,
                                                                                                                         updateValue,
                                                                                                                         SelectedDestinationCompany,
                                                                                                                         out errorMessage)); break;
                            case DataBaseEntity.Contract:
                                result = await Task.Run(() => new ContractModel(_eventAggregator).UpdateContract(searchEntity,
                                                                                                                 searchCriteria,
                                                                                                                 destinationColumn,
                                                                                                                 updateValue,
                                                                                                                 SelectedDestinationCompany,
                                                                                                                 out errorMessage)); break;
                            case DataBaseEntity.SimCard:
                                result = await Task.Run(() => new SimCardModel(_eventAggregator).UpdateSimCard(searchEntity,
                                                                                                               searchCriteria,
                                                                                                               destinationColumn,
                                                                                                               updateValue,
                                                                                                               SelectedDestinationCompany,
                                                                                                               out errorMessage)); break;
                            case DataBaseEntity.ClientBilling:
                                result = await Task.Run(() => new ClientBillingModel(_eventAggregator).UpdateClientBilling(searchEntity,
                                                                                                                           searchCriteria,
                                                                                                                           destinationColumn,
                                                                                                                           updateValue,
                                                                                                                           SelectedDestinationCompany,
                                                                                                                           out errorMessage)); break;
                        }
                    }

                    if (result)
                    {
                        ++ImportUpdatesPassed;
                    }
                    else
                    {
                        if (ExceptionsCollection == null)
                            ExceptionsCollection = new ObservableCollection<string>();

                        ++ImportUpdatesFailed;
                        ExceptionsCollection.Add(string.Format("{0} in datasheet {1} row {2}.", errorMessage, SelectedDataSheet.SheetName, rowIdx + 2));
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteUpdate",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}
