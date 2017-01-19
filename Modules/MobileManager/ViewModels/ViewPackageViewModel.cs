using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
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
    public class ViewPackageViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private PackageModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SPCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) package entity
        /// </summary>
        public Package SelectedPackage
        {
            get { return _selectedPackage; }
            set
            {
                if (value != null)
                {
                    SelectedPackageName = value.PackageName;
                    SelectedServiceProvider = ServiceProviderCollection != null && value.fkServiceProviderID != null ? ServiceProviderCollection.First(p => p.pkServiceProviderID == value.fkServiceProviderID) :
                                              ServiceProviderCollection != null ? ServiceProviderCollection.First(p => p.pkServiceProviderID == 0) : null;
                    SelectedPackageType = value.enPackageType != null ? ((PackageType)value.enPackageType).ToString() : PackageType.NONE.ToString();
                    SelectedCost = value.Cost;
                    SelectedTalkTime = value.TalkTimeMinutes;
                    SelectedMBData = value.MBData;
                    SelectedSMSCount = value.SMSNumber;
                    SelectedRandValue = value.RandValue;
                    SelectedSPULValue = value.SPULValue;
                    PackageState = value.IsActive;
                    SetProperty(ref _selectedPackage, value);
                }
            }
        }
        private Package _selectedPackage = new Package();

        /// <summary>
        /// The entered talk time in minutes
        /// </summary>
        public int SelectedTalkTime
        {
            get { return _selectedTalkTime; }
            set { SetProperty(ref _selectedTalkTime, value); }
        }
        private int _selectedTalkTime;

        /// <summary>
        /// The entered MB data
        /// </summary>
        public int SelectedMBData
        {
            get { return _selectedMBData; }
            set { SetProperty(ref _selectedMBData, value); }
        }
        private int _selectedMBData;

        /// <summary>
        /// The entered sms count
        /// </summary>
        public int SelectedSMSCount
        {
            get { return _selectedSmsCount; }
            set { SetProperty(ref _selectedSmsCount, value); }
        }
        private int _selectedSmsCount;

        /// <summary>
        /// The entered rand value
        /// </summary>
        public decimal SelectedRandValue
        {
            get { return _selectedRandValue; }
            set { SetProperty(ref _selectedRandValue, value); }
        }
        private decimal _selectedRandValue;

        /// <summary>
        /// The entered SPUL value
        /// </summary>
        public decimal SelectedSPULValue
        {
            get { return _selectedSPULValue; }
            set { SetProperty(ref _selectedSPULValue, value); }
        }
        private decimal _selectedSPULValue;

        /// <summary>
        /// The selected package state
        /// </summary>
        public bool PackageState
        {
            get { return _packageState; }
            set { SetProperty(ref _packageState, value); }
        }
        private bool _packageState;

        /// <summary>
        /// The collection of companies from the database
        /// </summary>
        public ObservableCollection<Package> PackageCollection
        {
            get { return _packageCollection; }
            set { SetProperty(ref _packageCollection, value); }
        }
        private ObservableCollection<Package> _packageCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of service providers from the database
        /// </summary>
        public ObservableCollection<ServiceProvider> ServiceProviderCollection
        {
            get { return _serviceProviderCollection; }
            set { SetProperty(ref _serviceProviderCollection, value); }
        }
        private ObservableCollection<ServiceProvider> _serviceProviderCollection = null;

        /// <summary>
        /// The collection of package types from the PackageType enum
        /// </summary>
        public ObservableCollection<string> PackageTypeCollection
        {
            get { return _packageTypeCollection; }
            set { SetProperty(ref _packageTypeCollection, value); }
        }
        private ObservableCollection<string> _packageTypeCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered package name
        /// </summary>
        public string SelectedPackageName
        {
            get { return _selectedPackageName; }
            set { SetProperty(ref _selectedPackageName, value); }
        }
        private string _selectedPackageName = string.Empty;

        /// <summary>
        /// The selected service provider
        /// </summary>
        public ServiceProvider SelectedServiceProvider
        {
            get { return _selectedServiceProvider; }
            set {SetProperty(ref _selectedServiceProvider, value);}
        }
        private ServiceProvider _selectedServiceProvider;

        /// <summary>
        /// The selected package type
        /// </summary>
        public string SelectedPackageType
        {
            get { return _selectedPackageType; }
            set { SetProperty(ref _selectedPackageType, value); }
        }
        private string _selectedPackageType;

        /// <summary>
        /// The entered monthly cost
        /// </summary>
        public decimal SelectedCost
        {
            get { return _selectedCost; }
            set { SetProperty(ref _selectedCost, value); }
        }
        private decimal _selectedCost;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidPackageName
        {
            get { return _validPackageName; }
            set { SetProperty(ref _validPackageName, value); }
        }
        private Brush _validPackageName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidServiceProvider
        {
            get { return _validServiceProvider; }
            set { SetProperty(ref _validServiceProvider, value); }
        }
        private Brush _validServiceProvider = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidPackageType
        {
            get { return _validPackageType; }
            set { SetProperty(ref _validPackageType, value); }
        }
        private Brush _validPackageType = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCost
        {
            get { return _validCost; }
            set { SetProperty(ref _validCost, value); }
        }
        private Brush _validCost = Brushes.Red;

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
                    case "SelectedPackageName":
                        ValidPackageName = string.IsNullOrEmpty(SelectedPackageName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedServiceProvider":
                        ValidServiceProvider = SelectedServiceProvider == null || SelectedServiceProvider.pkServiceProviderID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedPackageType":
                        ValidPackageType = SelectedPackageType == null || SelectedPackageType == PackageType.NONE.ToString() ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCost":
                        ValidCost = SelectedCost < 1 ? Brushes.Red : Brushes.Silver; break;
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
        public ViewPackageViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialisePackageView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialisePackageView()
        {
            _model = new PackageModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedPackageName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedPackageName)
                                                                          .ObservesProperty(() => SelectedServiceProvider)
                                                                          .ObservesProperty(() => SelectedPackageType)
                                                                          .ObservesProperty(() => SelectedCost);
            SPCommand = new DelegateCommand(ExecuteShowSPView, CanExecuteMaintenace);

            // Load the view data
            await ReadPackagesAsync();
            await ReadServiceProvidersAsync();
            ReadPackageTypes();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedPackage = new Package();
            PackageState = true;
        }

        /// <summary>
        /// Load all the packages from the database
        /// </summary>
        private async Task ReadPackagesAsync()
        {
            try
            {
                PackageCollection = await Task.Run(() => _model.ReadPackages(false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the service providers from the database
        /// </summary>
        private async Task ReadServiceProvidersAsync()
        {
            try
            {
                ServiceProviderCollection = await Task.Run(() => new ServiceProviderModel(_eventAggregator).ReadServiceProviders(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Populate the package types combobox from the PackageType enum
        /// </summary>
        private void ReadPackageTypes()
        {
            PackageTypeCollection = new ObservableCollection<string>();

            foreach (PackageType source in Enum.GetValues(typeof(PackageType)))
            {
                PackageTypeCollection.Add(source.ToString());
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
            return !string.IsNullOrWhiteSpace(SelectedPackageName);
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
            if (SelectedServiceProvider != null)
                return !string.IsNullOrWhiteSpace(SelectedPackageName) && SelectedServiceProvider.pkServiceProviderID > 0
                                                                       && SelectedPackageType != PackageType.NONE.ToString()
                                                                       && SelectedCost > 0;
            else
                return false;
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

            SelectedPackage.PackageName = SelectedPackageName.ToUpper();
            SelectedPackage.fkServiceProviderID = SelectedServiceProvider.pkServiceProviderID;
            SelectedPackage.enPackageType = ((PackageType)Enum.Parse(typeof(PackageType), SelectedPackageType)).Value();
            SelectedPackage.Cost = SelectedCost;
            SelectedPackage.TalkTimeMinutes = SelectedTalkTime;
            SelectedPackage.MBData = SelectedMBData;
            SelectedPackage.SMSNumber = SelectedSMSCount;
            SelectedPackage.RandValue = SelectedRandValue;
            SelectedPackage.SPULValue = SelectedSPULValue;
            SelectedPackage.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedPackage.ModifiedDate = DateTime.Now;
            SelectedPackage.IsActive = PackageState;

            if (SelectedPackage.pkPackageID == 0)
                result = _model.CreatePackage(SelectedPackage);
            else
                result = _model.UpdatePackage(SelectedPackage);

            if (result)
            {
                InitialiseViewControls();
                await ReadPackagesAsync();
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteMaintenace()
        {
            return true;
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private async void ExecuteShowSPView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewServiceProvider(), "Service Provider Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadServiceProvidersAsync();
        }

        #endregion

        #endregion
    }
}
