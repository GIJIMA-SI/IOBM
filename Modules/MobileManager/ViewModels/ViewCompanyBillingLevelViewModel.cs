using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewCompanyBillingLevelViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private CompanyBillingLevelModel _model = null;
        private IEventAggregator _eventAggregator;
        private int _companyGroupID = 0;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand BillingLevelCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) company billing level entity
        /// </summary>
        public CompanyBillingLevel SelectedCompanyBillingLevel
        {
            get { return _selectedCompanyBillingLevel; }
            set
            {
                if (value != null)
                {
                    SelectedBillingLevel = BillingLevelCollection != null ? BillingLevelCollection.First(p => p.pkBillingLevelID == value.fkBillingLevelID) : null;
                    BillingAmount = value.Amount > 0 ? value.Amount.ToString() : "0";
                    SetProperty(ref _selectedCompanyBillingLevel, value);
                }
            }
        }
        private CompanyBillingLevel _selectedCompanyBillingLevel = new CompanyBillingLevel();

        /// <summary>
        /// The selected company billing level state
        /// </summary>
        public bool CompanyBillingLevelState
        {
            get { return _billinglevelState; }
            set { SetProperty(ref _billinglevelState, value); }
        }
        private bool _billinglevelState;

        /// <summary>
        /// The collection of companies from the database
        /// </summary>
        public ObservableCollection<CompanyBillingLevel> CompanyBillingLevelCollection
        {
            get { return _companyBillingLevelCollection; }
            set { SetProperty(ref _companyBillingLevelCollection, value); }
        }
        private ObservableCollection<CompanyBillingLevel> _companyBillingLevelCollection = null;

        #region Required Fields

        /// <summary>
        /// The selected billing level
        /// </summary>
        public BillingLevel SelectedBillingLevel
        {
            get { return _selectedBillingLevel; }
            set { SetProperty(ref _selectedBillingLevel, value); }
        }
        private BillingLevel _selectedBillingLevel;

        /// <summary>
        /// The entered billing amount
        /// </summary>
        public string BillingAmount
        {
            get { return _billingAmount; }
            set { SetProperty(ref _billingAmount, value); }
        }
        private string _billingAmount;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of billing levels from the database
        /// </summary>
        public ObservableCollection<BillingLevel> BillingLevelCollection
        {
            get { return _billingLevelCollection; }
            set { SetProperty(ref _billingLevelCollection, value); }
        }
        private ObservableCollection<BillingLevel> _billingLevelCollection = null;

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

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidBillingLevel
        {
            get { return _validBillingLevel; }
            set { SetProperty(ref _validBillingLevel, value); }
        }
        private Brush _validBillingLevel = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidBillingAmount
        {
            get { return _validBillingAmount; }
            set { SetProperty(ref _validBillingAmount, value); }
        }
        private Brush _validBillingAmount = Brushes.Red;

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
                    case "SelectedBillingLevel":
                        ValidBillingLevel = SelectedBillingLevel != null && SelectedBillingLevel.pkBillingLevelID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "BillingAmount":
                        ValidBillingAmount = string.IsNullOrWhiteSpace(BillingAmount) || Convert.ToDecimal(BillingAmount) < 1 ? Brushes.Red : Brushes.Silver; break;
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
        public ViewCompanyBillingLevelViewModel(IEventAggregator eventAggragator, int companyGroupID = 0)
        {           
            _eventAggregator = eventAggragator;
            _companyGroupID = companyGroupID;
            InitialiseCompanyBillingLevelView();
         }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseCompanyBillingLevelView()
        {
            _model = new CompanyBillingLevelModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedBillingLevel);
            AddCommand = new DelegateCommand(ExecuteAdd);
            DeleteCommand = new DelegateCommand(ExecuteDelete, CanExecuteDelete).ObservesProperty(() => SelectedCompanyBillingLevel);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedBillingLevel)
                                                                          .ObservesProperty(() => BillingAmount);
            BillingLevelCommand = new DelegateCommand(ExecuteShowBillingLevelView, CanExecuteMaintenace);

            // Load the view data
            await ReadCompanyBillingLevelsAsync();
            await ReadBillingLevelsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedCompanyBillingLevel = new CompanyBillingLevel();
        }

        /// <summary>
        /// Load all the companie's billing levels from the database
        /// </summary>
        private async Task ReadCompanyBillingLevelsAsync()
        {
            try
            {
                CompanyBillingLevelCollection = await Task.Run(() => _model.ReadCompanyBillingLevels(_companyGroupID, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Load all the billing levels from the database
        /// </summary>
        private async Task ReadBillingLevelsAsync()
        {
            try
            {
                BillingLevelCollection = await Task.Run(() => new BillingLevelModel(_eventAggregator).ReadBillingLevels(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCancel()
        {
            return SelectedBillingLevel != null && SelectedBillingLevel.pkBillingLevelID > 0;
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
        private bool CanExecuteDelete()
        {
            return SelectedCompanyBillingLevel.pkCompanyBillingLevelID > 0;
        }

        /// <summary>
        /// Execute when the delete command button is clicked 
        /// </summary>
        private async void ExecuteDelete()
        {
            _model.DeleteCompanyBillingLevel(SelectedCompanyBillingLevel.pkCompanyBillingLevelID);
            InitialiseViewControls();
            await ReadCompanyBillingLevelsAsync();
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return SelectedBillingLevel != null &&
                   SelectedBillingLevel.pkBillingLevelID > 0 && 
                   !string.IsNullOrWhiteSpace(BillingAmount) && Convert.ToDecimal(BillingAmount) > 0;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;
            SelectedCompanyBillingLevel.fkCompanyGroupID = _companyGroupID;
            SelectedCompanyBillingLevel.fkBillingLevelID = SelectedBillingLevel.pkBillingLevelID;
            SelectedCompanyBillingLevel.Amount = Convert.ToDecimal(BillingAmount);
            SelectedCompanyBillingLevel.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedCompanyBillingLevel.ModifiedDate = DateTime.Now;

            if (SelectedCompanyBillingLevel.pkCompanyBillingLevelID == 0)
                result = _model.CreateCompanyBillingLevel(_companyGroupID, SelectedCompanyBillingLevel);
            else
                result = _model.UpdateCompanyBillingLevel(_companyGroupID, SelectedCompanyBillingLevel);

            if (result)
            {
                InitialiseViewControls();
                await ReadCompanyBillingLevelsAsync();
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
        /// Execute when the billing view command button is clicked 
        /// </summary>
        private async void ExecuteShowBillingLevelView()
        {
            int selectedBillingLevelID = SelectedBillingLevel.pkBillingLevelID;
            PopupWindow popupWindow = new PopupWindow(new ViewBillingLevel(), "Billing Level Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadBillingLevelsAsync();
            SelectedBillingLevel = BillingLevelCollection.Where(p => p.pkBillingLevelID == selectedBillingLevelID).FirstOrDefault();
        }

        #endregion

        #endregion
    }
}
