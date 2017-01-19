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
    public class ViewSimmCardViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private SimmCardModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private int _selectedContractID = 0;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SimmCardStatusCommand { get; set; }
        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) simm card entity
        /// </summary>
        public SimmCard SelectedSimmCard
        {
            get { return _selectedSimmCard; }
            set
            {
                // Prevent circullar action
                if (value != null && value.pkSimmCardID != _selectedSimmCard.pkSimmCardID)
                {
                    SelectedStatus = StatusCollection != null ? StatusCollection.Where(p => p.pkStatusID == value.fkStatusID).FirstOrDefault() : null;
                    SelectedCellNumber = value.CellNumber;
                    SelectedCardNumber = value.CardNumber;
                    SelectedPinNumber = value.PinNumber;
                    SelectedPUKNumber = value.PUKNumber;
                    SimmCardState = value.IsActive;

                    SetProperty(ref _selectedSimmCard, value);

                    // Link the simmcard to its device on the device view
                    LinkSimmCardToDevice();
                }
            }
        }
        private SimmCard _selectedSimmCard = new SimmCard();

        /// <summary>
        /// The selected simm card state
        /// </summary>
        public bool SimmCardState
        {
            get { return _simmCardState; }
            set { SetProperty(ref _simmCardState, value); }
        }
        private bool _simmCardState;

        /// <summary>
        /// The collection of simm cards from the database
        /// </summary>
        public ObservableCollection<SimmCard> SimmCardCollection
        {
            get { return _simmCardCollection; }
            set { SetProperty(ref _simmCardCollection, value); }
        }
        private ObservableCollection<SimmCard> _simmCardCollection = null;

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

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewSimmCardViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseSimmCardView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseSimmCardView()
        {
            _model = new SimmCardModel(_eventAggregator);
            _securityHelper = new SecurityHelper(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedCellNumber);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedSimmCard);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedCellNumber)
                                                                          .ObservesProperty(() => SelectedCardNumber)
                                                                          .ObservesProperty(() => SelectedPinNumber)
                                                                          .ObservesProperty(() => SelectedPUKNumber)
                                                                          .ObservesProperty(() => SelectedStatus);
            SimmCardStatusCommand = new DelegateCommand(ExecuteShowStatusView, CanExecuteMaintenace);

            // Subscribe to this event to read simmCards linked to selected contract
            _eventAggregator.GetEvent<ReadSimmCardsEvent>().Subscribe(ReadContractSimmCardsAsync, true);
            
            // Subscribe to this event to link the selected device to a simmcard
            _eventAggregator.GetEvent<LinkDeviceSimmCardEvent>().Subscribe(LinkDeviceToSimmCard, true);

            // Load the view data
            await ReadStatusesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedSimmCard = new SimmCard();
            SimmCardState = true;
        }

        /// <summary>
        /// Load all the simm cards linked to the selected contract from the database
        /// </summary>
        /// <param name="contractID">The selected contract ID.</param>
        private async void ReadContractSimmCardsAsync(int contractID)
        {
            try
            {
                _selectedContractID = contractID;

                if (contractID > 0)
                {
                    SimmCardCollection = await Task.Run(() => new SimmCardModel(_eventAggregator).ReadSimmCardsForContract(contractID));

                    // Publish this event to tell the Device view to link the device to its simmcard
                    if (SimmCardCollection != null && SimmCardCollection.Count > 0)
                        _eventAggregator.GetEvent<ActionCompletedEvent>().Publish(ActionCompleted.ReadContractSimmCards);
                }
                else
                {
                    ExecuteCancel();
                    SimmCardCollection = null;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Link the selected device to a simmcard
        /// </summary>
        /// <param name="simmCardID">The seleted simm card.</param>
        private void LinkDeviceToSimmCard(int simmCardID)
        {
            try
            {
                if (SimmCardCollection != null && SimmCardCollection.Count > 0)
                {
                    if (simmCardID > 0)
                        SelectedSimmCard = SimmCardCollection.Where(p => p.pkSimmCardID == simmCardID && p.IsActive).FirstOrDefault();
                    else
                        ExecuteCancel();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Link the selected simm card to its device on the device view
        /// </summary>
        private void LinkSimmCardToDevice()
        {
            // Publish the event so the simmcard view can link the simmcard to the device
            if (SelectedSimmCard.CellNumber != null && !string.IsNullOrEmpty(SelectedSimmCard.CellNumber))
                _eventAggregator.GetEvent<LinkSimmCardDeviceEvent>().Publish(SelectedSimmCard.pkSimmCardID);
            else
                _eventAggregator.GetEvent<LinkSimmCardDeviceEvent>().Publish(0);
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the statuses from the database
        /// </summary>
        private async Task ReadStatusesAsync()
        {
            try
            {
                StatusCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Simm, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = SelectedSimmCard != null && SelectedSimmCard.pkSimmCardID > 0 && 
                     (MobileManagerEnvironment.ClientCompanyID == 0 || _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false);

            if (result)
                result = !string.IsNullOrEmpty(SelectedCellNumber);

            return result;
        }

        /// <summary>
        /// Execute the add new simmCard process
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the save simmCard data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = MobileManagerEnvironment.ClientCompanyID == 0 || _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result)
                result = !string.IsNullOrEmpty(SelectedCellNumber) && !string.IsNullOrEmpty(SelectedCardNumber) && !string.IsNullOrEmpty(SelectedPinNumber) &&
                         !string.IsNullOrEmpty(SelectedPUKNumber) && SelectedStatus != null && SelectedStatus.pkStatusID > 0;

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private void ExecuteSave()
        {
            int selectedSimmCardID = 0;
            bool result = false;
            SelectedSimmCard.fkContractID = selectedSimmCardID = _selectedContractID;
            SelectedSimmCard.fkStatusID = SelectedStatus.pkStatusID;
            SelectedSimmCard.CellNumber = SelectedCellNumber.Trim();
            SelectedSimmCard.CardNumber = SelectedCardNumber.ToUpper().Trim();
            SelectedSimmCard.PinNumber = SelectedPinNumber.ToUpper().Trim();
            SelectedSimmCard.PUKNumber = SelectedPUKNumber.ToUpper().Trim();
            SelectedSimmCard.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedSimmCard.ModifiedDate = DateTime.Now;
            SelectedSimmCard.IsActive = SimmCardState;

            if (SelectedSimmCard.pkSimmCardID == 0)
                result = _model.CreateSimmCard(SelectedSimmCard);
            else
                result = _model.UpdateSimmCard(SelectedSimmCard);

            if (result)
                ReadContractSimmCardsAsync(_selectedContractID);
        }

        /// <summary>
        /// Validate if the maintenace functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteMaintenace()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || _securityHelper.IsUserInRole(SecurityRole.Supervisor.Value());
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
