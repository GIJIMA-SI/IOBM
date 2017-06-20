using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    class ViewDepartmentViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DepartmentModel _model = null;
        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) department entity
        /// </summary>
        public Department SelectedDepartment
        {
            get { return _selectedDepartment; }
            set
            {
                if (value != null)
                {
                    SelectedDepartmentName = value.DepartmentDescription;
                    if (CompanyGroupCollection != null)
                        SelectedCompanyGroup = CompanyGroupCollection.Where(x => x.pkCompanyGroupID == value.fkCompanyGroupID).FirstOrDefault();
                    DepartmentState = value.IsActive;
                    SetProperty(ref _selectedDepartment, value);
                }
            }
        }
        private Department _selectedDepartment = new Department();

        /// <summary>
        /// The selected companyGroup
        /// </summary>
        public CompanyGroup SelectedCompanyGroup
        {
            get { return _selectedCompanyGroup; }
            set { SetProperty(ref _selectedCompanyGroup, value); }
        }
        private CompanyGroup _selectedCompanyGroup;

        /// <summary>
        /// The selected department state
        /// </summary>
        public bool DepartmentState
        {
            get { return _departmentState; }
            set { SetProperty(ref _departmentState, value); }
        }
        private bool _departmentState = true;

        /// <summary>
        /// The collection of departments from the database
        /// </summary>
        public ObservableCollection<Department> DepartmentCollection
        {
            get { return _departmentCollection; }
            set { SetProperty(ref _departmentCollection, value); }
        }
        private ObservableCollection<Department> _departmentCollection = null;

        /// <summary>
        /// The collection of companygroups from the database
        /// </summary>
        public ObservableCollection<CompanyGroup> CompanyGroupCollection
        {
            get { return _companyGroupCollection; }
            set { SetProperty(ref _companyGroupCollection, value); }
        }
        private ObservableCollection<CompanyGroup> _companyGroupCollection = null;

        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered department description
        /// </summary>
        public string SelectedDepartmentName
        {
            get { return _selectedDepartmentName; }
            set { SetProperty(ref _selectedDepartmentName, value); }
        }
        private string _selectedDepartmentName = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDepartmentName
        {
            get { return _validDepartmentName; }
            set { SetProperty(ref _validDepartmentName, value); }
        }
        private Brush _validDepartmentName = Brushes.Red;

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
                    case "SelectedDepartmentName":
                        ValidDepartmentName = string.IsNullOrEmpty(SelectedDepartmentName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCompanyGroup":
                        ValidCompanyGroup = SelectedCompanyGroup != null && SelectedCompanyGroup.GroupName != _defaultItem ? Brushes.Silver : Brushes.Red; break;
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
        public ViewDepartmentViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseClientSiteView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseClientSiteView()
        {
            _model = new DepartmentModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => ValidCompanyGroup)
                                                                          .ObservesProperty(() => ValidDepartmentName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => ValidCompanyGroup)
                                                                      .ObservesProperty(() => ValidDepartmentName);

            // Load the view data
            await ReadCompanyGroups();
            await ReadAllDepartments();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedDepartment = new Department();
            SelectedDepartment.pkDepartmentID = 0;
            SelectedCompanyGroup = CompanyGroupCollection != null ? CompanyGroupCollection.FirstOrDefault() : new CompanyGroup();
            SelectedDepartmentName = "";
            DepartmentState = true;
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the departments from the database
        /// </summary>
        private async Task ReadAllDepartments()
        {
            try
            {
                DepartmentCollection = await Task.Run(() => _model.ReadDepartments(false, false));
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
        /// Read all the company groups in the database
        /// </summary>
        /// <returns></returns>
        private async Task ReadCompanyGroups()
        {
            try
            {
                CompanyGroupCollection = await Task.Run(() => new CompanyGroupModel(_eventAggregator).ReadCompanyGroups(true));
                SelectedCompanyGroup = CompanyGroupCollection.FirstOrDefault();
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
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecute()
        {
            return ValidCompanyGroup != Brushes.Red && ValidDepartmentName != Brushes.Red;
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;

            SelectedDepartment.DepartmentDescription = SelectedDepartmentName.ToUpper();
            SelectedDepartment.fkCompanyGroupID = SelectedCompanyGroup.pkCompanyGroupID;
            SelectedDepartment.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedDepartment.ModifiedDate = DateTime.Now;
            SelectedDepartment.IsActive = DepartmentState;

            if (SelectedDepartment.pkDepartmentID == 0)
                result = _model.CreateDepartment(SelectedDepartment);
            else
                result = _model.UpdateDepartment(SelectedDepartment);

            if (result)
            {
                InitialiseViewControls();
                await ReadAllDepartments();
            }
        }

        #endregion

        #endregion
    }
}
