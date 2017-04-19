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
using System.Windows;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDeviceModelViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DevicesModelModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand DeviceMakeCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) device make entity
        /// </summary>
        public DeviceModel SelectedDeviceModel
        {
            get { return _selectedDeviceModel; }
            set
            {
                if (value != null)
                {
                    SelectedModelDescription = value.ModelDescription;
                    SelectedDeviceMake = DeviceMakeCollection != null ? DeviceMakeCollection.Where(p => p.pkDeviceMakeID == value.fkDeviceMakeID).FirstOrDefault() :
                                         DeviceMakeCollection != null ? DeviceMakeCollection.Where(p => p.pkDeviceMakeID == 0).FirstOrDefault() : null;
                    ModelState = value.IsActive;
                    SetProperty(ref _selectedDeviceModel, value);
                }
            }
        }
        private DeviceModel _selectedDeviceModel = new DeviceModel();

        /// <summary>
        /// The selected DeviceModel state
        /// </summary>
        public bool ModelState
        {
            get { return _modelState; }
            set { SetProperty(ref _modelState, value); }
        }
        private bool _modelState;

        /// <summary>
        /// The collection of DeviceModel links from the DeviceModelLink enum
        /// </summary>
        public ObservableCollection<DeviceModel> DeviceModelCollection
        {
            get { return _deviceModelCollection; }
            set { SetProperty(ref _deviceModelCollection, value); }
        }
        private ObservableCollection<DeviceModel> _deviceModelCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of device makes from the database
        /// </summary>
        public ObservableCollection<DeviceMake> DeviceMakeCollection
        {
            get { return _deviceMakeCollection; }
            set { SetProperty(ref _deviceMakeCollection, value); }
        }
        private ObservableCollection<DeviceMake> _deviceMakeCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered DeviceModel description
        /// </summary>
        public string SelectedModelDescription
        {
            get { return _selectedModelDescription; }
            set { SetProperty(ref _selectedModelDescription, value); }
        }
        private string _selectedModelDescription = string.Empty;

        /// <summary>
        /// The selected device make link to the device model
        /// </summary>
        public DeviceMake SelectedDeviceMake
        {
            get { return _selectedDeviceMake; }
            set { SetProperty(ref _selectedDeviceMake, value); }
        }
        private DeviceMake _selectedDeviceMake;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidModelDescription
        {
            get { return _validModelDescription; }
            set { SetProperty(ref _validModelDescription, value); }
        }
        private Brush _validModelDescription = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDeviceMake
        {
            get { return _validDeviceMake; }
            set { SetProperty(ref _validDeviceMake, value); }
        }
        private Brush _validDeviceMake = Brushes.Red;

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
                    case "SelectedModelDescription":
                        ValidModelDescription = string.IsNullOrEmpty(SelectedModelDescription) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDeviceMake":
                        ValidDeviceMake = SelectedDeviceMake != null && SelectedDeviceMake.pkDeviceMakeID < 1 ? Brushes.Red : Brushes.Silver; break;
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
        public ViewDeviceModelViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseDeviceModelView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDeviceModelView()
        {
            _model = new DevicesModelModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedModelDescription);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedModelDescription)
                                                                          .ObservesProperty(() => SelectedDeviceMake);
            DeviceMakeCommand = new DelegateCommand(ExecuteShowMakeView, CanExecuteMaintenace);

            // Load the view data
            await ReadDeviceModelsAsync();
            await ReadDeviceMakesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedDeviceModel = new DeviceModel();
        }

        /// <summary>
        /// Load all the device makes from the database
        /// </summary>
        private async Task ReadDeviceModelsAsync()
        {
            try
            {
                DeviceModelCollection = await Task.Run(() => _model.ReadDeviceModels(false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the device makes from the database
        /// </summary>
        private async Task ReadDeviceMakesAsync()
        {
            try
            {
                DeviceMakeCollection = await Task.Run(() => new DevicesMakeModel(_eventAggregator).ReadDeviceMakes(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
            return !string.IsNullOrWhiteSpace(SelectedModelDescription);
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
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(SelectedModelDescription) && (SelectedDeviceMake != null && SelectedDeviceMake.pkDeviceMakeID > 0);
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;
            SelectedDeviceModel.ModelDescription = SelectedModelDescription.ToUpper();
            SelectedDeviceModel.fkDeviceMakeID = SelectedDeviceMake.pkDeviceMakeID;
            SelectedDeviceModel.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedDeviceModel.ModfiedDate = DateTime.Now;
            SelectedDeviceModel.IsActive = ModelState;

            if (SelectedDeviceModel.pkDeviceModelID == 0)
                result = _model.CreateDeviceModel(SelectedDeviceModel);
            else
                result = _model.UpdateDeviceModel(SelectedDeviceModel);

            if (result)
            {
                InitialiseViewControls();
                await ReadDeviceModelsAsync();
            }
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
        /// Execute show device make maintenance view
        /// </summary>
        private async void ExecuteShowMakeView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewDeviceMake(), "Device Make Maintenance", PopupWindow.PopupButtonType.Close);
            //So width and heigh can be set auto for referencedata
            popupWindow.MaxHeight = popupWindow.MinHeight = 300;
            popupWindow.MaxWidth = popupWindow.MinWidth = 400;
            popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            popupWindow.ShowDialog();
            await ReadDeviceMakesAsync();
        }

        #endregion

        #endregion
    }
}
