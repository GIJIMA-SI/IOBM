using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewContractServiceViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ContractServiceModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) contract service entity
        /// </summary>
        public ContractService SelectedContractService
        {
            get { return _selectedContractService; }
            set
            {
                if (value != null)
                {
                    SelectedContractServiceDescription = value.ServiceDescription;
                    ServiceState = value.IsActive;
                    SetProperty(ref _selectedContractService, value);
                }
            }
        }
        private ContractService _selectedContractService = new ContractService();

        /// <summary>
        /// The selected ContractService state
        /// </summary>
        public bool ServiceState
        {
            get { return _serviceState; }
            set { SetProperty(ref _serviceState, value); }
        }
        private bool _serviceState;

        /// <summary>
        /// The collection of ContractServices from the database
        /// </summary>
        public ObservableCollection<ContractService> ContractServiceCollection
        {
            get { return _ContractServiceCollection; }
            set { SetProperty(ref _ContractServiceCollection, value); }
        }
        private ObservableCollection<ContractService> _ContractServiceCollection = null;

        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered ContractService description
        /// </summary>
        public string SelectedContractServiceDescription
        {
            get { return _selectedServiceDescription; }
            set { SetProperty(ref _selectedServiceDescription, value); }
        }
        private string _selectedServiceDescription = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidContractServiceDescription
        {
            get { return _validServiceDescription; }
            set { SetProperty(ref _validServiceDescription, value); }
        }
        private Brush _validServiceDescription = Brushes.Red;

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
                    case "SelectedContractServiceDescription":
                        ValidContractServiceDescription = string.IsNullOrEmpty(SelectedContractServiceDescription) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewContractServiceViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseContractServiceView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseContractServiceView()
        {
            _model = new ContractServiceModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => SelectedContractServiceDescription);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => SelectedContractServiceDescription);

            // Load the view data
            await ReadContractServicesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedContractService = new ContractService();
        }

        /// <summary>
        /// Load all the contract services from the database
        /// </summary>
        private async Task ReadContractServicesAsync()
        {
            try
            {
                ContractServiceCollection = await Task.Run(() => _model.ReadContractService(false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecute()
        {
            return !string.IsNullOrWhiteSpace(SelectedContractServiceDescription);
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
            SelectedContractService.ServiceDescription = SelectedContractServiceDescription.ToUpper();
            SelectedContractService.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedContractService.ModifiedDate = DateTime.Now;
            SelectedContractService.IsActive = ServiceState;

            if (SelectedContractService.pkContractServiceID == 0)
                result = _model.CreateContractService(SelectedContractService);
            else
                result = _model.UpdateContractService(SelectedContractService);

            if (result)
            {
                InitialiseViewControls();
                await ReadContractServicesAsync();
            }
        }

        #endregion

        #endregion
    }
}
