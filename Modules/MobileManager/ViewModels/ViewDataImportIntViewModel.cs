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

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataImportIntViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataImportRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private IEnumerable<DataImportRule> _importRules = null;
        private MSOfficeHelper _officeHelper = null;
        private string _defaultItem = "-- Please Select --";

        #region Commands

        public DelegateCommand OpenFileCommand { get; set; }
        public DelegateCommand ImportCommand { get; set; }
        public DelegateCommand MapCommand { get; set; }
        public DelegateCommand UnMapCommand { get; set; }

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
        public ObservableCollection<string> DestinationColumnCollection
        {
            get { return _destinationColumnCollection; }
            set { SetProperty(ref _destinationColumnCollection, value); }
        }
        private ObservableCollection<string> _destinationColumnCollection = null;

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
        public ObservableCollection<MultipleEntry> MultipleEntriesColection
        {
            get { return _multipleEntriesColection; }
            set { SetProperty(ref _multipleEntriesColection, value); }
        }
        private ObservableCollection<MultipleEntry> _multipleEntriesColection;

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
                    MultipleEntriesColection = new ObservableCollection<MultipleEntry>();
                    ReadDataDestinationInfo();

                    //Add all the properties that can have multiple entires to the collection
                    foreach (string column in DestinationColumnCollection)
                    {
                        if (new DataImportPropertyModel(_eventAggregator).HasMultipleProperty(column, EnumHelper.GetEnumFromDescription<DataImportEntity>(value).Value()))
                        {
                            MultipleEntriesColection.Add(new MultipleEntry(column));
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
        public string SelectedDestinationProperty
        {
            get { return _selectedDestinationProperty; }
            set { SetProperty(ref _selectedDestinationProperty, value); }
        }
        private string _selectedDestinationProperty;

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
                        {
                            foreach (string property in MappedPropertyCollection)
                            {
                                string[] mappedProperties = property.Split('=');
                                SourceColumnCollection.Add(mappedProperties[0].Trim());
                                if (mappedProperties[1].Trim() != EnumHelper.GetDescriptionFromEnum(DataImportColumn.IMENumber).Replace(" ", string.Empty))
                                    DestinationColumnCollection.Add(mappedProperties[1].Trim());
                                SelectedMappedProperty = null;
                                CanStartImport();
                            }
                            MappedPropertyCollection.Clear();
                        }
                        SourceColumnCollection = new ObservableCollection<string>(SourceSearchCollection);
                        SourceColumnCollection.Remove(value);
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
        private async void InitialiseDataUpdateView()
        {
            InitialiseViewControls();

            // Initialise the view commands
            OpenFileCommand = new DelegateCommand(ExecuteOpenFileCommand);
            ImportCommand = new DelegateCommand(ExecuteUpdate);
            MapCommand = new DelegateCommand(ExecuteMap, CanMap).ObservesProperty(() => SelectedSourceProperty)
                                                                .ObservesProperty(() => SelectedDestinationProperty);
            UnMapCommand = new DelegateCommand(ExecuteUnMap, CanUnMap).ObservesProperty(() => SelectedMappedProperty);

            // Load the view data
            await ReadDataUpdateRulesAsync();
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
            ImportUpdateDescription = string.Format("Updateing - {0} of {1}", 0, 0);
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
                    //Test if all multiple entries is selcted
                    bool mulitpleEntriesTest = true;
                    if (MultipleEntriesColection.Count > 0)
                    {
                        foreach (MultipleEntry multipleEntry in MultipleEntriesColection)
                        {
                            if (multipleEntry.NumberMappings == 0)
                            {
                                mulitpleEntriesTest = false;
                            }
                        }
                    }

                    CanImport = SelectedSourceSearch != _defaultItem && SelectedDestinationSearch != _defaultItem && DestinationColumnCollection.Count <= MultipleEntriesColection.Count + 1 && mulitpleEntriesTest ? true : false;
                    ValidMapping = SelectedSourceSearch != _defaultItem && SelectedDestinationSearch != _defaultItem &&
                           DestinationColumnCollection.Count <= MultipleEntriesColection.Count + 1 && mulitpleEntriesTest ? Brushes.Silver : Brushes.Red;
                }
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Read all the data import rules from the database
        /// </summary>
        private async Task ReadDataUpdateRulesAsync()
        {
            try
            {
                //DestinationEntityCollection = new ObservableCollection<string>();
                //DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataBaseEntity.None));

                _importRules = await Task.Run(() => _model.ReadDataImportRules());

                //foreach (DataUpdateRule rule in _updateRules)
                //{
                //    DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum((DataUpdateEntity)rule.enDataBaseEntity));
                //}

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
            DestinationColumnCollection = new DataImportPropertyModel(_eventAggregator).GetPropertiesDescription(destinationEntity);

            SelectedDestinationSearch = _defaultItem;
            SelectedDestinationProperty = _defaultItem;
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
                   SelectedDestinationProperty != null && SelectedDestinationProperty != _defaultItem;
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

                SelectedMappedProperty = string.Format("{0} = {1}", SelectedSourceProperty, SelectedDestinationProperty);
                MappedPropertyCollection.Add(SelectedMappedProperty);
                SourceColumnCollection.Remove(SelectedSourceProperty);

                //Test if the item selected my have multiple mappings
                if (new DataImportPropertyModel(_eventAggregator).HasMultipleProperty(SelectedDestinationProperty, EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity).Value()))
                {
                    MultipleEntriesColection.Where(p => p.Name == SelectedDestinationProperty).FirstOrDefault().NumberMappings++;
                }
                else
                {
                    DestinationColumnCollection.Remove(SelectedDestinationProperty);
                }

                SelectedSourceProperty = SelectedDestinationProperty = _defaultItem;
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
                if (new DataImportPropertyModel(_eventAggregator).HasMultipleProperty(mappedProperties[1].Trim(), EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity).Value()))
                {
                    MultipleEntriesColection.Where(p => p.Name == mappedProperties[1].Trim()).FirstOrDefault().NumberMappings--;
                }
                else
                {
                    DestinationColumnCollection.Add(mappedProperties[1].Trim());
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
        private async void ExecuteUpdate()
        {
            try
            {
                ImportUpdateDescription = string.Format("Updating {0} - {1} of {2}", "", 0, 0);
                ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
                ImportUpdateCount = ImportedDataCollection.Rows.Count;
                string errorMessage = string.Empty;
                string searchCriteria = string.Empty;
                bool result = false;
                int rowIdx = 1;

                // Convert enum description back to the enum
                SearchEntity searchEntity = ((SearchEntity)Enum.Parse(typeof(SearchEntity), SelectedDestinationSearch));

                foreach (DataRow row in ImportedDataCollection.Rows)
                {
                    ImportUpdateDescription = string.Format("Updating {0} - {1} of {2}", "", ++ImportUpdateProgress, ImportUpdateCount);
                    searchCriteria = row[SelectedSourceSearch] as string;
                    rowIdx = ImportedDataCollection.Rows.IndexOf(row);

                    // Update the related entity data
                    switch (EnumHelper.GetEnumFromDescription<DataImportEntity>(SelectedDestinationEntity))
                    {
                        //case DataBaseEntity.Client:
                        //    result = await Task.Run(() => new ClientModel(_eventAggregator).UpdateClient(searchEntity,
                        //                                                                                 searchCriteria,
                        //                                                                                 destinationColumn,
                        //                                                                                 importValue,
                        //                                                                                 SelectedDestinationCompany,
                        //                                                                                 out errorMessage)); break;
                        //case DataBaseEntity.Company:
                        //    result = await Task.Run(() => new CompanyModel(_eventAggregator).UpdateCompany(searchCriteria,
                        //                                                                                   destinationColumn,
                        //                                                                                   importValue,
                        //                                                                                   SelectedDestinationCompany,
                        //                                                                                   out errorMessage)); break;
                        //case DataBaseEntity.PackageSetup:
                        //    result = await Task.Run(() => new PackageSetupModel(_eventAggregator).UpdatePackageSetup(searchEntity,
                        //                                                                                             searchCriteria,
                        //                                                                                             destinationColumn,
                        //                                                                                             importValue,
                        //                                                                                             SelectedDestinationCompany,
                        //                                                                                             out errorMessage)); break;
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
                                                                                                                 SecurityHelper.LoggedInUserFullName,
                                                                                                                 out errorMessage)); break;
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

        public short NumberMappings
        {
            get { return _numberMappings; }
            set { _numberMappings = value; }
        }
        private short _numberMappings;

        /// <summary>
        /// Constructor
        /// </summary>
        public MultipleEntry(string Name)
        {
            this.Name = Name;
            NumberMappings = 0;
        }
    }
}
