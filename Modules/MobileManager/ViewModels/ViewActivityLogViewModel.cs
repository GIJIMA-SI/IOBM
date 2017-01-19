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
    public class ViewActivityLogViewModel : BindableBase
    {
        #region Properties & Attributes

        private ActivityLogModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected activity log
        /// </summary>
        public ActivityLog SelectedActivityLog
        {
            get { return _selectedActivityLog; }
            set
            {
                ShowComment = value == null ? Visibility.Collapsed : Visibility.Visible;
                SelectedComment = value != null ? value.ActivityComment : string.Empty;
                SetProperty(ref _selectedActivityLog, value);
            }
        }
        private ActivityLog _selectedActivityLog = null;

        /// <summary>
        /// Holds the selected activity log filter
        /// </summary>
        public string SelectedActivityLogFilter
        {
            get { return _selectedActivityLogFilter; }
            set
            {
                SetProperty(ref _selectedActivityLogFilter, value);
                Task.Run(() => ReadActivityLogsAsync());
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
        public ObservableCollection<ActivityLog> ActivityLogCollection
        {
            get { return _activityLogCollection; }
            set { SetProperty(ref _activityLogCollection, value); }
        }
        private ObservableCollection<ActivityLog> _activityLogCollection = null;

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
            ReadActivityLogFilters(sender);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewActivityLogViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseActivityLogView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseActivityLogView()
        {
            _model = new ActivityLogModel(_eventAggregator);
            _securityHelper = new SecurityHelper(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedActivityLog);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedActivityLog);

            // Subscribe to this event to read to activity logs from the database for the specified process
            _eventAggregator.GetEvent<SetActivityLogProcessEvent>().Subscribe(SetActivityLogProcess_Event, true);

            ReadActivityLogFilters(ActivityProcess.Administration);
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedActivityLog = null;
            SelectedActivityLogFilter = AdminActivityFilter.None.ToString();
            SelectedComment = string.Empty;
        }

        /// <summary>
        /// Load all the activity logs based on the selected filter from the database
        /// </summary>
        private async Task ReadActivityLogsAsync()
        {
            try
            {
                ActivityLogCollection = await Task.Run(() => new ActivityLogModel(_eventAggregator).ReadActivityLogs(SelectedActivityLogFilter));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Populate the filter combobox from the admin activity filter enum
        /// </summary>
        /// <param name="activityProcess">The activity process enum.</param> 
        private async void ReadActivityLogFilters(object activityProcess)
        {
            ActivityLogFilterCollection = new ObservableCollection<string>();

            switch ((ActivityProcess)activityProcess)
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
                await ReadActivityLogsAsync();
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Validate if the save functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteCancel()
        {
            return SelectedActivityLog != null;
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
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = MobileManagerEnvironment.ClientCompanyID == 0 || _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result)
                result = SelectedActivityLog != null;

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
                SelectedActivityLog.ActivityComment = SelectedComment;
                SelectedActivityLog.ModifiedBy = SecurityHelper.LoggedInDomainName;
                SelectedActivityLog.ModifiedDate = DateTime.Now;

                result = await Task.Run(() => _model.UpdateActivityLog(SelectedActivityLog));

                if (result)
                {
                    InitialiseViewControls();
                    await ReadActivityLogsAsync();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        #endregion

        #endregion
    }
}
