using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewCompanyViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private CompanyModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand GroupCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) company entity
        /// </summary>
        public Company SelectedCompany
        {
            get { return _selectedCompany; }
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedCompany, value);

                    SelectedCompanyName = value.CompanyName;
                    SelectedGroup = GroupCollection != null && value.fkCompanyGroupID != null ? GroupCollection.First(p => p.pkCompanyGroupID == value.fkCompanyGroupID) : null;
                    SelectedWBSNumber = value.WBSNumber;
                    SelectedCostCode = value.CostCode;
                    SelectedPONumber = value.PONumber != null ? value.PONumber : string.Empty;
                    SelectedPOExpiryDate = value.POExpiryDate != null ? value.POExpiryDate.Value : DateTime.MinValue;
                    HasSplitBilling = value.HasSpitBilling;
                    SelectedIPAddress = value.IPAddress;
                    CompanyState = value.IsActive;
                    ReadCompanyBillingLevelsAsync();
                }
            }
        }
        private Company _selectedCompany;

        /// <summary>
        /// The entered company IP Adrress
        /// </summary>
        public string SelectedIPAddress
        {
            get { return _selectedIPAddress; }
            set { SetProperty(ref _selectedIPAddress, value); }
        }
        private string _selectedIPAddress = string.Empty;

        /// <summary>
        /// The entered company PO number
        /// </summary>
        public string SelectedPONumber
        {
            get { return _selectedPONumber; }
            set { SetProperty(ref _selectedPONumber, value); }
        }
        private string _selectedPONumber = string.Empty;

        /// <summary>
        /// The entered company PO expiry date
        /// </summary>
        public DateTime SelectedPOExpiryDate
        {
            get { return _selectedPOExpiryDate; }
            set { SetProperty(ref _selectedPOExpiryDate, value); }
        }
        private DateTime _selectedPOExpiryDate = DateTime.MinValue;

        /// <summary>
        /// Indicate if the company has a PO Number
        /// </summary>
        public bool HasPONumber
        {
            get { return _hasPONumber; }
            set { SetProperty(ref _hasPONumber, value); }
        }
        private bool _hasPONumber;

        /// <summary>
        /// The selected company state
        /// </summary>
        public bool CompanyState
        {
            get { return _companyState; }
            set { SetProperty(ref _companyState, value); }
        }
        private bool _companyState;

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
        /// The collection of company billing level from the database
        /// </summary>
        public List<CompanyBillingLevel> CompanyBillingLevelCollection
        {
            get { return _companyBillingLevelCollection; }
            set { SetProperty(ref _companyBillingLevelCollection, value); }
        }
        private List<CompanyBillingLevel> _companyBillingLevelCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of company groups from the database
        /// </summary>
        public ObservableCollection<CompanyGroup> GroupCollection
        {
            get { return _groupCollection; }
            set { SetProperty(ref _groupCollection, value); }
        }
        private ObservableCollection<CompanyGroup> _groupCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered company name
        /// </summary>
        public string SelectedCompanyName
        {
            get { return _selectedCompanyName; }
            set { SetProperty(ref _selectedCompanyName, value); }
        }
        private string _selectedCompanyName = string.Empty;

        /// <summary>
        /// The selected company group
        /// </summary>
        public CompanyGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set { SetProperty(ref _selectedGroup, value); }
        }
        private CompanyGroup _selectedGroup;

        /// <summary>
        /// The entered wbs number
        /// </summary>
        public string SelectedWBSNumber
        {
            get { return _selectedWBSNumber; }
            set { SetProperty(ref _selectedWBSNumber, value); }
        }
        private string _selectedWBSNumber;

        /// <summary>
        /// The entered cost code
        /// </summary>
        public string SelectedCostCode
        {
            get { return _selectedCostCode; }
            set { SetProperty(ref _selectedCostCode, value); }
        }
        private string _selectedCostCode;

        /// <summary>
        /// Indicate if the company has split billing
        /// </summary>
        public bool HasSplitBilling
        {
            get { return _hasSplitBilling; }
            set { SetProperty(ref _hasSplitBilling, value); }
        }
        private bool _hasSplitBilling;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCompanyName
        {
            get { return _validCompanyName; }
            set { SetProperty(ref _validCompanyName, value); }
        }
        private Brush _validCompanyName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidGroup
        {
            get { return _validGroup; }
            set { SetProperty(ref _validGroup, value); }
        }
        private Brush _validGroup = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidWBSNumber
        {
            get { return _validWBSNumber; }
            set { SetProperty(ref _validWBSNumber, value); }
        }
        private Brush _validWBSNumber = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCostCode
        {
            get { return _validCostCode; }
            set { SetProperty(ref _validCostCode, value); }
        }
        private Brush _validCostCode = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidPOExpiryDate
        {
            get { return _validPOExpiryDate; }
            set { SetProperty(ref _validPOExpiryDate, value); }
        }
        private Brush _validPOExpiryDate = Brushes.Red;

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
                    case "SelectedCompanyName":
                        ValidCompanyName = string.IsNullOrEmpty(SelectedCompanyName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedGroup":
                        ValidGroup = SelectedGroup != null && SelectedGroup.pkCompanyGroupID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedWBSNumber":
                        ValidWBSNumber = string.IsNullOrEmpty(SelectedWBSNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCostCode":
                        ValidCostCode = string.IsNullOrEmpty(SelectedCostCode) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedPONumber":
                       if (string.IsNullOrEmpty(SelectedPONumber))
                        {
                            HasPONumber = false;
                            SelectedPOExpiryDate = DateTime.MinValue;
                            ValidPOExpiryDate = Brushes.Silver;
                        }
                        else
                        {
                            HasPONumber = true;
                            SelectedPOExpiryDate = DateTime.Now;
                            ValidPOExpiryDate = Brushes.Red;
                        }
                        break;
                    case "SelectedPOExpiryDate":
                        ValidPOExpiryDate = HasPONumber && SelectedPOExpiryDate < DateTime.Now ? Brushes.Red : Brushes.Silver; break;
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
        public ViewCompanyViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseCompanyView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseCompanyView()
        {
            _model = new CompanyModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedCompanyName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedCompanyName)
                                                                          .ObservesProperty(() => SelectedGroup)
                                                                          .ObservesProperty(() => SelectedWBSNumber)
                                                                          .ObservesProperty(() => SelectedCostCode)
                                                                          .ObservesProperty(() => SelectedPONumber)
                                                                          .ObservesProperty(() => SelectedPOExpiryDate);
            GroupCommand = new DelegateCommand(ExecuteShowCompanyGroupView);

            // Load the view data
            await ReadCompanyGroupsAsync();
            await ReadCompaniesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedCompany = new Company();
            HasSplitBilling = CompanyState = true;
        }

        /// <summary>
        /// Load all the companies from the database
        /// </summary>
        private async Task ReadCompaniesAsync()
        {
            try
            {
                CompanyCollection = await Task.Run(() => _model.ReadCompanies(false, true));

                if (SelectedCompany != null)
                    SelectedCompany = CompanyCollection.Where(p => p.pkCompanyID == SelectedCompany.pkCompanyID).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCompanyViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompaniesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the company groups from the database
        /// </summary>
        private async Task ReadCompanyGroupsAsync()
        {
            try
            {
                GroupCollection = await Task.Run(() => new CompanyGroupModel(_eventAggregator).ReadCompanyGroups(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCompanyViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompaniesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the companie's billing levels from the database
        /// </summary>
        private async void ReadCompanyBillingLevelsAsync()
        {
            try
            {
                if (SelectedCompany.fkCompanyGroupID != null)
                {
                    CompanyBillingLevelCollection = await Task.Run(() => new CompanyBillingLevelModel(_eventAggregator).ReadCompanyBillingLevels(SelectedCompany.fkCompanyGroupID.Value, null, true).ToList());
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCompanyViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompanyBillingLevelsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCancel()
        {
            return !string.IsNullOrWhiteSpace(SelectedCompanyName);
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(SelectedCompanyName) && !string.IsNullOrWhiteSpace(SelectedWBSNumber) &&
                   !string.IsNullOrWhiteSpace(SelectedCostCode) && SelectedGroup != null && SelectedGroup.pkCompanyGroupID > 0 &&
                   (!HasPONumber || (HasPONumber && SelectedPOExpiryDate > DateTime.Now));
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
            SelectedCompany.CompanyName = SelectedCompanyName.ToUpper();
            SelectedCompany.fkCompanyGroupID = SelectedGroup.pkCompanyGroupID;
            SelectedCompany.WBSNumber = SelectedWBSNumber.ToUpper();
            SelectedCompany.CostCode = SelectedCostCode.ToUpper();
            SelectedCompany.PONumber = SelectedPONumber.ToUpper();
            SelectedCompany.POExpiryDate = HasPONumber && SelectedPOExpiryDate > DateTime.MinValue ? SelectedPOExpiryDate : (DateTime?)null;
            SelectedCompany.HasSpitBilling = HasSplitBilling;
            SelectedCompany.ModifiedBy = SecurityHelper.LoggedInUserFullName;
            SelectedCompany.ModifiedDate = DateTime.Now;
            SelectedCompany.IsActive = CompanyState;

            if (SelectedCompany.pkCompanyID == 0)
                result = _model.CreateCompany(SelectedCompany);
            else
                result = _model.UpdateCompany(SelectedCompany);

            if (result)
            {
                InitialiseViewControls();
                await ReadCompaniesAsync();
            }
        }

        /// <summary>
        /// Execute when the company group view command button is clicked 
        /// </summary>
        private async void ExecuteShowCompanyGroupView()
        {
            int selectedGroupID = SelectedGroup.pkCompanyGroupID;
            PopupWindow popupWindow = new PopupWindow(new ViewCompanyGroup(), "Company Group Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadCompanyGroupsAsync();
            SelectedGroup = GroupCollection.Where(p => p.pkCompanyGroupID == selectedGroupID).FirstOrDefault();
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteShowBillingLevelView()
        {
            int companyGroupID = SelectedCompany != null && SelectedCompany.fkCompanyGroupID != null ? SelectedCompany.fkCompanyGroupID.Value : 0;
            int companyID = SelectedCompany != null ? SelectedCompany.pkCompanyID : 0;
            ViewCompanyBillingLevel view = new ViewCompanyBillingLevel();
            ViewCompanyBillingLevelViewModel viewModel = new ViewCompanyBillingLevelViewModel(_eventAggregator, companyGroupID);
            view.DataContext = viewModel;
            PopupWindow popupWindow = new PopupWindow(view, "Company Billing Level Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            Task.Run(() => ReadCompaniesAsync());
        }

        #endregion

        #endregion
    }
}
