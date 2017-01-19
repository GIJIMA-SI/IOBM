using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Linq;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Security;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewSuburbViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private SuburbModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CityCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) suburb entity
        /// </summary>
        public Suburb SelectedSuburb
        {
            get { return _selectedSuburb; }
            set
            {
                if (value != null)
                {
                    SelectedSuburbName = value.SuburbName;
                    SelectedCity = CityCollection.First(p => p.pkCityID == value.fkCityID);
                    SelectedPostalCode = value.PostalCode;
                    SuburbState = value.IsActive;
                    SetProperty(ref _selectedSuburb, value);
                }
            }
        }
        private Suburb _selectedSuburb = new Suburb();

        /// <summary>
        /// The collection of suburbs from from the database
        /// </summary>
        public ObservableCollection<Suburb> SuburbCollection
        {
            get { return _suburbCollection; }
            set { SetProperty(ref _suburbCollection, value); }
        }
        private ObservableCollection<Suburb> _suburbCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered suburb name
        /// </summary>
        public string SelectedSuburbName
        {
            get { return _suburbName; }
            set { SetProperty(ref _suburbName, value); }
        }
        private string _suburbName = string.Empty;

        /// <summary>
        /// The selected city
        /// </summary>
        public City SelectedCity
        {
            get { return _selectedCity; }
            set
            {

                SetProperty(ref _selectedCity, value);
            }
        }
        private City _selectedCity;

        /// <summary>
        /// The entered postal code
        /// </summary>
        public string SelectedPostalCode
        {
            get { return _postalCode; }
            set { SetProperty(ref _postalCode, value); }
        }
        private string _postalCode;

        /// <summary>
        /// The selected suburb state
        /// </summary>
        public bool SuburbState
        {
            get { return _suburbState; }
            set { SetProperty(ref _suburbState, value); }
        }
        private bool _suburbState;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of cities from the database
        /// </summary>
        public ObservableCollection<City> CityCollection
        {
            get { return _cityCollection; }
            set { SetProperty(ref _cityCollection, value); }
        }
        private ObservableCollection<City> _cityCollection = null;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSuburbName
        {
            get { return _validSuburbName; }
            set { SetProperty(ref _validSuburbName, value); }
        }
        private Brush _validSuburbName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidPostalCode
        {
            get { return _validPostalCode; }
            set { SetProperty(ref _validPostalCode, value); }
        }
        private Brush _validPostalCode = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCity
        {
            get { return _validCity; }
            set { SetProperty(ref _validCity, value); }
        }
        private Brush _validCity = Brushes.Red;

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
                    case "SelectedSuburbName":
                        ValidSuburbName = string.IsNullOrEmpty(SelectedSuburbName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedPostalCode":
                        ValidPostalCode = string.IsNullOrEmpty(SelectedPostalCode) || SelectedSuburbName.Length < 4 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCity":
                        ValidCity = SelectedCity != null && SelectedCity.pkCityID < 1 ? Brushes.Red : Brushes.Silver; break;
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
        public ViewSuburbViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseSuburbView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseSuburbView()
        {
            _model = new SuburbModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedSuburbName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedSuburbName)
                                                                          .ObservesProperty(() => SelectedCity)
                                                                          .ObservesProperty(() => SelectedPostalCode);
            CityCommand = new DelegateCommand(ExecuteShowCityView, CanExecuteMaintenace);

            // Load the view data
            await ReadSuburbsAsync();
            await ReadCitiesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedSuburb = new Suburb();
            SelectedSuburbName = SelectedPostalCode = string.Empty;
            SuburbState = true;
        }

        /// <summary>
        /// Load all the suburbs from the database
        /// </summary>
        private async Task ReadSuburbsAsync()
        {
            try
            {
                SuburbCollection = await Task.Run(() => new SuburbModel(_eventAggregator).ReadSuburbes(false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the cities from the database
        /// </summary>
        private async Task ReadCitiesAsync()
        {
            try
            {
                CityCollection = await Task.Run(() => new CityModel(_eventAggregator).ReadCityes(false));
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
            return !string.IsNullOrWhiteSpace(SelectedSuburbName);
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
            return !string.IsNullOrWhiteSpace(SelectedSuburbName) && SelectedCity.pkCityID > 0
                                                                  && !string.IsNullOrWhiteSpace(SelectedPostalCode);
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;

            SelectedSuburb.SuburbName = SelectedSuburbName.ToUpper();
            SelectedSuburb.fkCityID = SelectedCity.pkCityID;
            SelectedSuburb.PostalCode = SelectedPostalCode;
            SelectedSuburb.ModifiedBy = SecurityHelper.LoggedInUserFullName;
            SelectedSuburb.ModifiedDate = DateTime.Now;
            SelectedSuburb.IsActive = SuburbState;

            if (SelectedSuburb.pkSuburbID == 0)
                result = _model.CreateSuburb(SelectedSuburb);
            else
                result = _model.UpdateSuburb(SelectedSuburb);

            if (result)
            {
                InitialiseViewControls();
                await ReadSuburbsAsync();
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
        /// Execute when the city maintenance command button is clicked 
        /// </summary>
        private async void ExecuteShowCityView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewCity(), "City Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadCitiesAsync();
        }

        #endregion

        #endregion
    }
}
