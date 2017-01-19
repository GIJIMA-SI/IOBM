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

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewCityViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private CityModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CityCommand { get; set; }
        public DelegateCommand ProvinceCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) city entity
        /// </summary>
        public City SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                if (value != null)
                {
                    SelectedCityName = value.CityName;
                    SelectedProvince = ProvinceCollection.First(p => p.pkProvinceID == value.fkProvinceID);
                    CityState = value.IsActive;
                    SetProperty(ref _selectedCity, value);
                }
            }
        }
        private City _selectedCity = new City();

        /// <summary>
        /// The collection of citys from from the database
        /// </summary>
        public ObservableCollection<City> CityCollection
        {
            get { return _cityCollection; }
            set { SetProperty(ref _cityCollection, value); }
        }
        private ObservableCollection<City> _cityCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered city name
        /// </summary>
        public string SelectedCityName
        {
            get { return _cityName; }
            set { SetProperty(ref _cityName, value); }
        }
        private string _cityName = string.Empty;

        /// <summary>
        /// The selected province
        /// </summary>
        public Province SelectedProvince
        {
            get { return _selectedProvince; }
            set { SetProperty(ref _selectedProvince, value); }
        }
        private Province _selectedProvince;

        /// <summary>
        /// The selected city state
        /// </summary>
        public bool CityState
        {
            get { return _cityState; }
            set { SetProperty(ref _cityState, value); }
        }
        private bool _cityState;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of provinces from the database
        /// </summary>
        public ObservableCollection<Province> ProvinceCollection
        {
            get { return _provinceCollection; }
            set { SetProperty(ref _provinceCollection, value); }
        }
        private ObservableCollection<Province> _provinceCollection = null;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCityName
        {
            get { return _validCityName; }
            set { SetProperty(ref _validCityName, value); }
        }
        private Brush _validCityName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidProvince
        {
            get { return _validProvince; }
            set { SetProperty(ref _validProvince, value); }
        }
        private Brush _validProvince = Brushes.Red;

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
                    case "SelectedCityName":
                        ValidCityName = string.IsNullOrEmpty(SelectedCityName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedProvince":
                        ValidProvince = SelectedProvince != null && SelectedProvince.pkProvinceID < 1 ? Brushes.Red : Brushes.Silver; break;
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
        public ViewCityViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseCityView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseCityView()
        {
            _model = new CityModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedCityName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedCityName)
                                                                          .ObservesProperty(() => SelectedProvince);
            ProvinceCommand = new DelegateCommand(ExecuteShowProvinceView, CanExecuteMaintenace);

            // Load the view data
            await ReadCitysAsync();
            await ReadProvincesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedCity = new City();
        }

        /// <summary>
        /// Load all the cities from the database
        /// </summary>
        private async Task ReadCitysAsync()
        {
            try
            {
                CityCollection = await Task.Run(() => new CityModel(_eventAggregator).ReadCityes(false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the provinces from the database
        /// </summary>
        private async Task ReadProvincesAsync()
        {
            try
            {
                ProvinceCollection = await Task.Run(() => new ProvinceModel(_eventAggregator).ReadProvincees(false));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
            return !string.IsNullOrWhiteSpace(SelectedCityName);
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
            return !string.IsNullOrWhiteSpace(SelectedCityName) && SelectedProvince.pkProvinceID > 0;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;

            SelectedCity.CityName = SelectedCityName.ToUpper();
            SelectedCity.fkProvinceID = SelectedProvince.pkProvinceID;
            SelectedCity.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedCity.ModifiedDate = DateTime.Now;
            SelectedCity.IsActive = CityState;

            if (SelectedCity.pkCityID == 0)
                result = _model.CreateCity(SelectedCity);
            else
                result = _model.UpdateCity(SelectedCity);

            if (result)
            {
                InitialiseViewControls();
                await ReadCitysAsync();
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
        /// Execute when the province maintenance command button is clicked 
        /// </summary>
        private async void ExecuteShowProvinceView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewProvince(), "Province Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadProvincesAsync();
        }

        #endregion

        #endregion
    }
}
