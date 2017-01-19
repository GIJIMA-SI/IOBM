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
    public class ViewServiceProviderViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ServiceProviderModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) service provider entity
        /// </summary>
        public ServiceProvider SelectedSP
        {
            get { return _selectedSP; }
            set
            {
                if (value != null)
                {
                    SelectedSPName = value.ServiceProviderName;
                    SPState = value.IsActive;
                    SetProperty(ref _selectedSP, value);
                }
            }
        }
        private ServiceProvider _selectedSP = new ServiceProvider();

        /// <summary>
        /// The selected service provider state
        /// </summary>
        public bool SPState
        {
            get { return _spState; }
            set { SetProperty(ref _spState, value); }
        }
        private bool _spState;

        /// <summary>
        /// The collection of service providers from the database
        /// </summary>
        public ObservableCollection<ServiceProvider> SPCollection
        {
            get { return _spCollection; }
            set { SetProperty(ref _spCollection, value); }
        }
        private ObservableCollection<ServiceProvider> _spCollection = null;

        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered service provider name
        /// </summary>
        public string SelectedSPName
        {
            get { return _selectedSPName; }
            set { SetProperty(ref _selectedSPName, value); }
        }
        private string _selectedSPName = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSPName
        {
            get { return _validSPName; }
            set { SetProperty(ref _validSPName, value); }
        }
        private Brush _validSPName = Brushes.Red;

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
                    case "SelectedSPName":
                        ValidSPName = string.IsNullOrEmpty(SelectedSPName) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewServiceProviderViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseServiceProviderView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseServiceProviderView()
        {
            _model = new ServiceProviderModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => SelectedSPName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => SelectedSPName);

            // Load the view data
            await ReadServiceProvidersAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedSP = new ServiceProvider();
        }

        /// <summary>
        /// Load all the service providers from the database
        /// </summary>
        private async Task ReadServiceProvidersAsync()
        {
            try
            {
                SPCollection = await Task.Run(() => _model.ReadServiceProviders(false, true));
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
            return !string.IsNullOrWhiteSpace(SelectedSPName);
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
            SelectedSP.ServiceProviderName = SelectedSPName.ToUpper();
            SelectedSP.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedSP.ModifiedDate = DateTime.Now;
            SelectedSP.IsActive = SPState;

            if (SelectedSP.pkServiceProviderID == 0)
                result = _model.CreateServiceProvider(SelectedSP);
            else
                result = _model.UpdateServiceProvider(SelectedSP);

            if (result)
            {
                InitialiseViewControls();
                await ReadServiceProvidersAsync();
            }
        }

        #endregion

        #endregion
    }
}
