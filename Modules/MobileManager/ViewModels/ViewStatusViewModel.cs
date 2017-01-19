using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
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
    public class ViewStatusViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private StatusModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) status entity
        /// </summary>
        public Status SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                if (value != null)
                {
                    SelectedStatusDescription = value.StatusDescription;
                    SelectedStatusLink = (StatusLink)value.enStatusLink;
                    StatusState = value.IsActive;
                    SetProperty(ref _selectedStatus, value);
                }
            }
        }
        private Status _selectedStatus = new Status();

        /// <summary>
        /// The selected status link
        /// </summary>
        public StatusLink SelectedStatusLink
        {
            get { return _selectedStatusLink; }
            set { SetProperty(ref _selectedStatusLink, value); }
        }
        private StatusLink _selectedStatusLink;

        /// <summary>
        /// The selected status state
        /// </summary>
        public bool StatusState
        {
            get { return _statusState; }
            set { SetProperty(ref _statusState, value); }
        }
        private bool _statusState;

        /// <summary>
        /// The collection of statuses from the database
        /// </summary>
        public ObservableCollection<Status> StatusCollection
        {
            get { return _statusCollection; }
            set { SetProperty(ref _statusCollection, value); }
        }
        private ObservableCollection<Status> _statusCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of status links from the StatusLink enum
        /// </summary>
        public ObservableCollection<StatusLink> StatusLinkCollection
        {
            get { return _statusLinkCollection; }
            set { SetProperty(ref _statusLinkCollection, value); }
        }
        private ObservableCollection<StatusLink> _statusLinkCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered status description
        /// </summary>
        public string SelectedStatusDescription
        {
            get { return _selectedStatusDescription; }
            set { SetProperty(ref _selectedStatusDescription, value); }
        }
        private string _selectedStatusDescription = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidStatusDescription
        {
            get { return _validStatusDescription; }
            set { SetProperty(ref _validStatusDescription, value); }
        }
        private Brush _validStatusDescription = Brushes.Red;

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
                    case "SelectedStatusDescription":
                        ValidStatusDescription = string.IsNullOrEmpty(SelectedStatusDescription) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewStatusViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseStatusView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseStatusView()
        {
            _model = new StatusModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => SelectedStatusDescription);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => SelectedStatusDescription);

            // Load the view data
            await ReadStatusesAsync();
            ReadStatusLinks();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedStatus = new Status();
        }

        /// <summary>
        /// Load all the statuses from the database
        /// </summary>
        private async Task ReadStatusesAsync()
        {
            try
            {
                StatusCollection = await Task.Run(() => _model.ReadStatuses(StatusLink.All, false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Populate the status link combobox from the StatusLink enum
        /// </summary>
        private void ReadStatusLinks()
        {
            StatusLinkCollection = new ObservableCollection<StatusLink>();

            foreach (StatusLink source in Enum.GetValues(typeof(StatusLink)))
                StatusLinkCollection.Add(source);
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecute()
        {
            return !string.IsNullOrWhiteSpace(SelectedStatusDescription); 
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
            SelectedStatus.StatusDescription = SelectedStatusDescription.ToUpper();
            SelectedStatus.enStatusLink = SelectedStatusLink.Value();
            SelectedStatus.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedStatus.ModifiedDate = DateTime.Now;
            SelectedStatus.IsActive = StatusState;

            if (SelectedStatus.pkStatusID == 0)
                result = _model.CreateStatus(SelectedStatus);
            else
                result = _model.UpdateStatus(SelectedStatus);

            if (result)
            {
                InitialiseViewControls();
                await ReadStatusesAsync();
            }
        }

        #endregion

        #endregion
    }
}
