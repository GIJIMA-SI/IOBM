using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Linq;
using Gijima.IOBM.MobileManager.Views;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Security;
using System.Windows;
using Gijima.IOBM.Infrastructure.Structs;
using System.Reflection;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewLineManagerViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private LineManagerModel _model = null;
        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CompanyGroupCommand { get; set; }
        public DelegateCommand DepartmentCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) line manager entity
        /// </summary>
        public LineManager SelectedLineManager
        {
            get { return _selectedLineManager; }
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedLineManager, value);

                    SelectedName = value.Name;
                    SelectedSurname = value.Surname;
                    SelectedPhoneNumber = value.PhoneNumber;
                    SelectedCellNumber = value.CellNumber;
                    SelectedEmail = value.LineManagerEmail;
                    LineManagerState = value.IsActive;

                    if (CompanyGroupCollection != null && value.Department != null)
                        SelectedCompanyGroup = CompanyGroupCollection.Where(x => x.pkCompanyGroupID == value.Department.fkCompanyGroupID).FirstOrDefault();
                }
            }
        }
        private LineManager _selectedLineManager = new LineManager();

        /// <summary>
        /// The collection of line managers from from the database
        /// </summary>
        public ObservableCollection<LineManager> LineManagerCollection
        {
            get { return _lineManagerCollection; }
            set { SetProperty(ref _lineManagerCollection, value); }
        }
        private ObservableCollection<LineManager> _lineManagerCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered name
        /// </summary>
        public string SelectedName
        {
            get { return _selectedName; }
            set { SetProperty(ref _selectedName, value); }
        }
        private string _selectedName = string.Empty;

        /// <summary>
        /// The selected surname
        /// </summary>
        public string SelectedSurname
        {
            get { return _selectedSurname; }
            set { SetProperty(ref _selectedSurname, value); }
        }
        private string _selectedSurname = string.Empty;

        /// <summary>
        /// The selected email
        /// </summary>
        public string SelectedEmail
        {
            get { return _selectedEmail; }
            set { SetProperty(ref _selectedEmail, value); }
        }
        private string _selectedEmail = string.Empty;

        /// <summary>
        /// The selected cell number
        /// </summary>
        public string SelectedCellNumber
        {
            get { return _selectedCellNumber; }
            set { SetProperty(ref _selectedCellNumber, value); }
        }
        private string _selectedCellNumber = string.Empty;

        /// <summary>
        /// The selected phone number
        /// </summary>
        public string SelectedPhoneNumber
        {
            get { return _selectedPhoneNumber; }
            set { SetProperty(ref _selectedPhoneNumber, value); }
        }
        private string _selectedPhoneNumber = string.Empty;

        /// <summary>
        /// The selected company group
        /// </summary>
        public CompanyGroup SelectedCompanyGroup
        {
            get { return _selectedCompanyGroup; }
            set
            {
                SetProperty(ref _selectedCompanyGroup, value);

                if (value != null)
                {
                    Task.Run(() => ReadDepartments());
                }
            }
        }
        private CompanyGroup _selectedCompanyGroup;

        /// <summary>
        /// The selected phone number
        /// </summary>
        public Department SelectedDepartment
        {
            get { return _selectedDepartment; }
            set { SetProperty(ref _selectedDepartment, value); }
        }
        private Department _selectedDepartment;

        /// <summary>
        /// The selected line manager state
        /// </summary>
        public bool LineManagerState
        {
            get { return _sineManagerState; }
            set { SetProperty(ref _sineManagerState, value); }
        }
        private bool _sineManagerState;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of company groups from the database
        /// </summary>
        public ObservableCollection<CompanyGroup> CompanyGroupCollection
        {
            get { return _companyGroupCollection; }
            set { SetProperty(ref _companyGroupCollection, value); }
        }
        private ObservableCollection<CompanyGroup> _companyGroupCollection = null;

        /// <summary>
        /// The collection of departments from the database
        /// </summary>
        public ObservableCollection<Department> DepartmentCollection
        {
            get { return _departmentCollection; }
            set { SetProperty(ref _departmentCollection, value); }
        }
        private ObservableCollection<Department> _departmentCollection = null;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidName
        {
            get { return _validName; }
            set { SetProperty(ref _validName, value); }
        }
        private Brush _validName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSurname
        {
            get { return _validSurname; }
            set { SetProperty(ref _validSurname, value); }
        }
        private Brush _validSurname = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidEmail
        {
            get { return _validEmail; }
            set { SetProperty(ref _validEmail, value); }
        }
        private Brush _validEmail = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCompanyGroup
        {
            get { return _validCompanyGroup; }
            set { SetProperty(ref _validCompanyGroup, value); }
        }
        private Brush _validCompanyGroup = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDepartment
        {
            get { return _validDepartment; }
            set { SetProperty(ref _validDepartment, value); }
        }
        private Brush _validDepartment = Brushes.Red;

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
                    case "SelectedName":
                        ValidName = string.IsNullOrEmpty(SelectedName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedSurname":
                        ValidSurname = string.IsNullOrEmpty(SelectedSurname) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedEmail":
                        ValidEmail = string.IsNullOrEmpty(SelectedEmail) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCompanyGroup":
                        ValidCompanyGroup = SelectedCompanyGroup == null || SelectedCompanyGroup.GroupName == _defaultItem ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDepartment":
                        ValidDepartment = SelectedDepartment == null || SelectedDepartment.DepartmentDescription == _defaultItem ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewLineManagerViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseView()
        {
            _model = new LineManagerModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedName)
                                                                                .ObservesProperty(() => SelectedSurname)
                                                                                .ObservesProperty(() => SelectedEmail)
                                                                                .ObservesProperty(() => SelectedPhoneNumber)
                                                                                .ObservesProperty(() => SelectedCellNumber)
                                                                                .ObservesProperty(() => SelectedCompanyGroup)
                                                                                .ObservesProperty(() => SelectedDepartment)
                                                                                .ObservesProperty(() => LineManagerState);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => ValidName)
                                                                                .ObservesProperty(() => ValidSurname)
                                                                                .ObservesProperty(() => ValidEmail)
                                                                                .ObservesProperty(() => SelectedPhoneNumber)
                                                                                .ObservesProperty(() => SelectedCellNumber)
                                                                                .ObservesProperty(() => ValidCompanyGroup)
                                                                                .ObservesProperty(() => ValidDepartment)
                                                                                .ObservesProperty(() => LineManagerState);
            CompanyGroupCommand = new DelegateCommand(ExecuteShowCompanyGroup, CanExecuteMaintenace);
            DepartmentCommand = new DelegateCommand(ExecuteShowDepartment, CanExecuteMaintenace);

            // Load the view data
            await ReadCompanyGroups();
            await ReadLineManagers();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            //Creates a new LineManager
            SelectedLineManager = new LineManager();
            SelectedLineManager.pkLineManagerID = 0;

            SelectedName = string.Empty;
            SelectedSurname = string.Empty;
            SelectedEmail = string.Empty;
            SelectedPhoneNumber = string.Empty;
            SelectedCellNumber = string.Empty;

            SelectedCompanyGroup = CompanyGroupCollection != null ? CompanyGroupCollection.FirstOrDefault() : null;
            SelectedDepartment = DepartmentCollection != null ? DepartmentCollection.FirstOrDefault() : null;

            LineManagerState = true;
        }

        /// <summary>
        /// Load all the company groups from the database
        /// </summary>
        private async Task ReadCompanyGroups()
        {
            try
            {
                CompanyGroupCollection = await Task.Run(() => new CompanyGroupModel(_eventAggregator).ReadCompanyGroups(true));
                SelectedCompanyGroup = CompanyGroupCollection != null ? CompanyGroupCollection.FirstOrDefault() : null;
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
        /// Read all the line managers
        /// </summary>
        /// <returns></returns>
        private async Task ReadLineManagers()
        {
            try
            {
                LineManagerCollection = await Task.Run(() => _model.ReadLineManagers(false, false));
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
        /// Load all the departments from the database
        /// </summary>
        private async Task ReadDepartments()
        {
            try
            {
                DepartmentCollection = await Task.Run(() => new DepartmentModel(_eventAggregator).ReadCompanyDepartments(SelectedCompanyGroup.pkCompanyGroupID));

                if (DepartmentCollection != null && SelectedLineManager.pkLineManagerID != 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedDepartment = DepartmentCollection.Where(x => x.pkDepartmentID == SelectedLineManager.fkDepartmentID).FirstOrDefault();
                    });
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

        #endregion

        #region Command Execution

        /// <summary>
        /// Validate if the cancel functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteCancel()
        {
            return  !string.IsNullOrWhiteSpace(SelectedName) || !string.IsNullOrWhiteSpace(SelectedSurname) || !string.IsNullOrWhiteSpace(SelectedEmail) ||
                    !string.IsNullOrWhiteSpace(SelectedCellNumber) || !string.IsNullOrWhiteSpace(SelectedPhoneNumber) ||
                    (SelectedCompanyGroup != null && SelectedCompanyGroup.GroupName != _defaultItem) ||
                    (SelectedDepartment != null && SelectedDepartment.DepartmentDescription != _defaultItem);
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the save functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            return ValidName == Brushes.Silver && ValidSurname == Brushes.Silver && ValidEmail == Brushes.Silver && ValidCompanyGroup == Brushes.Silver &&
                    ValidDepartment == Brushes.Silver;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;

            SelectedLineManager.fkDepartmentID = SelectedDepartment.pkDepartmentID;
            SelectedLineManager.Name = SelectedName;
            SelectedLineManager.Surname = SelectedSurname;
            SelectedLineManager.PhoneNumber = SelectedPhoneNumber;
            SelectedLineManager.CellNumber = SelectedCellNumber;
            SelectedLineManager.LineManagerEmail = SelectedEmail;
            SelectedLineManager.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedLineManager.ModifiedDate = DateTime.Now;
            SelectedLineManager.IsActive = LineManagerState;

            if (SelectedLineManager.pkLineManagerID == 0)
                result = _model.CreateLineManager(SelectedLineManager);
            else
                result = _model.UpdateLineManager(SelectedLineManager);

            if (result)
            {
                InitialiseViewControls();
                await ReadLineManagers();
            }
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the maintenace functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteMaintenace()
        {
            return true;
        }

        /// <summary>
        /// Execute when the company group maintenance command button is clicked 
        /// </summary>
        private async void ExecuteShowCompanyGroup()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewCompanyGroup(), "Company Group Maintenance", PopupWindow.PopupButtonType.Close);
            //So width and heigh can be set auto for referencedata
            popupWindow.MaxHeight = popupWindow.MinHeight = 300;
            popupWindow.MaxWidth = popupWindow.MinWidth = 450;
            popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            popupWindow.ShowDialog();
            await ReadCompanyGroups();
        }

        /// <summary>
        /// Execute when the department maintenance command button is clicked 
        /// </summary>
        private async void ExecuteShowDepartment()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewDepartment(), "Department Maintenance", PopupWindow.PopupButtonType.Close);
            //So width and heigh can be set auto for referencedata
            popupWindow.MaxHeight = popupWindow.MinHeight = 450;
            popupWindow.MaxWidth = popupWindow.MinWidth = 450;
            popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            popupWindow.ShowDialog();
            await ReadCompanyGroups();
        }

        #endregion

        #endregion
    }
}
