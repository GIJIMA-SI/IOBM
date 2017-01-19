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
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewClientSiteViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ClientLocationModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) client location entity
        /// </summary>
        public ClientLocation SelectedLocation
        {
            get { return _selectedLocation; }
            set
            {
                if (value != null)
                {
                    SelectedLocationName = value.LocationDescription;
                    LocationState = value.IsActive;
                    SetProperty(ref _selectedLocation, value);
                }
            }
        }
        private ClientLocation _selectedLocation = new ClientLocation();

        /// <summary>
        /// The selected location state
        /// </summary>
        public bool LocationState
        {
            get { return _locationState; }
            set { SetProperty(ref _locationState, value); }
        }
        private bool _locationState;

        /// <summary>
        /// The collection of locations from the database
        /// </summary>
        public ObservableCollection<ClientLocation> LocationCollection
        {
            get { return _locationCollection; }
            set { SetProperty(ref _locationCollection, value); }
        }
        private ObservableCollection<ClientLocation> _locationCollection = null;

        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered location description
        /// </summary>
        public string SelectedLocationName
        {
            get { return _selectedlocationName; }
            set { SetProperty(ref _selectedlocationName, value); }
        }
        private string _selectedlocationName = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidLocationName
        {
            get { return _validLocationName; }
            set { SetProperty(ref _validLocationName, value); }
        }
        private Brush _validLocationName = Brushes.Red;

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
                    case "SelectedLocationName":
                        ValidLocationName = string.IsNullOrEmpty(SelectedLocationName) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewClientSiteViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseClientSiteView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseClientSiteView()
        {
            _model = new ClientLocationModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => SelectedLocationName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => SelectedLocationName);

            // Load the view data
            await ReadClientLocationsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedLocation = new ClientLocation();
        }

        /// <summary>
        /// Load all the client locations from the database
        /// </summary>
        private async Task ReadClientLocationsAsync()
        {
            try
            {
                LocationCollection = await Task.Run(() => _model.ReadClientLocations(false, true));
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
            return !string.IsNullOrWhiteSpace(SelectedLocationName);
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
            SelectedLocation.LocationDescription = SelectedLocationName.ToUpper();
            SelectedLocation.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedLocation.ModifiedDate = DateTime.Now;
            SelectedLocation.IsActive = LocationState;

            if (SelectedLocation.pkClientLocationID == 0)
                result = _model.CreateClientLocation(SelectedLocation);
            else
                result = _model.UpdateClientLocation(SelectedLocation);

            if (result)
            {
                InitialiseViewControls();
                await ReadClientLocationsAsync();
            }
        }

        #endregion

        #endregion
    }
}

