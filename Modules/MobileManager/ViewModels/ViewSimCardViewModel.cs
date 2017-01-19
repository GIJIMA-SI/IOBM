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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewSimCardViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private SimCardModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private DataActivityLog _activityLogInfo = null;
        private bool _autoSave = false;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SimCardStatusCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) Sim card entity
        /// </summary>
        public SimCard SelectedSimCard
        {
            get { return _selectedSimCard; }
            set
            {
                SetProperty(ref _selectedSimCard, value);

                // Prevent circullar action
                if (value != null && value.pkSimCardID > 0)
                {
                    SelectedStatus = StatusCollection != null ? StatusCollection.Where(p => p.pkStatusID == value.fkStatusID).FirstOrDefault() : null;
                    SelectedCellNumber = value.CellNumber;
                    SelectedCardNumber = value.CardNumber;
                    SelectedPinNumber = value.PinNumber;
                    SelectedPUKNumber = value.PUKNumber;
                    SimCardState = value.IsActive;

                    // Link the Simcard to its device on the device view
                    LinkSimCardToDevice();
                }
            }
        }
        private SimCard _selectedSimCard = new SimCard();
        
        /// <summary>
        /// The selected Sim card index
        /// </summary>
        public int SelectedSimCardIndex
        {
            get { return _selectedSimCardIndex; }
            set { SetProperty(ref _selectedSimCardIndex, value); }
        }
        private int _selectedSimCardIndex;

        /// <summary>
        /// The selected Sim card state
        /// </summary>
        public bool SimCardState
        {
            get { return _simCardState; }
            set { SetProperty(ref _simCardState, value); }
        }
        private bool _simCardState;

        /// <summary>
        /// The collection of Sim cards from the database
        /// </summary>
        public ObservableCollection<SimCard> SimCardCollection
        {
            get { return _simCardCollection; }
            set { SetProperty(ref _simCardCollection, value); }
        }
        private ObservableCollection<SimCard> _simCardCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of statuses from the database
        /// </summary>
        public ObservableCollection<Status> StatusCollection
        {
            get { return _statusCollection; }
            set { SetProperty(ref _statusCollection, value); }
        }
        private ObservableCollection<Status> _statusCollection = null;

        #endregion

        #region Required Fields

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
        /// The entered cell number
        /// </summary>
        public string SelectedCellNumber
        {
            get { return _selectedCellNumber; }
            set { SetProperty(ref _selectedCellNumber, value); }
        }
        private string _selectedCellNumber;

        /// <summary>
        /// The entered card number
        /// </summary>
        public string SelectedCardNumber
        {
            get { return _selectedCardNumber; }
            set { SetProperty(ref _selectedCardNumber, value); }
        }
        private string _selectedCardNumber;

        /// <summary>
        /// The entered pin number
        /// </summary>
        public string SelectedPinNumber
        {
            get { return _selectedPinNumber; }
            set { SetProperty(ref _selectedPinNumber, value); }
        }
        private string _selectedPinNumber;

        /// <summary>
        /// The entered PUK number
        /// </summary>
        public string SelectedPUKNumber
        {
            get { return _selectedPUKNumber; }
            set { SetProperty(ref _selectedPUKNumber, value); }
        }
        private string _selectedPUKNumber;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCellNumber
        {
            get { return _validCellNumber; }
            set { SetProperty(ref _validCellNumber, value); }
        }
        private Brush _validCellNumber = Brushes.Red;

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
        public Brush ValidCardNumber
        {
            get { return _validCardNumber; }
            set { SetProperty(ref _validCardNumber, value); }
        }
        private Brush _validCardNumber = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidPinNumber
        {
            get { return _validPinNumber; }
            set { SetProperty(ref _validPinNumber, value); }
        }
        private Brush _validPinNumber = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidPUKNumber
        {
            get { return _validPUKNumber; }
            set { SetProperty(ref _validPUKNumber, value); }
        }
        private Brush _validPUKNumber = Brushes.Red;

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
                    case "SelectedCellNumber":
                        ValidCellNumber = string.IsNullOrEmpty(SelectedCellNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCardNumber":
                        ValidCardNumber = string.IsNullOrEmpty(SelectedCardNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedPinNumber":
                        ValidPinNumber = string.IsNullOrEmpty(SelectedPinNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedPUKNumber":
                        ValidPUKNumber = string.IsNullOrEmpty(SelectedPUKNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedStatus":
                        ValidStatus = SelectedStatus == null || SelectedStatus.pkStatusID < 1 ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Load all the Sim cards linked to the selected contract from the database
        /// </summary>
        /// <param name="contractID">The selected contract ID.</param>
        private async void ReadContractSimCards_Event(int contractID)
        {
            try
            {
                await ReadContractSimCardsAsync();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Save the simcard data
        /// </summary>
        /// <param name="sender">The selected client contract ID.</param>
        private void SaveContractSimCards_Event(int sender)
        {
            if (CanExecuteSave())
            {
                _autoSave = true;
                ExecuteSave();
                _autoSave = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewSimCardViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseSimCardView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseSimCardView()
        {
            _model = new SimCardModel(_eventAggregator);
            _securityHelper = new SecurityHelper(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedCellNumber);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedSimCard);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedCellNumber)
                                                                          .ObservesProperty(() => SelectedCardNumber)
                                                                          .ObservesProperty(() => SelectedPinNumber)
                                                                          .ObservesProperty(() => SelectedPUKNumber)
                                                                          .ObservesProperty(() => SelectedStatus);
            SimCardStatusCommand = new DelegateCommand(ExecuteShowStatusView, CanExecuteMaintenace);

            // Subscribe to this event to read SimCards linked to selected contract
            _eventAggregator.GetEvent<ReadSimCardsEvent>().Subscribe(ReadContractSimCards_Event, true);

            // Subscribe to this event to save simcard data when the client save was executed
            _eventAggregator.GetEvent<SaveSimCardEvent>().Subscribe(SaveContractSimCards_Event, true);

            // Subscribe to this event to link the selected device to a Simcard
            _eventAggregator.GetEvent<LinkDeviceSimCardEvent>().Subscribe(LinkDeviceToSimCard, true);

            // Initialise the data activity log info entity
            _activityLogInfo = new DataActivityLog();
            _activityLogInfo.ActivityProcess = ActivityProcess.Administration.Value();

            // Load the view data
            await ReadStatusesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedSimCard = null;
            SelectedSimCardIndex = -1;
            SelectedStatus = StatusCollection != null ? StatusCollection.Where(p => p.pkStatusID == 0).FirstOrDefault() : null;
            SelectedCellNumber = SelectedCardNumber = SelectedPinNumber = SelectedPUKNumber = string.Empty;
            SimCardState = true; 
        }

        /// <summary>
        /// Load all the Sim cards linked to the selected contract from the database
        /// </summary>
        private async Task ReadContractSimCardsAsync()
        {
            try
            {
                if (MobileManagerEnvironment.ClientContractID > 0)
                {
                    SimCardCollection = await Task.Run(() => new SimCardModel(_eventAggregator).ReadSimCardsForContract(MobileManagerEnvironment.ClientContractID));

                    if (SimCardCollection != null && SimCardCollection.Count > 0)
                    {
                        // Set the selected sim card to the primary simcard
                        SelectedSimCard = SimCardCollection.Where(p => p.CellNumber == MobileManagerEnvironment.ClientPrimaryCell && p.IsActive).FirstOrDefault();

                        // If no primary simcard found
                        // then select the first active simcard 
                        if (SelectedSimCard == null)
                            SelectedSimCard = SimCardCollection.Where(p => p.IsActive).FirstOrDefault();
                    }
                }
                else
                {
                    ExecuteCancel();
                    SimCardCollection = null;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Link the selected device to a Simcard
        /// </summary>
        /// <param name="simCardID">The seleted Sim card.</param>
        private void LinkDeviceToSimCard(int simCardID)
        {
            try
            {
                if (SimCardCollection != null && SimCardCollection.Count > 0)
                {
                    if (simCardID > 0)
                        SelectedSimCard = SimCardCollection.Where(p => p.pkSimCardID == simCardID && p.IsActive).FirstOrDefault();
                    else
                        ExecuteCancel();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Link the selected Sim card to its device on the device view
        /// </summary>
        private void LinkPrimarySimCardToDevice()
        {

            // Publish the event so the Simcard view can link the Simcard to the device
            if (SelectedSimCard.CellNumber != null && !string.IsNullOrEmpty(SelectedSimCard.CellNumber))
                _eventAggregator.GetEvent<LinkSimCardDeviceEvent>().Publish(SelectedSimCard.pkSimCardID);
            else
                _eventAggregator.GetEvent<LinkSimCardDeviceEvent>().Publish(0);
        }

        /// <summary>
        /// Link the selected Sim card to its device on the device view
        /// </summary>
        private void LinkSimCardToDevice()
        {
            // Publish the event so the Simcard view can link the Simcard to the device
            if (SelectedSimCard.CellNumber != null && !string.IsNullOrEmpty(SelectedSimCard.CellNumber))
                _eventAggregator.GetEvent<LinkSimCardDeviceEvent>().Publish(SelectedSimCard.pkSimCardID);
            else
                _eventAggregator.GetEvent<LinkSimCardDeviceEvent>().Publish(0);
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the statuses from the database
        /// </summary>
        private async Task ReadStatusesAsync()
        {
            try
            {
                StatusCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Sim, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
            return !string.IsNullOrEmpty(SelectedCellNumber);
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
        /// Execute the add new SimCard process
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the save SimCard data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = MobileManagerEnvironment.ClientCompanyID > 0 && _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result)
                result = !string.IsNullOrEmpty(SelectedCellNumber) && !string.IsNullOrEmpty(SelectedCardNumber) && !string.IsNullOrEmpty(SelectedPinNumber) &&
                         !string.IsNullOrEmpty(SelectedPUKNumber) && SelectedStatus != null && SelectedStatus.pkStatusID > 0;

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;
            int selectedSimCardID = 0;

            if (SelectedSimCard == null)
                SelectedSimCard = new SimCard();

            SelectedSimCard.fkContractID = selectedSimCardID = MobileManagerEnvironment.ClientContractID;
            SelectedSimCard.fkStatusID = SelectedStatus.pkStatusID;
            SelectedSimCard.CellNumber = SelectedCellNumber.Trim();
            SelectedSimCard.CardNumber = SelectedCardNumber.ToUpper().Trim();
            SelectedSimCard.PinNumber = SelectedPinNumber.ToUpper().Trim();
            SelectedSimCard.PUKNumber = SelectedPUKNumber.ToUpper().Trim();
            SelectedSimCard.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedSimCard.ModifiedDate = DateTime.Now;
            SelectedSimCard.IsActive = SimCardState;

            // Ensure a sim card that gets re-allocated is in-active
            if (SelectedStatus.StatusDescription == "REALLOCATED")
                SelectedSimCard.IsActive = false;

            if (SelectedSimCard.pkSimCardID == 0)
                result = _model.CreateSimCard(SelectedSimCard);
            else
                result = _model.UpdateSimCard(SelectedSimCard);

            if (result)
            {
                if (_autoSave)
                    _eventAggregator.GetEvent<ActionCompletedEvent>().Publish(ActionCompleted.SaveContractSimCards);
                else
                    await ReadContractSimCardsAsync();
            }

            // Publish the event to read the administartion activity logs
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
