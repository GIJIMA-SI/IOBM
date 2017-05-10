using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Events;
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
using System.Windows.Media;
using System.Windows;
using System.Threading;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using System.Reflection;
using System.Windows.Controls;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataImportIntViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataImportRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private MSOfficeHelper _officeHelper = null;
        private string _defaultItem = "-- Please Select --";

        #region Commands

        public DelegateCommand OpenFileCommand { get; set; }
        public DelegateCommand ImportCommand { get; set; }
        public DelegateCommand MapCommand { get; set; }
        public DelegateCommand UnMapCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }

        #endregion

        #region Properties       

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
        /// Client/Data import existing clients 
        /// to know what mapping to expect
        /// </summary>
        public bool ExistingClients
        {
            get { return _existingClients; }
            set { SetProperty(ref _existingClients, value); }
        }
        private bool _existingClients = false;

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
        public ObservableCollection<string> SourceSearchCollection
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
        public ObservableCollection<string> DestinationSearchCollection
        {
            get { return _searchEntityCollection; }
            set { SetProperty(ref _searchEntityCollection, value); }
        }
        private ObservableCollection<string> _searchEntityCollection = null;

        /// <summary>
        /// The collection of data columns to import to from the DataUpdateColumn enum
        /// </summary>
        public ObservableCollection<DataImportProperty> DestinationColumnCollection
        {
            get { return _destinationColumnCollection; }
            set { SetProperty(ref _destinationColumnCollection, value); }
        }
        private ObservableCollection<DataImportProperty> _destinationColumnCollection = null;

        /// <summary>
        /// The collection of the data columns to import as a comboboxitem
        /// </summary>
        public ObservableCollection<ComboBoxItem> DestinationComboBoxItemCollection
        {
            get { return _destinationComboBoxItemCollection; }
            set { SetProperty(ref _destinationComboBoxItemCollection, value); }
        }
        private ObservableCollection<ComboBoxItem> _destinationComboBoxItemCollection;

        /// <summary>
        /// The collection of data entities to import to from the DataUpdateEntities enum
        /// </summary>
        public ObservableCollection<string> DestinationEntityCollection
        {
            get { return _destinationEntityCollection; }
            set { SetProperty(ref _destinationEntityCollection, value); }
        }
        private ObservableCollection<string> _destinationEntityCollection = null;

        /// <summary>
        /// The collection of mapped properties
        /// </summary>
        public ObservableCollection<string> MappedPropertyCollection
        {
            get { return _mappedPropertyCollection; }
            set { SetProperty(ref _mappedPropertyCollection, value); }
        }
        private ObservableCollection<string> _mappedPropertyCollection = null;

        /// <summary>
        /// Collection of columns that can be maped more than once
        /// </summary>
        public ObservableCollection<MultipleEntry> MultipleEntriesCollection
        {
            get { return _multipleEntriesCollection; }
            set { SetProperty(ref _multipleEntriesCollection, value); }
        }
        private ObservableCollection<MultipleEntry> _multipleEntriesCollection;

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
        /// The selected destination entity to import to
        /// </summary>
        public string SelectedDestinationEntity
        {
            get { return _selectedDestinationEntity; }
            set
            {
                SetProperty(ref _selectedDestinationEntity, value);
                DestinationSearchCollection = new ObservableCollection<string>();
                if (value != null && value != _defaultItem)
                {
                    //Ask if the users already exist within the system
                    MessageBoxResult result;
                    if (value == EnumHelper.GetDescriptionFromEnum(DataImportEntity.Client))
                    {
                        result = MessageBox.Show("Do the clients in the importing list already exist in the system?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        ExistingClients = result == MessageBoxResult.Yes ? true : false; 
                    }

                    MultipleEntriesCollection = new ObservableCollection<MultipleEntry>();
                    ReadDataDestinationInfo();

                    //Add all the properties that can have multiple entires to the collection
                    foreach (DataImportProperty dataImportProperty in DestinationColumnCollection)
                    {
                        if (dataImportProperty.MultipleProperty)
                        {
                            MultipleEntriesCollection.Add(new MultipleEntry(dataImportProperty.PropertyDescription, dataImportProperty.Required));
                        }
                    }
                }
            }
        }
        private string _selectedDestinationEntity;

        /// <summary>
        /// The selected entity to search on
        /// </summary>
        public string SelectedDestinationSearch
        {
            get { return _selectedDestinationSearch; }
            set { SetProperty(ref _selectedDestinationSearch, value); }
        }
        private string _selectedDestinationSearch;

        /// <summary>
        /// The selected destination column to import to
        /// </summary>
        public ComboBoxItem SelectedDestinationProperty
        {
            get { return _selectedDestinationProperty; }
            set { SetProperty(ref _selectedDestinationProperty, value); }
        }
        private ComboBoxItem _selectedDestinationProperty;

        /// <summary>
        /// The selected column to search on
        /// </summary>
        public string SelectedSourceSearch
        {
            get { return _selectedSourceSearch; }
            set
            {
                SetProperty(ref _selectedSourceSearch, value);

                //Remove the seleceted item from the SourceColumn Collection
                //Reset propert mapping collection
                try
                {
                    if (value != _defaultItem)
                    {
                        if (MappedPropertyCollection != null)
                            MappedPropertyCollection.Clear();

                        //Clear all required items
                        SourceColumnCollection = new ObservableCollection<string>();
                        SourceColumnCollection = new ObservableCollection<string>(SourceSearchCollection);
                        SelectedDestinationEntity = _defaultItem;
                        DestinationSearchCollection = new ObservableCollection<string>();
                        SelectedDestinationSearch = null;
                        DestinationComboBoxItemCollection = new ObservableCollection<ComboBoxItem>();
                        SelectedDestinationProperty = null;

                        if (value != _defaultItem)
                            SourceColumnCollection.Remove(value);

                        SelectedDestinationSearch = SelectedDestinationSearch;

                    }
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
        }
        private string _selectedSourceSearch;

        /// <summary>
        /// The selected source column to import from
        /// </summary>
        public string SelectedSourceProperty
        {
            get { return _selectedSourceProperty; }
            set { SetProperty(ref _selectedSourceProperty, value); }
        }
        private string _selectedSourceProperty;

        /// <summary>
        /// The selected mapped properties
        /// </summary>
        public string SelectedMappedProperty
        {
            get { return _selectedMappedProperty; }
            set { SetProperty(ref _selectedMappedProperty, value); }
        }
        private string _selectedMappedProperty;

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
        public bool ValidSelectedDestinationEntity
        {
            get { return _validSelectedDestinationEntity; }
            set { SetProperty(ref _validSelectedDestinationEntity, value); }
        }
        private bool _validSelectedDestinationEntity = false;

        /// <summary>
        /// Check if the data can be imported
        /// </summary>
        public bool CanImport
        {
            get { return _canImport; }
            set { SetProperty(ref _canImport, value); }
        }
        private bool _canImport = false;

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
        public Brush ValidSourceSearch
        {
            get { return _validSearchColumn; }
            set { SetProperty(ref _validSearchColumn, value); }
        }
        private Brush _validSearchColumn = Brushes.Red;

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
        public Brush ValidDestinationSearch
        {
            get { return _validSearchEntity; }
            set { SetProperty(ref _validSearchEntity, value); }
        }
        private Brush _validSearchEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidMapping
        {
            get { return _validMapping; }
            set { SetProperty(ref _validMapping, value); }
        }
        private Brush _validMapping = Brushes.Red;

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
                        ValidDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == _defaultItem ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == _defaultItem ? false : true; break;
                    case "SelectedSourceSearch":
                        ValidSourceSearch = string.IsNullOrEmpty(SelectedSourceSearch) || SelectedSourceSearch == _defaultItem ? Brushes.Red : Brushes.Silver;
                        CanStartImport(); break;
                    case "SelectedDestinationEntity":
                        ValidDestinationEntity = string.IsNullOrEmpty(SelectedDestinationEntity) || SelectedDestinationEntity == _defaultItem ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDestinationEntity = string.IsNullOrEmpty(SelectedDestinationEntity) || SelectedDestinationEntity == _defaultItem ? false : true; break;
                    case "SelectedDestinationSearch":
                        ValidDestinationSearch = string.IsNullOrEmpty(SelectedDestinationSearch) || SelectedDestinationSearch == _defaultItem ? Brushes.Red : Brushes.Silver;
                        CanStartImport(); break;
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
        public ViewDataImportIntViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            _model = new DataImportRuleModel(_eventAggregator);
            InitialiseDataUpdateView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseDataUpdateView()
        {
            InitialiseViewControls();

            // Initialise the view commands
            OpenFileCommand = new DelegateCommand(ExecuteOpenFileCommand);
            ImportCommand = new DelegateCommand(ExecuteImport);
            MapCommand = new DelegateCommand(ExecuteMap, CanMap).ObservesProperty(() => SelectedSourceProperty)
                                                                .ObservesProperty(() => SelectedDestinationProperty);
            UnMapCommand = new DelegateCommand(ExecuteUnMap, CanUnMap).ObservesProperty(() => SelectedMappedProperty);
            ExportCommand = new DelegateCommand(ExecuteExport, CanExecuteExport).ObservesProperty(() => ExceptionsCollection);

            // Load the view data
            ReadDataUpdateRulesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ValidDataFile = ValidSelectedDestinationEntity = false;

            // Add the default items
            DataSheetCollection = new ObservableCollection<WorkSheetInfo>();
            WorkSheetInfo defaultInfo = new WorkSheetInfo();
            defaultInfo.SheetName = _defaultItem;
            SelectedDataSheet = defaultInfo;
            DataSheetCollection.Add(defaultInfo);
            DestinationEntityCollection = new ObservableCollection<string>();
            DestinationEntityCollection.Add(_defaultItem);
            InitialiseUpdateControls();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseUpdateControls()
        {
            ImportUpdateDescription = string.Format("Importing - {0} of {1}", 0, 0);
            ImportUpdateCount = 1;
            ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
            ValidImportData = false;
            MappedPropertyCollection = ExceptionsCollection = null;
            ImportedDataCollection = null;

            // Add the default items
            SourceSearchCollection = new ObservableCollection<string>();
            SourceSearchCollection.Add(_defaultItem);
            SourceColumnCollection = new ObservableCollection<string>();
            SourceColumnCollection.Add(_defaultItem);
            SelectedSourceSearch = SelectedSourceProperty = _defaultItem;
        }

        /// <summary>
        /// Import the data from the selected workbook sheet
        /// </summary>
        private void ImportWorkSheetDataAsync()
        {
            try
            {
                InitialiseUpdateControls();

                if (SelectedDataSheet != null && SelectedDataSheet.WorkBookName != null)
                {
                    DataTable sheetData = null;
                    ImportUpdateDescription = string.Format("Reading - {0} of {1}", 0, 0);
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
                    ValidImportData = true;

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
        /// Check if the import can start
        /// </summary>
        private void CanStartImport()
        {
            if (SelectedSourceSearch != null && SelectedDestinationSearch != null && MappedPropertyCollection != null)
            {
                if (MappedPropertyCollection != null)
                {
                    //Test if required fields are used
                    bool allRequiredFields = true;
                    if (DestinationComboBoxItemCollection.Count > 0)
                    {
                        foreach (ComboBoxItem comboBoxItem in DestinationComboBoxItemCollection)
                        {
                            if (DestinationColumnCollection.Where(p => p.PropertyDescription == comboBoxItem.Content.ToString()).FirstOrDefault().MultipleProperty)
                            {
                                //Check if the existing clients option was used
                                if (ExistingClients)
                                {
                                    if (MultipleEntriesCollection.Where(p => p.Name == comboBoxItem.Content.ToString()).FirstOrDefault().NumberMappings == 0 &&
                                    MultipleEntriesCollection.Where(p => p.Name == comboBoxItem.Content.ToString()).FirstOrDefault().Required == true)
                                    {
                                        allRequiredFields = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (MultipleEntriesCollection.Where(p => p.Name == comboBoxItem.Content.ToString()).FirstOrDefault().NumberMappings == 0 &&
                                    MultipleEntriesCollection.Where(p => p.Name == comboBoxItem.Content.ToString()).FirstOrDefault().Required == true)
                                    {
                                        allRequiredFields = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                //Check if the existing clients option was used
                                if (ExistingClients)
                                {
                                    if (DestinationColumnCollection.Where(p => p.PropertyDescription == comboBoxItem.Content.ToString()).FirstOrDefault().Required &&
                                        DestinationColumnCollection.Where(p => p.PropertyDescription == comboBoxItem.Content.ToString()).FirstOrDefault().ExistingClientRequired)
                                    {
                                        allRequiredFields = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (DestinationColumnCollection.Where(p => p.PropertyDescription == comboBoxItem.Content.ToString()).FirstOrDefault().Required)
                                    {
                                        allRequiredFields = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    CanImport = SelectedSourceSearch != _defaultItem && SelectedDestinationSearch != _defaultItem && allRequiredFields ? true : false;
                    ValidMapping = SelectedSourceSearch != _defaultItem && SelectedDestinationSearch != _defaultItem && allRequiredFields ? Brushes.Silver : Brushes.Red;
                }
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Read all the data import rules from the database
        /// </summary>
        private void ReadDataUpdateRulesAsync()
        {
            try
            {
                DestinationEntityCollection = new ObservableCollection<string>();
                foreach (DataImportEntity dataUpdateEntity in Enum.GetValues(typeof(DataImportEntity)))
                {
                    DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(dataUpdateEntity));
                }

                SelectedDestinationEntity = _defaultItem;
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
        /// Populate the data source column combobox from the selected data sheet
        /// </summary>
        private void ReadDataSourceColumns()
        {
            if (SelectedDataSheet != null && SelectedDataSheet.ColumnNames != null)
            {
                List<string> sheetColumns = new List<string>();
                sheetColumns.Add(_defaultItem);

                foreach (string columnName in SelectedDataSheet.ColumnNames)
                {
                    sheetColumns.Add(columnName);
                }

                SourceSearchCollection = new ObservableCollection<string>(sheetColumns);
                SourceColumnCollection = new ObservableCollection<string>(sheetColumns);
                Application.Current.Dispatcher.Invoke(() => { SelectedSourceSearch = SelectedSourceProperty = _defaultItem; });
            }
        }

        /// <summary>
        /// Populate the data search and destination column combobox from the DataImportProperty/Seach
        /// </summary>
        private void ReadDataDestinationInfo()
        {
            DataImportEntity destinationEntity = EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity);

            DestinationSearchCollection = new DataImportSearchPropertyModel(_eventAggregator).GetSearchProperties(destinationEntity);
            DestinationColumnCollection = new DataImportPropertyModel(_eventAggregator).GetPropertiesCollection(destinationEntity);

            //Creates a new collection of required fields
            DestinationComboBoxItemCollection = new ObservableCollection<ComboBoxItem>();
            foreach (DataImportProperty dataImportProperty in DestinationColumnCollection)
            {
                //Check client/contract detail if selected
                if (ExistingClients)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = dataImportProperty.PropertyDescription;
                    item.Foreground = dataImportProperty.Required == true && dataImportProperty.ExistingClientRequired == true ? Brushes.Red : Brushes.Black;
                    DestinationComboBoxItemCollection.Add(item);
                }
                else
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = dataImportProperty.PropertyDescription;
                    item.Foreground = dataImportProperty.Required == true ? Brushes.Red : Brushes.Black;
                    DestinationComboBoxItemCollection.Add(item);
                }
            }

            SelectedDestinationSearch = _defaultItem;
            SelectedDestinationProperty = DestinationComboBoxItemCollection.Where(p => p.Content.ToString() == _defaultItem).FirstOrDefault();
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
                InitialiseDataUpdateView();
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                ValidImportData = false;

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
                .Publish(new ApplicationMessage("ViewDataImportViewModel",
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
        private bool CanMap()
        {
            return SelectedSourceProperty != null && SelectedSourceProperty != _defaultItem &&
                   SelectedDestinationProperty != null && SelectedDestinationProperty.Content.ToString() != _defaultItem;
        }

        /// <summary>
        /// Execute when the Map command button is clicked 
        /// </summary>
        private void ExecuteMap()
        {
            try
            {
                if (MappedPropertyCollection == null)
                    MappedPropertyCollection = new ObservableCollection<string>();

                SelectedMappedProperty = string.Format("{0} = {1}", SelectedSourceProperty, SelectedDestinationProperty.Content.ToString());
                MappedPropertyCollection.Add(SelectedMappedProperty);
                SourceColumnCollection.Remove(SelectedSourceProperty);

                //Test if the item selected my have multiple mappings
                if (DestinationColumnCollection.Where(p => p.PropertyDescription == SelectedDestinationProperty.Content.ToString()).FirstOrDefault().MultipleProperty)
                {
                    MultipleEntriesCollection.Where(p => p.Name == SelectedDestinationProperty.Content.ToString()).FirstOrDefault().NumberMappings++;
                }
                else
                {
                    DestinationComboBoxItemCollection.Remove(SelectedDestinationProperty);
                }

                SelectedSourceProperty = _defaultItem;
                SelectedDestinationProperty = DestinationComboBoxItemCollection.Where(p => p.Content.ToString() == _defaultItem).FirstOrDefault();
                CanStartImport();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportViewModel",
                                         string.Format("Error! {0}, {1}.",
                                         ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                         "ExecuteMap",
                                         ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanUnMap()
        {
            return SelectedMappedProperty != null;
        }

        /// <summary>
        /// Check if the ExportToExcel button may be enabled
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteExport()
        {
            return ExceptionsCollection != null && ExceptionsCollection.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Execute when the export to excel command button is clicked 
        /// </summary>
        private void ExecuteExport()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                //dialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                dialog.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result.ToString() == "OK")
                {
                    MSOfficeHelper officeHelper = new MSOfficeHelper();
                    string fileName = string.Format("{0}\\Data Update Errors - {1}.xlsx", dialog.SelectedPath, String.Format("{0:dd MMM yyyy HH.mm}", DateTime.Now));

                    //Creates a temp datatable to collect the errors and export
                    DataTable tempDataTable = new DataTable();
                    tempDataTable.Columns.Add("Exception List");
                    foreach (string myError in ExceptionsCollection)
                    {
                        tempDataTable.Rows.Add(myError);
                    }

                    officeHelper.ExportDataTableToExcel(tempDataTable, fileName);
                }

                MessageBox.Show("Results exported successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
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
        /// Execute when the UnMap command button is clicked 
        /// </summary>
        private void ExecuteUnMap()
        {
            try
            {
                string[] mappedProperties = SelectedMappedProperty.Split('=');
                MappedPropertyCollection.Remove(SelectedMappedProperty);
                SourceColumnCollection.Add(mappedProperties[0].Trim());
                SelectedMappedProperty = null;

                //Test the selected option for multiple entries
                if (DestinationColumnCollection.Where(p => p.PropertyDescription == mappedProperties[1].Trim()).FirstOrDefault().MultipleProperty)
                {
                    MultipleEntriesCollection.Where(p => p.Name == mappedProperties[1].Trim()).FirstOrDefault().NumberMappings--;
                }
                else
                {
                    //Create a new combox item to add mapped item
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = mappedProperties[1].Trim();
                    item.Foreground = DestinationColumnCollection.Where(p => p.PropertyDescription == mappedProperties[1].Trim()).FirstOrDefault().Required == true ? Brushes.Red : Brushes.Black;
                    DestinationComboBoxItemCollection.Add(item);
                }
                CanStartImport();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportViewModel",
                                         string.Format("Error! {0}, {1}.",
                                         ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                         "ExecuteUnMap",
                                         ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private async void ExecuteImport()
        {
            try
            {
                //Clear the previous error list
                ExceptionsCollection = new ObservableCollection<string>();

                ImportUpdateDescription = string.Format("Importing {0} - {1} of {2}", "", 0, 0);
                ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
                ImportUpdateCount = ImportedDataCollection.Rows.Count;
                string errorMessage = string.Empty;
                string searchCriteria = string.Empty;
                bool result = false;
                int rowIdx = 1;

                // Convert enum description back to the enum
                //SearchEntity searchEntity = ((SearchEntity)Enum.Parse(typeof(SearchEntity), SelectedDestinationSearch.Replace(" ","")));

                foreach (DataRow row in ImportedDataCollection.Rows)
                {
                    ImportUpdateDescription = string.Format("Importing {0} - {1} of {2}", "", ++ImportUpdateProgress, ImportUpdateCount);
                    searchCriteria = row[SelectedSourceSearch] as string;
                    rowIdx = ImportedDataCollection.Rows.IndexOf(row);

                    // Update the related entity data
                    switch (EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity))
                    {
                        case DataImportEntity.Device:
                            result = await Task.Run(() => new DevicesModel(_eventAggregator).CreateDeviceImport(searchCriteria,
                                                                                                                MappedPropertyCollection,
                                                                                                                row,
                                                                                                                EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity).Value(),
                                                                                                                out errorMessage)); break;
                        case DataImportEntity.SimCard:
                            result = await Task.Run(() => new SimCardModel(_eventAggregator).CreateSimCardImport(searchCriteria,
                                                                                                                 MappedPropertyCollection,
                                                                                                                 row,
                                                                                                                 EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity).Value(),
                                                                                                                 out errorMessage)); break;
                        case DataImportEntity.Client:
                            result = await Task.Run(() => new ClientModel(_eventAggregator).CreateClientImport(searchCriteria,
                                                                                                               MappedPropertyCollection,
                                                                                                               row,
                                                                                                               EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity).Value(),
                                                                                                               ExistingClients,
                                                                                                               out errorMessage)); break;
                        case DataImportEntity.ClientBilling:
                            result = await Task.Run(() => new ClientBillingModel(_eventAggregator).CreateBillingImport(searchCriteria,
                                                                                                                       MappedPropertyCollection,
                                                                                                                       row,
                                                                                                                       EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity).Value(),
                                                                                                                       out errorMessage)); break;
                        case DataImportEntity.Package:
                            result = await Task.Run(() => new PackageModel(_eventAggregator).CreatePackageImport(searchCriteria,
                                                                                                                 MappedPropertyCollection,
                                                                                                                 row,
                                                                                                                 EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity).Value(),
                                                                                                                 out errorMessage)); break;
                    }

                    if (result)
                    {
                        ++ImportUpdatesPassed;
                    }
                    else
                    {
                        //Creates a tempCollection to update the collection so the export button can be notified
                        ExceptionsCollection.Add(string.Format("{0} in datasheet {1} row {2}.", errorMessage, SelectedDataSheet.SheetName, rowIdx + 2));
                        ObservableCollection<string> tempExceptionCollection = new ObservableCollection<string>(ExceptionsCollection);
                        ExceptionsCollection = new ObservableCollection<string>(tempExceptionCollection);

                        ++ImportUpdatesFailed;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportViewModel",
                                         string.Format("Error! {0}, {1}.",
                                         ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                         "ExecuteUpdate",
                                         ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// A class to keep information on a column if 
    /// it can have more than none mapping
    /// </summary>
    public class MultipleEntry
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _name;
        /// <summary>
        /// Number of times mapping used
        /// To know when enable disable imprt button
        /// </summary>
        public short NumberMappings
        {
            get { return _numberMappings; }
            set { _numberMappings = value; }
        }
        private short _numberMappings;
        /// <summary>
        /// If the field is required
        /// </summary>
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }
        private bool _required;

        /// <summary>
        /// Constructor
        /// </summary>
        public MultipleEntry(string Name, bool Required)
        {
            this.Name = Name;
            NumberMappings = 0;
            this.Required = Required;
        }
    }
}
