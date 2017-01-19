using Gijima.Controls.WPF;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewSecurityCFViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private SecurityModel _model = null;
        private IEventAggregator _eventAggregator;
        private ADUserSearchUX _adUserSearch = null;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The dual select listbox for user roles
        /// </summary>
        public DualListBoxUX DualSelectListBoxRoles
        {
            get { return _dualSelectListBoxRoles; }
            set { SetProperty(ref _dualSelectListBoxRoles, value); }
        }
        private DualListBoxUX _dualSelectListBoxRoles;

        /// <summary>
        /// The dual select listbox for user companies
        /// </summary>
        public DualListBoxUX DualSelectListBoxCompanies
        {
            get { return _dualSelectListBoxCompanies; }
            set { SetProperty(ref _dualSelectListBoxCompanies, value); }
        }
        private DualListBoxUX _dualSelectListBoxCompanies;

        /// <summary>
        /// Holds the selected (current) user entity
        /// </summary>
        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                SetProperty(ref _selectedUser, value);

                if (value != null)
                {
                    SelectedUserName = value.UserName;
                    SelectedUserFullName = value.UserFullName;
                    UserState = value.IsActive;
                    ReadUserRolesAsync();
                    ReadUserCompaniesAsync();
                }
            }
        }
        private User _selectedUser = new User();

        /// <summary>
        /// The collection of users from from the database
        /// </summary>
        public ObservableCollection<User> UserCollection
        {
            get { return _userCollection; }
            set { SetProperty(ref _userCollection, value); }
        }
        private ObservableCollection<User> _userCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered user name
        /// </summary>
        public string SelectedUserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }
        private string _userName = string.Empty;

        /// <summary>
        /// Holds the selected user fullname
        /// </summary>
        public string SelectedUserFullName
        {
            get { return _selectedUserFullName; }
            set { SetProperty(ref _selectedUserFullName, value); }
        }
        private string _selectedUserFullName = string.Empty;

        /// <summary>
        /// The selected user state
        /// </summary>
        public bool UserState
        {
            get { return _userState; }
            set { SetProperty(ref _userState, value); }
        }
        private bool _userState;

        /// <summary>
        /// The collection of user roles from the database
        /// </summary>
        public ObservableCollection<Role> UserRoleCollection
        {
            get { return _userRoleCollection; }
            set { SetProperty(ref _userRoleCollection, value); }
        }
        private ObservableCollection<Role> _userRoleCollection = null;

        /// <summary>
        /// The collection of user companies from the database
        /// </summary>
        public ObservableCollection<Company> UserCompanyCollection
        {
            get { return _userCompanyCollection; }
            set { SetProperty(ref _userCompanyCollection, value); }
        }
        private ObservableCollection<Company> _userCompanyCollection = null;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of available roles from the database
        /// </summary>
        public List<Role> AvailableRoleCollection
        {
            get { return _availableRoleCollection; }
            set { SetProperty(ref _availableRoleCollection, value); }
        }
        private List<Role> _availableRoleCollection = null;

        /// <summary>
        /// The collection of available companies from the database
        /// </summary>
        public List<Company> AvailableCompanyCollection
        {
            get { return _availableCompanyCollection; }
            set { SetProperty(ref _availableCompanyCollection, value); }
        }
        private List<Company> _availableCompanyCollection = null;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidUserName
        {
            get { return _validUserName; }
            set { SetProperty(ref _validUserName, value); }
        }
        private Brush _validUserName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidUserFullName
        {
            get { return _validUserFullName; }
            set { SetProperty(ref _validUserFullName, value); }
        }
        private Brush _validUserFullName = Brushes.Red;

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
                    case "SelectedUserName":
                        ValidUserName = string.IsNullOrEmpty(SelectedUserName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedUserFullName":
                        ValidUserFullName = string.IsNullOrEmpty(SelectedUserFullName) ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event gets raised when a domain user is selected on the domain user search UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ADUserSearch_UserSelected(object sender, UserSearchEventArgs e)
        {
            SelectedUserName = e.ADUser.Username;
            SelectedUserFullName = string.Format("{0} {1}", e.ADUser.Firstname, e.ADUser.Surname);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewSecurityCFViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseUserView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseUserView()
        {
            _model = new SecurityModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedUserName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedUserFullName)
                                                                          .ObservesProperty(() => SelectedUserName);

            // Load the view data
            await ReadUsersAsync();
            await ReadRolesAsync();
            await ReadCompaniesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            if (DualSelectListBoxRoles == null)
            {
                DualSelectListBoxRoles = new DualListBoxUX();
                DualSelectListBoxRoles.AvailableItemsHeader = "Available Roles";
                DualSelectListBoxRoles.SelectedItemsHeader = "Selected Roles";
                DualSelectListBoxRoles.DisplayMemberPath = "RoleName";
                DualSelectListBoxRoles.SelectedValuePath = "pkRoleId";
                DualSelectListBoxRoles.TabIndexStart = 3;
            }
            if (DualSelectListBoxCompanies == null)
            {
                DualSelectListBoxCompanies = new DualListBoxUX();
                DualSelectListBoxCompanies.AvailableItemsHeader = "Available Companies";
                DualSelectListBoxCompanies.SelectedItemsHeader = "Selected Companies";
                DualSelectListBoxCompanies.DisplayMemberPath = "CompanyName";
                DualSelectListBoxCompanies.SelectedValuePath = "pkCompanyId";
                DualSelectListBoxCompanies.TabIndexStart = 7;
            }
            DualSelectListBoxRoles.AvailableItems = AvailableRoleCollection;
            DualSelectListBoxRoles.SelectedItems = null;
            DualSelectListBoxCompanies.AvailableItems = AvailableCompanyCollection;
            DualSelectListBoxCompanies.SelectedItems = null;
            SelectedUserFullName = SelectedUserName = string.Empty;
        }

        /// <summary>
        /// Load all the users from the database
        /// </summary>
        private async Task ReadUsersAsync()
        {
            try
            {
                UserCollection = await Task.Run(() => _model.ReadApplicationUsers(false));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewSecurityCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadUsersAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the roles linked to the selected user from the database
        /// </summary>
        private async void ReadUserRolesAsync()
        {
            try
            {
                IEnumerable<Role> userRoles = await Task.Run(() => _model.ReadApplicationUserRoles(SelectedUser.pkUserID));
                List<Role> unselectedRoles = new List<Role>();

                // Filter the selected roles from the available roles
                if (userRoles != null)
                {
                    foreach (Role role in AvailableRoleCollection)
                    {
                        if (userRoles.Where(p => p.pkRoleID == role.pkRoleID).FirstOrDefault() == null)
                            unselectedRoles.Add(role);
                    }
                }

                DualSelectListBoxRoles.SelectedItems = userRoles;
                DualSelectListBoxRoles.AvailableItems = userRoles == null || userRoles.Count() == 0 ? AvailableRoleCollection : unselectedRoles;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewSecurityCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadUserRolesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the companies linked to the selected user from the database
        /// </summary>
        private async void ReadUserCompaniesAsync()
        {
            try
            {
                IEnumerable<Company> userCompanies = await Task.Run(() => _model.ReadApplicationUserCompanies(SelectedUser.pkUserID));
                List<Company> unselectedCompanies = new List<Company>();

                // Filter the selected companies from the available companies
                if (userCompanies != null)
                {
                    foreach (Company company in AvailableCompanyCollection)
                    {
                        if (userCompanies.Where(p => p.pkCompanyID == company.pkCompanyID).FirstOrDefault() == null)
                            unselectedCompanies.Add(company);
                    }
                }

                DualSelectListBoxCompanies.SelectedItems = userCompanies;
                DualSelectListBoxCompanies.AvailableItems = userCompanies == null || userCompanies.Count() == 0 ? AvailableCompanyCollection : unselectedCompanies;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewSecurityCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadUserCompaniesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the roles from the database
        /// </summary>
        private async Task ReadRolesAsync()
        {
            try
            {
                AvailableRoleCollection = await Task.Run(() => _model.ReadApplicationRoles());
                DualSelectListBoxRoles.AvailableItems = AvailableRoleCollection;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewSecurityCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadRolesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the companies from the database
        /// </summary>
        private async Task ReadCompaniesAsync()
        {
            try
            {
                AvailableCompanyCollection = await Task.Run(() => new CompanyModel(_eventAggregator).ReadCompanies(true, true).ToList());
                DualSelectListBoxCompanies.AvailableItems = AvailableCompanyCollection;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewSecurityCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompaniesAsync",
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
            return !string.IsNullOrWhiteSpace(SelectedUserName);
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
            return !string.IsNullOrWhiteSpace(SelectedUserName) && !string.IsNullOrWhiteSpace(SelectedUserFullName);
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            try
            {
                bool result = false;

                SelectedUser.UserName = SelectedUserName.ToUpper();
                SelectedUser.UserFullName = SelectedUserFullName.ToUpper();
                SelectedUser.LastActivityDate = DateTime.Now;
                SelectedUser.IsActive = UserState;
                SelectedUser.UserInRoles = new List<UserInRole>();
                SelectedUser.UserInCompanies = new List<UserInCompany>();
                UserInRole userRole = null;
                UserInCompany userCompany = null;

                // Add all the seleted roles to the selected user entity
                foreach (Role role in DualSelectListBoxRoles.SelectedItems)
                {
                    userRole = new UserInRole();
                    userRole.fkRoleID = role.pkRoleID;
                    userRole.fkUserID = SelectedUser.pkUserID;
                    SelectedUser.UserInRoles.Add(userRole);
                }

                // Add all the seleted companies to the selected user entity
                foreach (Company company in DualSelectListBoxCompanies.SelectedItems)
                {
                    userCompany = new UserInCompany();
                    userCompany.fkCompanyID = company.pkCompanyID;
                    userCompany.fkUserID = SelectedUser.pkUserID;
                    SelectedUser.UserInCompanies.Add(userCompany);
                }

                if (SelectedUser.pkUserID == 0)
                    result = _model.CreateApplicationUser(SelectedUser);
                else
                    result = _model.UpdateApplicationUser(SelectedUser);

                if (result)
                {
                    // Create a IOBM user for the new Mobile Manager user and publish the 
                    // event to sync the application user with the system user
                    IOBM.Model.Data.User iobmUser = new IOBM.Model.Data.User();
                    iobmUser.UserName = SelectedUser.UserName;
                    iobmUser.UserFullName = SelectedUser.UserFullName;
                    iobmUser.LastActivityDate = DateTime.Now;
                    iobmUser.IsActive = UserState;
                    _eventAggregator.GetEvent<MobileManagerSecurityEvent>().Publish(iobmUser);

                    // Reload the application users
                    InitialiseViewControls();
                    await ReadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewSecurityCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteSave",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private async void ExecuteAdd()
        {
            try
            {
                _adUserSearch = new ADUserSearchUX();
                _adUserSearch.onADUserSelected += ADUserSearch_UserSelected;
                UserState = true;
                PopupWindow popupWindow = new PopupWindow(_adUserSearch, "User Search", PopupWindow.PopupButtonType.Close);
                popupWindow.ShowDialog();
                await ReadUsersAsync();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewSecurityCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteAdd",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}
