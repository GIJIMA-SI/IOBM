using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.Security;
using Gijima.IOBM.Model.Data;
using Gijima.IOBM.Model.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Xml;

namespace Gijima.IOBM.Shell.ViewModels
{
    public class ViewIOBMShellViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private IEventAggregator _eventAggregator;

        #region Commands

        //public DelegateCommand CancelCommand { get; set; }
        //public DelegateCommand AddCommand { get; set; }
        //public DelegateCommand SaveCommand { get; set; }
        //public DelegateCommand CityCommand { get; set; }
        //public DelegateCommand ProvinceCommand { get; set; }

        #endregion

        #region Properties

        public Action CloseAction { get; set; }

        /// <summary>
        /// The IOBM published version
        /// </summary>
        public string LoggedInUser
        {
            get { return _loggedInUser; }
            set { SetProperty(ref _loggedInUser, value); }
        }
        private string _loggedInUser;

        /// <summary>
        /// The logged-in user's role
        /// </summary>
        public string UserRole
        {
            get { return _userRole; }
            set { SetProperty(ref _userRole, value); }
        }
        private string _userRole;

        /// <summary>
        /// The application database name
        /// </summary>
        public string DataBaseName
        {
            get { return _dataBaseName; }
            set { SetProperty(ref _dataBaseName, value); }
        }
        private string _dataBaseName;

        /// <summary>
        /// The application database server name
        /// </summary>
        public string ServerName
        {
            get { return _serverName; }
            set { SetProperty(ref _serverName, value); }
        }
        private string _serverName;

        /// <summary>
        /// The IOBM published version
        /// </summary>
        public string PublishedIOBMVersion
        {
            get { return _publishedIOBMVersion; }
            set { SetProperty(ref _publishedIOBMVersion, value); }
        }
        private string _publishedIOBMVersion;

        /// <summary>
        /// The IOBM published version
        /// </summary>
        public string PublishedAppVersion
        {
            get { return _publishedAppVersion; }
            set { SetProperty(ref _publishedAppVersion, value); }
        }
        private string _publishedAppVersion;

        /// <summary>
        /// Show or hiode the footer content
        /// </summary>
        public Visibility ShowApplicationFooter
        {
            get { return _showFooterContent; }
            set { SetProperty(ref _showFooterContent, value); }
        }
        private Visibility _showFooterContent;

        #region Required Fields

        #endregion

        #region View Lookup Data Collections
        #endregion

        #region Input Validation

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
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Receive the application info for the selected application
        /// </summary>
        /// <param name="sender">The selected application info.</param>
        private void ApplicationClose_Event(bool sender)
        {
            CloseAction();

        }

        /// <summary>
        /// Receive the application info for the selected application
        /// </summary>
        /// <param name="sender">The selected application info.</param>
        private void ApplicationInfo_Event(object sender)
        {
            ApplicationInfo appInfo = (ApplicationInfo)sender;

            switch (appInfo.ApplicationInfoSource)
            {
                case ApplicationInfo.InfoSource.UserInfo:
                    LoggedInUser = string.Format("Welcome {0}", SecurityHelper.LoggedInFullName);
                    //UserRole = appInfo.;
                    break;
                case ApplicationInfo.InfoSource.ConnectionInfo:
                    ServerName = appInfo.ServerName;
                    DataBaseName = appInfo.DatabaseName;
                    break;
                case ApplicationInfo.InfoSource.ApplicationVersion:
                    PublishedAppVersion = appInfo.PublisedApplicationVersion;
                    break;
            }
                
            ShowApplicationFooter = Visibility.Visible;
        }

        /// <summary>
        /// Receive the application message to display to the user
        /// </summary>
        /// <param name="sender">The application message.</param>
        private void ApplicationMessage_Event(object sender)
        {
            ApplicationMessage appMessage = (ApplicationMessage)sender;

            if (appMessage != null)
            {
                switch (appMessage.MessageType)
                {
                    case ApplicationMessage.MessageTypes.SystemError:
                        MessageBox.Show(appMessage.Message.ToString(), string.Format("{0} - {1}",
                                        appMessage.Owner.ToUpper(),
                                        appMessage.Header.ToUpper()),
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        break;
                    case ApplicationMessage.MessageTypes.ProcessError:
                        MessageBox.Show(appMessage.Message.ToString(), string.Format("{0} - {1}",
                                        appMessage.Owner.ToUpper(),
                                        appMessage.Header.ToUpper()),
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Exclamation);
                        break;
                    default:
                        MessageBox.Show(appMessage.Message.ToString(), string.Format("{0} - {1}",
                                        appMessage.Owner.ToUpper(),
                                        appMessage.Header.ToUpper()),
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                        break;
                }
            }
        }

        /// <summary>
        /// Sync the solution user with the application user
        /// </summary>
        /// <param name="sender">The application user enetity.</param>
        private void MobileManagerSecurity_Event(object sender)
        {
            new IOBMSecurityModel(_eventAggregator).SyncSolutionUser((User)sender);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewIOBMShellViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;

            // Subscribe to this event to close the application
            _eventAggregator.GetEvent<ApplicationCloseEvent>().Subscribe(ApplicationClose_Event, true);

            // Subscribe to this event to get the application info
            _eventAggregator.GetEvent<ApplicationInfoEvent>().Subscribe(ApplicationInfo_Event, true);

            // Subscribe to this event to display processing and exception messages to the user
            _eventAggregator.GetEvent<ApplicationMessageEvent>().Subscribe(ApplicationMessage_Event, true);

            // Subscribe to this event so the solution user and the mobile manager user can be synced
            _eventAggregator.GetEvent<MobileManagerSecurityEvent>().Subscribe(MobileManagerSecurity_Event, true);

            InitialiseShellView();
        }

        /// <summary>
        /// Validate the shell environment
        /// </summary>
        private void InitialiseShellView()
        {
            ShowApplicationFooter = Visibility.Collapsed;

            // Close the applicaion if the user do no authenticate
            if (!AuthenticateUser())
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewIOBMShellViewModel",
                                         string.Format("Authentication failed for user {0}.", WindowsIdentity.GetCurrent().Name),
                                                "InitialiseShellView",
                                                ApplicationMessage.MessageTypes.Information));

                // Raise the event to close the application
                _eventAggregator.GetEvent<ApplicationCloseEvent>().Publish(true);
            }

            GetPublishedSoltionVersion();
        }

        /// <summary>
        /// Authenticate the user against the IOBM database users
        /// </summary>
        /// <returns></returns>
        private bool AuthenticateUser()
        {
            try
            {
                return new SecurityHelper(_eventAggregator).IsUserAuthenticated(WindowsIdentity.GetCurrent().Name);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewIOBMShellViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "AuthenticateUser",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Get the published solution version
        /// </summary>
        /// <returns>The Version number</returns>
        private void GetPublishedSoltionVersion()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                Assembly asmCurrent = System.Reflection.Assembly.GetEntryAssembly();
                string executePath = new Uri(asmCurrent.GetName().CodeBase).LocalPath;

                xmlDoc.Load(executePath + ".manifest");
                string retval = string.Empty;

                if (xmlDoc.HasChildNodes)
                    retval = xmlDoc.ChildNodes[1].ChildNodes[0].Attributes.GetNamedItem("version").Value.ToString();

                PublishedIOBMVersion = retval;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewIOBMShellViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "GetPublishedSoltionVersion",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Command Execution

        #endregion

        #endregion
    }
}
