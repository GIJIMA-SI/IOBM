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
    public class ViewProvinceViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ProvinceModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) province entity
        /// </summary>
        public Province SelectedProvince
        {
            get { return _selectedProvince; }
            set
            {
                if (value != null)
                {
                    SelectedProvinceName = value.ProvinceName;
                    ProvinceState = value.IsActive;
                    SetProperty(ref _selectedProvince, value);
                }
            }
        }
        private Province _selectedProvince = new Province();

        /// <summary>
        /// The collection of provinces from from the database
        /// </summary>
        public ObservableCollection<Province> ProvinceCollection
        {
            get { return _provinceCollection; }
            set { SetProperty(ref _provinceCollection, value); }
        }
        private ObservableCollection<Province> _provinceCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered province name
        /// </summary>
        public string SelectedProvinceName
        {
            get { return _provinceName; }
            set { SetProperty(ref _provinceName, value); }
        }
        private string _provinceName = string.Empty;

        /// <summary>
        /// The selected province state
        /// </summary>
        public bool ProvinceState
        {
            get { return _provinceState; }
            set { SetProperty(ref _provinceState, value); }
        }
        private bool _provinceState;

        #endregion

        #region View Lookup Data Collections

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidProvinceName
        {
            get { return _validProvinceName; }
            set { SetProperty(ref _validProvinceName, value); }
        }
        private Brush _validProvinceName = Brushes.Red;

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
                    case "SelectedProvinceName":
                        ValidProvinceName = string.IsNullOrEmpty(SelectedProvinceName) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewProvinceViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseProvinceView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseProvinceView()
        {
            _model = new ProvinceModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedProvinceName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedProvinceName);

            // Load the view data
            await ReadProvincesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedProvince = new Province();
            SelectedProvinceName = string.Empty;
            ProvinceState = true;
        }

        /// <summary>
        /// Load all the cities from the database
        /// </summary>
        private async Task ReadProvincesAsync()
        {
            try
            {
                ProvinceCollection = await Task.Run(() => new ProvinceModel(_eventAggregator).ReadProvincees(false, true));
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
        /// Validate if the cancel functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteCancel()
        {
            return !string.IsNullOrWhiteSpace(SelectedProvinceName);
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
            return !string.IsNullOrWhiteSpace(SelectedProvinceName);
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;

            SelectedProvince.ProvinceName = SelectedProvinceName.ToUpper();
            SelectedProvince.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedProvince.ModifiedDate = DateTime.Now;
            SelectedProvince.IsActive = ProvinceState;

            if (SelectedProvince.pkProvinceID == 0)
                result = _model.CreateProvince(SelectedProvince);
            else
                result = _model.UpdateProvince(SelectedProvince);

            if (result)
            {
                InitialiseViewControls();
                await ReadProvincesAsync();
            }
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }
       
        #endregion

        #endregion
    }
}
