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
using Gijima.DataImport.MSOffice;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataUpdateViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataUpdateRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private MSOfficeHelper _officeHelper = null;
        private string _defaultItem = "-- Please Select --";
        
        #region Commands

        public DelegateCommand OpenFileCommand { get; set; }
        public DelegateCommand UpdateCommand { get; set; }
        public DelegateCommand MapCommand { get; set; }
        public DelegateCommand UnMapCommand { get; set; }

        #endregion

        #region Properties       

        /// <summary>
        /// The data update progessbar description
        /// </summary>
        public string UpdateUpdateDescription
        {
            get { return _updateUpdateDescription; }
            set { SetProperty(ref _updateUpdateDescription, value); }
        }
        private string _updateUpdateDescription;

        /// <summary>
        /// The data update progessbar value
        /// </summary>
        public int UpdateUpdateProgress
        {
            get { return _UpdateUpdateProgress; }
            set { SetProperty(ref _UpdateUpdateProgress, value); }
        }
        private int _UpdateUpdateProgress;

        /// <summary>
        /// The number of update data items
        /// </summary>
        public int UpdateUpdateCount
        {
            get { return _updateUpdateCount; }
            set { SetProperty(ref _updateUpdateCount, value); }
        }
        private int _updateUpdateCount;

        /// <summary>
        /// The number of updated data that passed validation
        /// </summary>
        public int UpdateUpdatesPassed
        {
            get { return _updateUpdatesPassed; }
            set { SetProperty(ref _updateUpdatesPassed, value); }
        }
        private int _updateUpdatesPassed;

        /// <summary>
        /// The number of updated data that failed
        /// </summary>
        public int UpdateUpdatesFailed
        {
            get { return _updateUpdatesFailed; }
            set { SetProperty(ref _updateUpdatesFailed, value); }
        }
        private int _updateUpdatesFailed;

        /// <summary>
        /// The collection of updateed excel data
        /// </summary>
        public DataTable UpdateDataCollection
        {
            get { return _updateDataCollection; }
            set { SetProperty(ref _updateDataCollection, value); }
        }
        private DataTable _updateDataCollection = null;

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
        /// The collection of mapped properties
        /// </summary>
        public ObservableCollection<string> MappedPropertyCollection
        {
            get { return _mappedPropertyCollection; }
            set { SetProperty(ref _mappedPropertyCollection, value); }
        }
        private ObservableCollection<string> _mappedPropertyCollection = null;

        /// <summary>
        /// Collection of columns that can be mapped more than once
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
        /// The selected data update file
        /// </summary>
        public string SelectedUpdateFile
        {
            get { return _selectedUpdateFile; }
            set { SetProperty(ref _selectedUpdateFile, value); }
        }
        private string _selectedUpdateFile = string.Empty;

        /// <summary>
        /// The selected data update sheet
        /// </summary>
        public WorkSheetInfo SelectedDataSheet
        {
            get { return _selectedUpdateSheet; }
            set
            {
                SetProperty(ref _selectedUpdateSheet, value);
                Task.Run(() => UpdateWorkSheetDataAsync());
            }
        }
        private WorkSheetInfo _selectedUpdateSheet;

        /// <summary>
        /// The selected destination entity to update to
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
                        if (new DataUpdatePropertyModel(_eventAggregator).HasMultipleProperty(column, EnumHelper.GetEnumFromDescription<DataUpdateEntity>(value).Value()))
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
        /// The selected destination column to update to
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

                                //Test the selected option for multiple entries
                                if (new DataUpdatePropertyModel(_eventAggregator).HasMultipleProperty(mappedProperties[1].Trim(), EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value()))
                                {  MultipleEntriesColection.Where(p => p.Name == mappedProperties[1].Trim()).FirstOrDefault().NumberMappings--; }
                                else
                                {  DestinationColumnCollection.Add(mappedProperties[1].Trim()); }

                                SelectedMappedProperty = null;
                                CanStartUpdate();
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
        /// The selected source column to update from
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
        /// Check if the data update was valid
        /// </summary>
        public bool ValidUpdateData
        {
            get { return _validDataUpdate; }
            set { SetProperty(ref _validDataUpdate, value); }
        }
        private bool _validDataUpdate = false;

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
        /// Check if the data can be updateed
        /// </summary>
        public bool CanUpdate
        {
            get { return _canUpdate; }
            set { SetProperty(ref _canUpdate, value); }
        }
        private bool _canUpdate = false;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidUpdateFile
        {
            get { return _validUpdateFile; }
            set { SetProperty(ref _validUpdateFile, value); }
        }
        private Brush _validUpdateFile = Brushes.Red;

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
                    case "SelectedUpdateFile":
                        ValidUpdateFile = string.IsNullOrEmpty(SelectedUpdateFile) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataSheet":
                        ValidDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == _defaultItem ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == _defaultItem ? false : true; break;
                    case "SelectedSourceSearch":
                        ValidSourceSearch = string.IsNullOrEmpty(SelectedSourceSearch) || SelectedSourceSearch == _defaultItem ? Brushes.Red : Brushes.Silver;
                        CanStartUpdate(); break;
                    case "SelectedDestinationEntity":
                        ValidDestinationEntity = string.IsNullOrEmpty(SelectedDestinationEntity) || SelectedDestinationEntity == _defaultItem ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDestinationEntity = string.IsNullOrEmpty(SelectedDestinationEntity) || SelectedDestinationEntity == _defaultItem ? false : true; break;
                    case "SelectedDestinationSearch":
                        ValidDestinationSearch = string.IsNullOrEmpty(SelectedDestinationSearch) || SelectedDestinationSearch == _defaultItem ? Brushes.Red : Brushes.Silver;
                        CanStartUpdate(); break;
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
            _model = new DataUpdateRuleModel(_eventAggregator);
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
            UpdateCommand = new DelegateCommand(ExecuteUpdate);
            MapCommand = new DelegateCommand(ExecuteMap, CanMap).ObservesProperty(() => SelectedSourceProperty)
                                                                .ObservesProperty(() => SelectedDestinationProperty);
            UnMapCommand = new DelegateCommand(ExecuteUnMap, CanUnMap).ObservesProperty(() => SelectedMappedProperty);

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
            UpdateUpdateDescription = string.Format("Updateing - {0} of {1}", 0, 0);
            UpdateUpdateCount = 1;
            UpdateUpdateProgress = UpdateUpdatesPassed = UpdateUpdatesFailed = 0;
            ValidUpdateData = false;
            MappedPropertyCollection = ExceptionsCollection = null;
            UpdateDataCollection = null;

            // Add the default items
            SourceSearchCollection = new ObservableCollection<string>();
            SourceSearchCollection.Add(_defaultItem);
            SourceColumnCollection = new ObservableCollection<string>();
            SourceColumnCollection.Add(_defaultItem);
            SelectedSourceSearch = SelectedSourceProperty = _defaultItem;
        }

        /// <summary>
        /// Update the data from the selected workbook sheet
        /// </summary>
        private void UpdateWorkSheetDataAsync()
        {
            try
            {
                InitialiseUpdateControls();

                if (SelectedDataSheet != null && SelectedDataSheet.WorkBookName != null)
                {
                    DataTable sheetData = null;
                    UpdateUpdateDescription = string.Format("Reading - {0} of {1}", 0, 0);
                    UpdateUpdateProgress = UpdateUpdatesPassed = UpdateUpdatesFailed = 0;
                    UpdateUpdateCount = SelectedDataSheet.RowCount;

                    if (_officeHelper == null)
                        _officeHelper = new MSOfficeHelper();

                    // Update the worksheet data
                    sheetData = _officeHelper.ReadSheetDataIntoDataTable(SelectedDataSheet.WorkBookName, SelectedDataSheet.SheetName);

                    // This is to fake the progress bar for updateing
                    for (int i = 1; i <= SelectedDataSheet.RowCount; i++)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateUpdateProgress = i;
                            UpdateUpdatesPassed = i;
                            UpdateUpdateDescription = string.Format("Updateing - {0} of {1}", UpdateUpdateProgress, SelectedDataSheet.RowCount);

                            if (i % 2 == 0)
                                Thread.Sleep(1);
                        });
                    }

                    UpdateDataCollection = sheetData;
                    ValidUpdateData = true;

                    // Read the data columns of the selected worksheet
                    ReadDataSourceColumns();
                }
            }
            catch (Exception ex)
            {
                if (ExceptionsCollection == null)
                    ExceptionsCollection = new ObservableCollection<string>();

                ++UpdateUpdatesFailed;
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateWorkSheetDataAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Check if the update can start
        /// </summary>
        private void CanStartUpdate()
        {
            if (SelectedSourceSearch != null && SelectedDestinationSearch != null && MappedPropertyCollection != null)
            {
                if (MappedPropertyCollection != null)
                {
                    CanUpdate = SelectedSourceSearch != _defaultItem && SelectedDestinationSearch != _defaultItem && MappedPropertyCollection.Count > 0 ? true : false;
                    
                    ValidMapping = SelectedSourceSearch != _defaultItem && SelectedDestinationSearch != _defaultItem &&  MappedPropertyCollection.Count > 0 ? Brushes.Silver : Brushes.Red;
                }
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Read all the data update rules from the database
        /// </summary>
        private void ReadDataUpdateRulesAsync()
        {
            try
            {
                DestinationEntityCollection = new ObservableCollection<string>();
                foreach (DataUpdateEntity dataUpdateEntity in Enum.GetValues(typeof(DataUpdateEntity)))
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
        /// Populate the data search and destination column combobox from the DataUpdateProperty/Seach
        /// </summary>
        private void ReadDataDestinationInfo()
        {
            DataUpdateEntity destinationEntity = EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity);

            DestinationSearchCollection = new DataUpdateSearchPropertyModel(_eventAggregator).GetSearchProperties(destinationEntity);
            DestinationColumnCollection = new DataUpdatePropertyModel(_eventAggregator).GetPropertiesDescription(destinationEntity);
            
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
                ValidUpdateData = false;

                if (result.ToString() == "OK")
                {
                    MSOfficeHelper officeHelper = new MSOfficeHelper();
                    SelectedUpdateFile = dialog.FileName;

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
                if (new DataUpdatePropertyModel(_eventAggregator).HasMultipleProperty(SelectedDestinationProperty, EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value()))
                {
                    MultipleEntriesColection.Where(p => p.Name == SelectedDestinationProperty).FirstOrDefault().NumberMappings++;
                }
                else
                {
                    DestinationColumnCollection.Remove(SelectedDestinationProperty);
                }
                
                //SelectedSourceProperty = SelectedDestinationProperty = _defaultItem;
                CanStartUpdate();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
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
                if (new DataUpdatePropertyModel(_eventAggregator).HasMultipleProperty(mappedProperties[1].Trim(), EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value()))
                {
                    MultipleEntriesColection.Where(p => p.Name == mappedProperties[1].Trim()).FirstOrDefault().NumberMappings--;
                }
                else
                {
                    DestinationColumnCollection.Add(mappedProperties[1].Trim());
                }
                
                CanStartUpdate();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataUpdateViewModel",
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
                UpdateUpdateDescription = string.Format("Updating {0} - {1} of {2}", "", 0, 0);
                UpdateUpdateProgress = UpdateUpdatesPassed = UpdateUpdatesFailed = 0;
                UpdateUpdateCount = UpdateDataCollection.Rows.Count;
                string errorMessage = string.Empty;
                string searchCriteria = string.Empty;
                bool result = false;
                int rowIdx = 1;

                // Convert enum description back to the enum
                SearchEntity searchEntity = ((SearchEntity)Enum.Parse(typeof(SearchEntity), SelectedDestinationSearch));

                foreach (DataRow row in UpdateDataCollection.Rows)
                {
                    UpdateUpdateDescription = string.Format("Updating {0} - {1} of {2}", "", ++UpdateUpdateProgress, UpdateUpdateCount);
                    searchCriteria = row[SelectedSourceSearch] as string;
                    rowIdx = UpdateDataCollection.Rows.IndexOf(row);

                    // Update the related entity data
                    switch (EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity))
                    {
                        case DataUpdateEntity.ClientBilling:
                            result = await Task.Run(() => new ClientBillingModel(_eventAggregator).UpdateClientBillingUpdate(searchCriteria,
                                                                                                                             MappedPropertyCollection,
                                                                                                                             row,
                                                                                                                             EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value(),
                                                                                                                             out errorMessage)); break;
                        case DataUpdateEntity.Contract:
                            result = await Task.Run(() => new ContractModel(_eventAggregator).UpdateContractUpdate(searchCriteria,
                                                                                                                   MappedPropertyCollection,
                                                                                                                   row,
                                                                                                                   EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value(),
                                                                                                                   out errorMessage)); break;
                        case DataUpdateEntity.Client:
                            result = await Task.Run(() => new SimCardModel(_eventAggregator).CreateSimCardImport(searchCriteria,
                                                                                                                 MappedPropertyCollection,
                                                                                                                 row,
                                                                                                                 EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value(),
                                                                                                                 out errorMessage)); break;
                        case DataUpdateEntity.SimCard:
                            result = await Task.Run(() => new SimCardModel(_eventAggregator).CreateSimCardImport(searchCriteria,
                                                                                                                 MappedPropertyCollection,
                                                                                                                 row,
                                                                                                                 EnumHelper.GetEnumFromDescription<DataUpdateEntity>(SelectedDestinationEntity).Value(),
                                                                                                                 out errorMessage)); break;
                    }

                    if (result)
                    {
                        ++UpdateUpdatesPassed;
                    }
                    else
                    {
                        if (ExceptionsCollection == null)
                            ExceptionsCollection = new ObservableCollection<string>();

                        ++UpdateUpdatesFailed;
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
