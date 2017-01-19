using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewAuditLogViewModel : BindableBase
    {
        #region Properties & Attributes

        private AuditLogModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the entity ID linked to the action
        /// </summary>
        public int EntityID
        {
            get { return _entityID; }
            set { SetProperty(ref _entityID, value); }
        }
        private int _entityID = 0;

        /// <summary>
        /// Holds the selected activity log
        /// </summary>
        public AuditLog SelectedAuditLog
        {
            get { return _selectedAuditLog; }
            set
            {
                ShowComment = value == null ? Visibility.Collapsed : Visibility.Visible;
                SelectedComment = value != null ? value.AuditComment : string.Empty;
                SetProperty(ref _selectedAuditLog, value);
            }
        }
        private AuditLog _selectedAuditLog = null;

        /// <summary>
        /// Holds the selected activity log filter
        /// </summary>
        public string SelectedActivityLogFilter
        {
            get { return _selectedActivityLogFilter; }
            set
            {
                SetProperty(ref _selectedActivityLogFilter, value);
                if (EntityID > 0)
                    Task.Run(() => ReadAuditLogsAsync(EntityID));
            }
        }
        private string _selectedActivityLogFilter = null;

        /// <summary>
        /// Holds the entered activity comment
        /// </summary>
        public string SelectedComment
        {
            get { return _activityComment; }
            set { SetProperty(ref _activityComment, value); }
        }
        private string _activityComment = string.Empty;

        /// <summary>
        /// Holds the value to hide or show comment
        /// </summary>
        public Visibility ShowComment
        {
            get { return _showComment; }
            set { SetProperty(ref _showComment, value); }
        }
        private Visibility _showComment = Visibility.Collapsed;

        /// <summary>
        /// The collection of activity log results
        /// </summary>
        public ObservableCollection<AuditLog> AuditLogCollection
        {
            get { return _activityLogCollection; }
            set { SetProperty(ref _activityLogCollection, value); }
        }
        private ObservableCollection<AuditLog> _activityLogCollection = null;

        #region Required Fields

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of activity log filters
        /// </summary>
        public ObservableCollection<string> ActivityLogFilterCollection
        {
            get { return _activityFilterCollection; }
            set { SetProperty(ref _activityFilterCollection, value); }
        }
        private ObservableCollection<string> _activityFilterCollection = null;

        #endregion

        #region Input Validation

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Load all the activity logs based on the specified process from the database
        /// </summary>
        /// <param name="sender">The selected contractID.</param>
        private void SetActivityLogProcess_Event(object sender)
        {
            ReadActivityLogFilters((DataActivityLog)sender);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewAuditLogViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseAuditLogView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseAuditLogView()
        {
            _model = new AuditLogModel(_eventAggregator);
            _securityHelper = new SecurityHelper(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedAuditLog);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => EntityID);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedAuditLog);

            // Subscribe to this event to read to activity logs from the database for the specified process
            _eventAggregator.GetEvent<SetActivityLogProcessEvent>().Subscribe(SetActivityLogProcess_Event, true);

            // Load all the activity logs for the client
            DataActivityLog activityLogInfo = new DataActivityLog();
            activityLogInfo.ActivityProcess = ActivityProcess.Administration.Value();
            activityLogInfo.EntityID = MobileManagerEnvironment.ClientContractID;
            ReadActivityLogFilters(activityLogInfo);
            SelectedActivityLogFilter = AdminActivityFilter.None.ToString();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedAuditLog = null;
            SelectedActivityLogFilter = AdminActivityFilter.None.ToString();
            SelectedComment = string.Empty;
        }

        /// <summary>
        /// Load all the activity logs based on the selected filter from the database
        /// </summary>
        /// <param name="entityID">The entity linked to the activity log.</param>
        private async Task ReadAuditLogsAsync(int entityID)
        {
            try
            {
                AuditLogCollection = await Task.Run(() => new AuditLogModel(_eventAggregator).ReadAuditLogs(SelectedActivityLogFilter, entityID));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Populate the filter combobox from the admin activity filter enum
        /// </summary>
        /// <param name="dataActivityLog">The data activity log info.</param> 
        private async void ReadActivityLogFilters(DataActivityLog dataActivityLog)
        {
            ActivityLogFilterCollection = new ObservableCollection<string>();
            EntityID = dataActivityLog.EntityID;

            switch ((ActivityProcess)dataActivityLog.ActivityProcess)
            {
                case ActivityProcess.Maintenance:
                    foreach (MaintActivityFilter source in Enum.GetValues(typeof(MaintActivityFilter)))
                    {
                        ActivityLogFilterCollection.Add(source.ToString());
                    }
                    break;
                case ActivityProcess.Configuartion:
                    foreach (ConfigActivityFilter source in Enum.GetValues(typeof(ConfigActivityFilter)))
                    {
                        ActivityLogFilterCollection.Add(source.ToString());
                    }
                    break;
                default:
                    foreach (AdminActivityFilter source in Enum.GetValues(typeof(AdminActivityFilter)))
                    {
                        ActivityLogFilterCollection.Add(source.ToString());
                    }
                    break;
            }

            if (ActivityLogFilterCollection.Count > 0)
            {
                SelectedActivityLogFilter = AdminActivityFilter.None.ToString();
                await ReadAuditLogsAsync(_entityID);
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
            return SelectedAuditLog != null;
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the add functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteAdd()
        {
            return EntityID > 0;
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            SelectedActivityLogFilter = AdminActivityFilter.Manual.ToString();
            ShowComment = Visibility.Visible;
            SelectedComment = string.Empty;
            SelectedAuditLog = new AuditLog();
            SelectedAuditLog.AuditGroup = AdminActivityFilter.Manual.ToString().ToUpper();
            SelectedAuditLog.AuditDescription = string.Format("Manual activity created by {0} on {1}.", SecurityHelper.LoggedInUserFullName, DateTime.Now.ToString());
            SelectedAuditLog.EntityID = EntityID;
            SelectedAuditLog.ChangedValue = string.Empty;
            SelectedAuditLog.AuditDate = DateTime.Now;
        }

        /// <summary>
        /// Validate if the save functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = MobileManagerEnvironment.ClientCompanyID == 0 || _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result)
                result = SelectedAuditLog != null;

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            try
            {
                bool result = false;
                SelectedAuditLog.AuditComment = SelectedComment;
                SelectedAuditLog.ModifiedBy = SecurityHelper.LoggedInUserFullName;
                SelectedAuditLog.ModifiedDate = DateTime.Now;

                if(SelectedAuditLog.pkAuditLogID == 0)
                    result = await Task.Run(() => _model.CreateAuditLog(SelectedAuditLog));
                else
                    result = await Task.Run(() => _model.UpdateAuditLog(SelectedAuditLog));

                if (result)
                {
                    InitialiseViewControls();
                    await ReadAuditLogsAsync(_entityID);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #endregion

        #endregion
    }
}
