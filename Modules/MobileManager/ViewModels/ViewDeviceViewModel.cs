using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
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
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDeviceViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DevicesModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private DataActivityLog _activityLogInfo = null;
        private bool _autoSave = false;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand DeviceMakeCommand { get; set; }
        public DelegateCommand DeviceModelCommand { get; set; }
        public DelegateCommand DeviceStatusCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) device entity
        /// </summary>
        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                SetProperty(ref _selectedDevice, value);
                
                // Prevent circullar action
                if (value != null && value.pkDeviceID > 0)
                {
                    // Device required fields
                    SelectedDeviceMake = DeviceMakeCollection != null ? value.fkDeviceMakeID > 0 ? 
                                         DeviceMakeCollection.Where(p => p.pkDeviceMakeID == value.fkDeviceMakeID).FirstOrDefault() :
                                         DeviceMakeCollection.Where(p => p.pkDeviceMakeID == 0).FirstOrDefault() : null;
                    SelectedDeviceModel = DeviceModelCollection != null ? DeviceModelCollection.Where(p => p.pkDeviceModelID == value.fkDeviceModelID).FirstOrDefault() : null;
                    SelectedSimCard = SimCardCollection != null ? value.fkSimCardID != null ?
                                       SimCardCollection.Where(p => p.pkSimCardID == value.fkSimCardID).FirstOrDefault() :
                                       SimCardCollection.Where(p => p.pkSimCardID == 0).FirstOrDefault() : null;
                    SelectedStatus = StatusCollection != null ? StatusCollection.Where(p => p.pkStatusID == value.fkStatusID).FirstOrDefault() : null;
                    SelectedReceivedDate = value.ReceiveDate != null ? value.ReceiveDate : DateTime.MinValue;
                    SelectedIMENumber = value.IMENumber;

                    // Insurance required fields
                    if (value.InsuranceCost != null && value.InsuranceValue != null)
                        SetDeviceInsurance(value.InsuranceCost.Value, value.InsuranceValue.Value);

                    // Link the device to its Simcard
                    //LinkDeviceToSimCard();

                    // Set the selected sim cards for the selected device
                    SetSelectedDeviceSimCard();
                }
            }
        }
        private Device _selectedDevice = new Device();

        /// <summary>
        /// The selected device index
        /// </summary>
        public int SelectedDeviceIndex
        {
            get { return _selectedDeviceIndex; }
            set { SetProperty(ref _selectedDeviceIndex, value); }
        }
        private int _selectedDeviceIndex;

        /// <summary>
        /// The selected Sim card
        /// </summary>
        public SimCard SelectedSimCard
        {
            get { return _selectedSimCard; }
            set { SetProperty(ref _selectedSimCard, value); }
        }
        private SimCard _selectedSimCard;
        
        /// <summary>
        /// The collection of devices from the database
        /// </summary>
        public ObservableCollection<Device> DeviceCollection
        {
            get { return _deviceCollection; }
            set { SetProperty(ref _deviceCollection, value); }
        }
        private ObservableCollection<Device> _deviceCollection = null;

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

        /// <summary>
        /// The collection of device models from the database
        /// </summary>
        public ObservableCollection<DeviceModel> DeviceModelCollection
        {
            get { return _deviceModelCollection; }
            set { SetProperty(ref _deviceModelCollection, value); }
        }
        private ObservableCollection<DeviceModel> _deviceModelCollection = null;

        /// <summary>
        /// The collection of contract Simcards from the database
        /// </summary>
        public ObservableCollection<SimCard> SimCardCollection
        {
            get { return _simCardCollection; }
            set { SetProperty(ref _simCardCollection, value); }
        }
        private ObservableCollection<SimCard> _simCardCollection = null;

        /// <summary>
        /// The collection of statuses from the database
        /// </summary>
        public ObservableCollection<Status> StatusCollection
        {
            get { return _statusCollection; }
            set { SetProperty(ref _statusCollection, value); }
        }
        private ObservableCollection<Status> _statusCollection = null;

        /// <summary>
        /// The collection of the selected device sim cards
        /// </summary>
        public Dictionary<string, object> SelectedDeviceSimCardCollection
        {
            get { return _selectedDeviceSimCardCollection; }
            set { SetProperty(ref _selectedDeviceSimCardCollection, value); }
        }
        private Dictionary<string, object> _selectedDeviceSimCardCollection = null;

        /// <summary>
        /// The collection of sim cards from the database
        /// </summary>
        public Dictionary<string, object> DeviceSimCardCollection
        {
            get { return _deviceSimCardCollection; }
            set { SetProperty(ref _deviceSimCardCollection, value); }
        }
        private Dictionary<string, object> _deviceSimCardCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected device make
        /// </summary>
        public DeviceMake SelectedDeviceMake
        {
            get { return _selectedDeviceMake; }
            set
            {
                int currentMakeID = SelectedDeviceMake != null ? SelectedDeviceMake.pkDeviceMakeID : 0;
                SetProperty(ref _selectedDeviceMake, value);

                if (SelectedDeviceMake != null && SelectedDeviceMake.pkDeviceMakeID != currentMakeID)
                    ReadDeviceMakeModelsAsync();       
            }
        }
        private DeviceMake _selectedDeviceMake;

        /// <summary>
        /// The selected device model
        /// </summary>
        public DeviceModel SelectedDeviceModel
        {
            get { return _selectedDeviceModel; }
            set { SetProperty(ref _selectedDeviceModel, value); }
        }
        private DeviceModel _selectedDeviceModel;

        /// <summary>
        /// The selected status
        /// </summary>
        public Status SelectedStatus
        {
            get { return _selectedStatus; }
            set { SetProperty(ref _selectedStatus, value); }
        }
        private Status _selectedStatus;

        /// <summary>
        /// The selected received date
        /// </summary>
        public DateTime SelectedReceivedDate
        {
            get { return _selectedReceivedDate; }
            set { SetProperty(ref _selectedReceivedDate, value); }
        }
        private DateTime _selectedReceivedDate;

        /// <summary>
        /// The entered IME number
        /// </summary>
        public string SelectedIMENumber
        {
            get { return _selectedIMENumber; }
            set { SetProperty(ref _selectedIMENumber, value); }
        }
        private string _selectedIMENumber;

        /// <summary>
        /// Indicate if a device has insurance
        /// </summary>
        public bool DeviceInsuranceYes
        {
            get { return _deviceInsuranceYes; }
            set { SetProperty(ref _deviceInsuranceYes, value); }
        }
        private bool _deviceInsuranceYes;

        /// <summary>
        /// Indicate if a device has insurance
        /// </summary>
        public bool DeviceInsuranceNo
        {
            get { return _deviceInsuranceNo; }
            set { SetProperty(ref _deviceInsuranceNo, value); }
        }
        private bool _deviceInsuranceNo;

        /// <summary>
        /// The entered insurance value
        /// </summary>
        public decimal SelectedInsuranceValue
        {
            get { return _selectedInsuranceValue; }
            set { SetProperty(ref _selectedInsuranceValue, value); }
        }
        private decimal _selectedInsuranceValue;

        /// <summary>
        /// The entered insurance cost
        /// </summary>
        public decimal SelectedInsuranceCost
        {
            get { return _selectedInsuranceCost; }
            set { SetProperty(ref _selectedInsuranceCost, value); }
        }
        private decimal _selectedInsuranceCost;

        #endregion

        #region Input Validation

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
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDeviceModel
        {
            get { return _validDeviceModel; }
            set { SetProperty(ref _validDeviceModel, value); }
        }
        private Brush _validDeviceModel = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidIMENumber
        {
            get { return _validIMENumber; }
            set { SetProperty(ref _validIMENumber, value); }
        }
        private Brush _validIMENumber = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidStatus
        {
            get { return _validStatus; }
            set { SetProperty(ref _validStatus, value); }
        }
        private Brush _validStatus = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidReceivedDate
        {
            get { return _validReceivedDate; }
            set { SetProperty(ref _validReceivedDate, value); }
        }
        private Brush _validReceivedDate = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidInsurance
        {
            get { return _validInsurance; }
            set { SetProperty(ref _validInsurance, value); }
        }
        private Brush _validInsurance = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidInsuranceValue
        {
            get { return _validInsuranceValue; }
            set { SetProperty(ref _validInsuranceValue, value); }
        }
        private Brush _validInsuranceValue = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidInsuranceCost
        { 
            get { return _validInsuranceCost; }
            set { SetProperty(ref _validInsuranceCost, value); }
        }
        private Brush _validInsuranceCost = Brushes.Red;

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
                    case "SelectedDeviceMake":
                        ValidDeviceMake = SelectedDeviceMake != null && SelectedDeviceMake.pkDeviceMakeID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDeviceModel":
                        ValidDeviceModel = SelectedDeviceModel == null || SelectedDeviceModel.pkDeviceModelID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedStatus":
                        ValidStatus = SelectedStatus == null || SelectedStatus.pkStatusID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedReceivedDate":
                        ValidReceivedDate = SelectedReceivedDate.Date == DateTime.MinValue.Date ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedIMENumber":
                        ValidIMENumber = string.IsNullOrEmpty(SelectedIMENumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedInsuranceValue":
                        ValidInsuranceValue = DeviceInsuranceYes && SelectedInsuranceValue < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedInsuranceCost":
                        ValidInsuranceCost = DeviceInsuranceYes && SelectedInsuranceCost < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "DeviceInsuranceYes":
                        ValidInsuranceCost = DeviceInsuranceYes && SelectedInsuranceCost < 1 ? Brushes.Red : Brushes.Silver;
                        ValidInsuranceValue = DeviceInsuranceYes && SelectedInsuranceValue < 1 ? Brushes.Red : Brushes.Silver;
                        ValidInsurance = !DeviceInsuranceYes && !DeviceInsuranceNo ? Brushes.Red : Brushes.Silver; break;
                    case "DeviceInsuranceNo":
                        if (DeviceInsuranceNo)
                        {
                            ValidInsuranceValue = ValidInsuranceCost = Brushes.Silver;
                            SelectedInsuranceValue = SelectedInsuranceCost = 0;
                        }
                        ValidInsurance = !DeviceInsuranceYes && !DeviceInsuranceNo ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Load all the device linked to the selected contract from the database
        /// </summary>
        /// <param name="sender">The selected client primary cell number.</param>
        private void ReadContractDevices_Event(int sender)
        {
            ReadContractDevicesAsync();
        }
        
        /// <summary>
        /// Save the device data
        /// </summary>
        /// <param name="sender">The selected client contract ID.</param>
        private void SaveContractDevices_Event(int sender)
        {
            if (CanExecuteSave())
            {
                _autoSave = true;
                ExecuteSave();
                _autoSave = false;
            }
        }

        /// <summary>
        /// Call the process based on the sender's completed action.
        /// </summary>
        /// <param name="sender">The completed action.</param>
        private void ActionCompleted_Event(ActionCompleted sender)
        {
            switch (sender)
            {
                case ActionCompleted.ReadContractSimCards:
                    LinkDeviceToSimCard();
                    break;
            }
        }

        /// <summary>
        /// Link the selected Simcard on the Simcard view to its device
        /// </summary>
        /// <param name="sender">The selected Sim card id.</param>
        private void LinkSimCardToDevice_Event(int sender)
        {
            if (DeviceCollection != null && DeviceCollection.Count > 0)
            {
                if (sender > 0 && DeviceCollection.Any(p => p.fkSimCardID != null && p.fkSimCardID.Value == sender))
                    SelectedDevice = DeviceCollection.Where(p => p.fkSimCardID != null && p.fkSimCardID.Value == sender).FirstOrDefault();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDeviceViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseDeviceView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDeviceView()
        {
            _model = new DevicesModel(_eventAggregator);
            _securityHelper = new SecurityHelper(_eventAggregator);
             InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedDeviceMake);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedDevice);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedDeviceMake)
                                                                          .ObservesProperty(() => SelectedDeviceModel)
                                                                          .ObservesProperty(() => SelectedIMENumber)                                                                       
                                                                          .ObservesProperty(() => SelectedReceivedDate)
                                                                          .ObservesProperty(() => ValidInsurance)
                                                                          .ObservesProperty(() => SelectedStatus)
                                                                          .ObservesProperty(() => DeviceInsuranceYes)
                                                                          .ObservesProperty(() => DeviceInsuranceNo)
                                                                          .ObservesProperty(() => SelectedInsuranceCost)
                                                                          .ObservesProperty(() => SelectedInsuranceValue);
            DeviceMakeCommand = new DelegateCommand(ExecuteShowMakeView, CanExecuteMaintenace);
            DeviceModelCommand = new DelegateCommand(ExecuteShowModelView, CanExecuteMaintenace);
            DeviceStatusCommand = new DelegateCommand(ExecuteShowStatusView, CanExecuteMaintenace);

            // Subscribe to this event to read devices linked to selectede contract
            _eventAggregator.GetEvent<ReadDevicesEvent>().Subscribe(ReadContractDevices_Event, true);

            // Subscribe to this event to save devices when the client save was executed
            _eventAggregator.GetEvent<SaveDeviceEvent>().Subscribe(SaveContractDevices_Event, true);

            // Subscribe to this event to link the first active device to its Simcard
            _eventAggregator.GetEvent<ActionCompletedEvent>().Subscribe(ActionCompleted_Event, true);

            // Subscribe to this event to link the selected Simcard to its device
            _eventAggregator.GetEvent<LinkSimCardDeviceEvent>().Subscribe(LinkSimCardToDevice_Event, true);

            // Initialise the data activity log info entity
            _activityLogInfo = new DataActivityLog();
            _activityLogInfo.ActivityProcess = ActivityProcess.Administration.Value();

            // Load the view data
            await ReadDeviceMakesAsync();
            await ReadStatusesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private async void InitialiseViewControls()
        {
            SelectedDeviceIndex = -1;
            SelectedDevice = null;
            SelectedDeviceMake = DeviceMakeCollection != null ? DeviceMakeCollection.Where(p => p.pkDeviceMakeID == 0).FirstOrDefault() : null;
            SelectedDeviceModel = DeviceModelCollection != null ? DeviceModelCollection.Where(p => p.pkDeviceModelID == 0).FirstOrDefault() : null;
            SelectedSimCard = SimCardCollection != null ? SimCardCollection.Where(p => p.pkSimCardID == 0).FirstOrDefault() : null;
            SelectedStatus = StatusCollection != null ? StatusCollection.Where(p => p.pkStatusID == 0).FirstOrDefault() : null;
            SelectedReceivedDate = DateTime.Now;
            SelectedIMENumber = string.Empty;
            DeviceInsuranceYes = DeviceInsuranceNo = false;
            await ReadContractSimCardsAsync();
        }

        /// <summary>
        /// Load all the device linked to the selected contract from the database
        /// </summary>
        private async void ReadContractDevicesAsync()
        {
            try
            {
                if (MobileManagerEnvironment.ClientContractID > 0)
                {
                     DeviceCollection = await Task.Run(() => new DevicesModel(_eventAggregator).ReadDevicesForContract(MobileManagerEnvironment.ClientContractID));

                    if (DeviceCollection != null && DeviceCollection.Count > 0)
                    {
                        await ReadDeviceMakeModelsAsync();
                        await ReadContractSimCardsAsync();
                    }
                }
                else
                {
                    ExecuteCancel();
                    DeviceCollection = null;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadContractDevicesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set the options for device insurance
        /// </summary>
        /// <param name="hasInsurance">Indicator if a device has insurance.</param>
        private void SetDeviceInsurance(decimal insuranceCost, decimal insuranceValue)
        {
            if (insuranceCost > 0)
            {
                DeviceInsuranceYes = true;
                DeviceInsuranceNo = false;
                SelectedInsuranceCost = insuranceCost;
                SelectedInsuranceValue = insuranceValue;
            }
            else
            {
                DeviceInsuranceYes = false;
                DeviceInsuranceNo = true;
                SelectedInsuranceCost = SelectedInsuranceValue = 0;
            }
        }

        /// <summary>
        /// Link the selected device to its Simcard on the Simcard view
        /// </summary>
        private void LinkDeviceToSimCard()
        {
            // Publish the event so the Simcard view can link the Simcard to the device
            if (SelectedDevice.fkSimCardID != null && SelectedSimCard != null && SelectedDevice.fkSimCardID != SelectedSimCard.pkSimCardID)
                _eventAggregator.GetEvent<LinkDeviceSimCardEvent>().Publish(SelectedDevice.fkSimCardID.Value);
            else
                _eventAggregator.GetEvent<LinkDeviceSimCardEvent>().Publish(0);
        }
        
        /// <summary>
        ///  Set the device selected simcards
        /// </summary>
        private async void SetSelectedDeviceSimCard()
        {
            try
            {
                IEnumerable<DeviceSimCard> simCard = await Task.Run(() => new DeviceSimCardModel(_eventAggregator).ReadDeviceSimCard(SelectedDevice.pkDeviceID));
                DeviceSimCardCollection = new Dictionary<string, object>();
                SelectedDeviceSimCardCollection = new Dictionary<string, object>();

                // Reset the simcards linked to the device
                foreach (SimCard deviceSimCard in SimCardCollection)
                {
                    DeviceSimCardCollection.Add(deviceSimCard.CellNumber, deviceSimCard.pkSimCardID);
                }
                DeviceSimCardCollection = new Dictionary<string, object>(DeviceSimCardCollection);

                // Set the selected device simcards
                foreach (DeviceSimCard deviceSimCard in simCard)
                {
                    SelectedDeviceSimCardCollection.Add(deviceSimCard.SimCard.CellNumber, deviceSimCard.fkSimCardID);
                }

                SelectedDeviceSimCardCollection = new Dictionary<string, object>(SelectedDeviceSimCardCollection);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "SetSelectedDeviceSimCard",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDeviceMakesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the device models from the database
        /// </summary>
        private async Task ReadDeviceMakeModelsAsync()
        {
            try
            {
                if (SelectedDeviceMake != null)
                    DeviceModelCollection = await Task.Run(() => new DevicesModelModel(_eventAggregator).ReadDeviceMakeModels(SelectedDeviceMake.pkDeviceMakeID, true));

                if (DeviceModelCollection != null && SelectedDevice != null)
                    SelectedDeviceModel = DeviceModelCollection.Where(p => p.pkDeviceModelID == SelectedDevice.fkDeviceModelID).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDeviceMakeModelsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the statuses from the database
        /// </summary>
        private async Task ReadStatusesAsync()
        {
            try
            {
                StatusCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Device, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadStatusesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the Sim cards linked to the selected contract from the database
        /// </summary>
        private async Task ReadContractSimCardsAsync()
        {
            try
            {
                if (MobileManagerEnvironment.ClientContractID > 0)
                    SimCardCollection = await Task.Run(() => new SimCardModel(_eventAggregator).ReadSimCardsForContract(MobileManagerEnvironment.ClientContractID));

                if (SimCardCollection != null)
                {
                    if (SimCardCollection.Count > 0)
                    {
                        // Get the primary simcard
                        SimCard primarySimcard = SimCardCollection.Where(p => p.CellNumber == MobileManagerEnvironment.ClientPrimaryCell).FirstOrDefault();

                        if (DeviceCollection != null && DeviceCollection.Count > 0)
                        {
                            // Set the selected device the one linked to the primary simcard
                            if (primarySimcard != null)
                                SelectedDevice = DeviceCollection.Where(p => p.fkSimCardID == primarySimcard.pkSimCardID && p.IsActive).FirstOrDefault();

                            // If device is not linked to a primary simcard
                            // then select the first active device 
                            if (SelectedDevice == null)
                                SelectedDevice = DeviceCollection.Where(p => p.IsActive).FirstOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadContractSimCardsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Validate if the cancel process can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteCancel()
        {
            return SelectedDeviceMake != null ? SelectedDeviceMake.pkDeviceMakeID > 0 : false;
        }

        /// <summary>
        /// Execute when the cancel process
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the cancel process can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteAdd()
        {
            // Validate if the logged-in user can administrate the company the client is linked to
            return MobileManagerEnvironment.ClientCompanyID > 0 && _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;
        }

        /// <summary>
        /// Execute the add new device process
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the save device data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = MobileManagerEnvironment.ClientCompanyID > 0 && _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result && SelectedDeviceMake != null && SelectedDeviceModel != null)
                result = SelectedDeviceMake.pkDeviceMakeID > 0 && SelectedDeviceModel.pkDeviceModelID > 0 && !string.IsNullOrEmpty(SelectedIMENumber) &&
                         SelectedReceivedDate.Date > DateTime.MinValue.Date && (DeviceInsuranceYes || DeviceInsuranceNo) && SelectedStatus.pkStatusID > 0 &&
                         (DeviceInsuranceYes ? SelectedInsuranceCost > 0 && SelectedInsuranceValue > 0 : true);
            else
                result = false;

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private void ExecuteSave()
        {
            bool result = false;

            if (SelectedDevice == null)
                SelectedDevice = new Device();

            SelectedDevice.fkContractID = MobileManagerEnvironment.ClientContractID;
            SelectedDevice.fkDeviceMakeID = SelectedDeviceMake.pkDeviceMakeID;
            SelectedDevice.fkDeviceModelID = SelectedDeviceModel.pkDeviceModelID;
            SelectedDevice.fkStatusID = SelectedStatus.pkStatusID;
            SelectedDevice.fkSimCardID = SelectedSimCard != null && SelectedSimCard.pkSimCardID > 0 ? SelectedSimCard.pkSimCardID : (int?)null;
            SelectedDevice.IMENumber = SelectedIMENumber.ToUpper();
            SelectedDevice.SerialNumber = SelectedDevice.SerialNumber;
            SelectedDevice.ReceiveDate = SelectedReceivedDate;
            SelectedDevice.InsuranceCost = SelectedInsuranceCost;
            SelectedDevice.InsuranceValue = SelectedInsuranceValue;
            SelectedDevice.ModifiedBy = SecurityHelper.LoggedInUserFullName;
            SelectedDevice.ModifiedDate = DateTime.Now;
            SelectedDevice.IsActive = SelectedStatus.StatusDescription == Statuses.ISSUED.ToString() ? true : false;
            
            // Ensure a device that is in-active
            // gets un-linked from any simcards
            if (!SelectedDevice.IsActive)
                SelectedDevice.fkSimCardID = (Int32?)null;
            
            if (SelectedDevice.pkDeviceID == 0)
                result = _model.CreateDevice(SelectedDevice);
            else
                result = _model.UpdateDevice(SelectedDevice);

            if (result)
            {
                if (SelectedDeviceSimCardCollection != null)
                    new DeviceSimCardModel(_eventAggregator).UpdateDeviceSimCards(SelectedDevice, SelectedDeviceSimCardCollection);

                if (_autoSave)
                    _eventAggregator.GetEvent<ActionCompletedEvent>().Publish(ActionCompleted.SaveContractDevices);
                else
                    ReadContractDevicesAsync();
            }

            // Publish the event to read the administration activity logs
            _activityLogInfo.EntityID = MobileManagerEnvironment.ClientContractID;
            _eventAggregator.GetEvent<SetActivityLogProcessEvent>().Publish(_activityLogInfo);
        }

        /// <summary>
        /// Validate if the maintenace functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteMaintenace()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || _securityHelper.IsUserInRole(SecurityRole.DataManager.Value());
        }

        /// <summary>
        /// Execute show device make maintenance view
        /// </summary>
        private async void ExecuteShowMakeView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewDeviceMake(), "Device Make Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadDeviceMakesAsync();
        }

        /// <summary>
        /// Execute show device model maintenance view
        /// </summary>
        private async void ExecuteShowModelView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewDeviceModel(), "Device Model Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadDeviceMakesAsync();
            await ReadDeviceMakeModelsAsync();
        }

        /// <summary>
        /// Execute show status maintenace view
        /// </summary>
        private async void ExecuteShowStatusView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewStatus(), "Status Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadStatusesAsync();
        }

        #endregion

        #endregion
    }
}
