using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Views;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewMobileManagerViewModel : BindableBase
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper;
        private int _selectedTabIndex = 0;

        #region Properties

        /// <summary>
        /// The current billing period
        /// </summary>
        public string BillingPeriod
        {
            get { return _billingPeriod; }
            set { SetProperty(ref _billingPeriod, value); }
        }
        private string _billingPeriod = null;

        /// <summary>
        /// Indicate if the billing period is open or closed
        /// </summary>
        public string BillingPeriodOpen
        {
            get { return _billingPeriodOpen; }
            set { SetProperty(ref _billingPeriodOpen, value); }
        }
        private string _billingPeriodOpen;

        /// <summary>
        /// The selected tab item
        /// </summary>
        public TabItem SelectedTab
        {
            get { return _selectedTab; }
            set { SetProperty(ref _selectedTab, value); SetTabContent(value); }
        }
        private TabItem _selectedTab = null;

        /// <summary>
        /// The collection of tab pages for the tab control menu
        /// </summary>
        public ObservableCollection<TabItem> TabCollection
        {
            get { return _tabCollection; }
            set { SetProperty(ref _tabCollection, value); }
        }
        private ObservableCollection<TabItem> _tabCollection = null;

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event navigate to the specified tab
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void Navigation_Event(int sender)
        {
            SelectedTab = TabCollection[sender];
        }

        /// <summary>
        /// This event update the billing period on the UI
        /// </summary>
        /// <param name="sender">null</param>
        private void BillingPeriod_Event(object sender)
        {
            BillingPeriod = MobileManagerEnvironment.BillingPeriod;
            BillingPeriodOpen = MobileManagerEnvironment.IsBillingPeriodOpen ? "(Open)" : "(Closed)";
        }

        ///// <summary>
        ///// Invoked when the main window is loaded.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // Moved the initialisation to the 'Loaded' event, so that the main window pops up first.
        //    // Delay a little here to allow the main window to show
        //    await System.Threading.Tasks.Task.Delay(100);

        //    try
        //    {
        //        BusyIndicatorMainWindow.BusyContent = "Loading configuration...";
        //        BusyIndicatorMainWindow.IsBusy = true;

        //        // Ensure properties are loaded
        //        // We had to move this from app.xaml to here so that we have a parent window for message boxes
        //        // If not, Mircosoft assumes that the first window created is the main window and refuses to load the right one
        //        LoadSettings();

        //        // Ensure logging is initialised
        //        LogHelper.Initialise();

        //        BusyIndicatorMainWindow.BusyContent = "Authenticating...";

        //        if (await AuthenticateUserAsync())
        //        {
        //            InitialiseControls();
        //            SetEnvironment();

        //            // Get the current published version
        //            TextBlockVersion.Text = TextBlockVersion.Text + ((Version)BrsMsEnvironment.GetPublishedVersion()).ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageDlg.Show(this, ex);
        //        Close();
        //    }
        //    finally
        //    {
        //        BusyIndicatorMainWindow.IsBusy = false;
        //    }
        //}

        ///// <summary>
        ///// Invoked when a caller has changed its busy status, and wants to show or hide the busy indicator.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void BusyIndicator_BusyChanged(object sender, BusyIndicatorEventArgs e)
        //{
        //    if (CheckAccess())
        //    {
        //        BusyIndicatorMainWindow.BusyContent = e.BusyContent;
        //        BusyIndicatorMainWindow.IsBusy = e.IsBusy;
        //    }
        //    else
        //    {
        //        TimeSpan delayTime = e.ShowBusyContent ? new TimeSpan(0, 0, 0, 0) : new TimeSpan(1, 0, 0, 0);

        //        // If this call comes from a different thread, dispatch it with waiting, so that any subsequent sync calls dont overtake me
        //        Dispatcher.Invoke(() =>
        //        {
        //            BusyIndicatorMainWindow.BusyContent = e.BusyContent;
        //            BusyIndicatorMainWindow.IsBusy = e.IsBusy;
        //            BusyIndicatorMainWindow.DisplayAfter = delayTime;
        //        });
        //    }
        //}

        /// <summary>
        /// When the user select a menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControlMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (_selectedIndex != TabControlMainWindowMenu.SelectedIndex)
            //{
            //    if (TabControlMainWindowMenu.SelectedItem != null)
            //    {
            //        // If the newly created control is a clever one, it can get de-activated
            //        if (_selectedTabItem != null && _selectedTabItem.Content is IWarehouseControl)
            //            (_selectedTabItem.Content as IWarehouseControl).Deactivate();

            //        _selectedTabItem = (TabItem)TabControlMainWindowMenu.SelectedItem;

            //        if (_selectedTabItem.Content == null)
            //        {
            //            switch (_selectedTabItem.Header.ToString())
            //            {
            //                // Pre-Scan menu
            //                case "Extraction":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Extraction;
            //                    _selectedTabItem.Content = new Controls.ExtractionUX();
            //                    break;
            //                case "Dispatching":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Dispatching;
            //                    _selectedTabItem.Content = new Controls.DispatchReturnUX();
            //                    break;
            //                case "Receiving":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Extraction;
            //                    _selectedTabItem.Content = new Controls.ExtractionUX();
            //                    //BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Receiving;
            //                    //_selectedTabItem.Content = new Controls.ReceiveDeliverUX();
            //                    break;
            //                case "Preparation":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Preparation;
            //                    _selectedTabItem.Content = new Controls.PreparationUX();
            //                    break;
            //                case "Scanning":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Scanning;
            //                    _selectedTabItem.Content = new Controls.ScanningUX();
            //                    break;
            //                // Post-Scan menu
            //                case "Indexing":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Indexing;
            //                    _selectedTabItem.Content = new Controls.PostScanIndexUX();
            //                    break;
            //                case "QC Managing":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.QCManaging;
            //                    _selectedTabItem.Content = new Controls.QCManagingUX();
            //                    break;
            //                case "QC Executing":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.QCExecution;
            //                    _selectedTabItem.Content = new Controls.QCExecuteUX();
            //                    break;
            //                case "Corrections":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Correction;
            //                    _selectedTabItem.Content = new Controls.CorrectionUX();
            //                    break;
            //                case "Packaging":
            //                //BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Packaging;
            //                //_selectedTabItem.Content = new Controls.PackagingUX();
            //                //break;
            //                case "Taping":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Taping;
            //                    _selectedTabItem.Content = new Controls.TapingUX();
            //                    break;
            //                case "Uploading":
            //                    //BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Uploading;
            //                    //_selectedTabItem.Content = new Controls.UploadUX();
            //                    break;
            //                case "Client QC":
            //                //BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.CQC;
            //                //_selectedTabItem.Content = new Controls.CQCDownloadUX_2();
            //                //break;
            //                case "Re-Assembly":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.ReAssembly;
            //                    _selectedTabItem.Content = new Controls.ReAssemblyUX();
            //                    break;
            //                case "Returning":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Returning;
            //                    _selectedTabItem.Content = new Controls.DispatchReturnUX();
            //                    break;
            //                case "Delivering":
            //                //BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Delivering;
            //                //_selectedTabItem.Content = new Controls.ReceiveDeliverUX();
            //                //break;
            //                case "Re-Filling":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.ReFiling;
            //                    _selectedTabItem.Content = new Controls.ReFillingUX();
            //                    break;
            //                // Administration menu
            //                case "Support Tools":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Reverting;
            //                    _selectedTabItem.Content = new Controls.SupportToolsUX();
            //                    break;
            //                case "Administration":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Security;
            //                    _selectedTabItem.Content = new Controls.AdministrationUX();
            //                    break;
            //                // Shared
            //                case "Transport Load":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.TransportLoad;
            //                    _selectedTabItem.Content = new Controls.TransportLoadUX();
            //                    break;
            //                case "Search":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Search;
            //                    _selectedTabItem.Content = new Controls.AdvancedSearchUX();
            //                    break;
            //                case "Advanced Search":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.AdvancedSearch;
            //                    _selectedTabItem.Content = new Controls.MoreAdvancedSearchUX();
            //                    break;
            //                case "Reports":
            //                    BrsMsEnvironment.SelectedMenuOption = BrsMsMenu.Reports;
            //                    _selectedTabItem.Content = new Controls.ReportingUX();
            //                    break;
            //            }

            //            // If the newly created control is a clever one, it has a Feedback event
            //            if (_selectedTabItem.Content is RichUserControl)
            //                (_selectedTabItem.Content as RichUserControl).Feedback += content_Feedback;
            //        }

            //        // If the newly created control is a clever one, it can get activated
            //        if (_selectedTabItem.Content is IWarehouseControl)
            //            (_selectedTabItem.Content as IWarehouseControl).Activate();
            //    }
            //}

            //_selectedIndex = TabControlMainWindowMenu.SelectedIndex;
        }

        #endregion

        #region Methods

        public ViewMobileManagerViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _securityHelper = new SecurityHelper(eventAggreagator);
            InitialiseMobileManagerView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseMobileManagerView()
        {
            InitialiseViewControls();

            // Subscribe to this event to navigate to the specified tab
            _eventAggregator.GetEvent<NavigationEvent>().Subscribe(Navigation_Event, true);

            // Subscribe to this event to update the billing period on the UI
            _eventAggregator.GetEvent<BillingPeriodEvent>().Subscribe(BillingPeriod_Event, true);

            // Add application functionality based on security roles
            TabCollection = new ObservableCollection<TabItem>();
            TabCollection.Add(new TabItem() { Header = "Administration" });
            if (_securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) ||
                _securityHelper.IsUserInRole(SecurityRole.AccountsUser.Value()))
                TabCollection.Add(new TabItem() { Header = "Accounts" });
            if (_securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) ||
                _securityHelper.IsUserInRole(SecurityRole.BillingUser.Value()))
                TabCollection.Add(new TabItem() { Header = "Billing" });
            TabCollection.Add(new TabItem() { Header = "Activity Log" });
            TabCollection.Add(new TabItem() { Header = "Reports" });
            if (_securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) ||
                _securityHelper.IsUserInRole(SecurityRole.DataManager.Value()))
                TabCollection.Add(new TabItem() { Header = "System Tools" });
            if (_securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || 
                _securityHelper.IsUserInRole(SecurityRole.DataManager.Value()))
                TabCollection.Add(new TabItem() { Header = "Configuration" });
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
        }

        /// <summary>
        /// When the user select a menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetTabContent(TabItem tabItem)
        {
            MobileManagerEnvironment.SelectedProcessMenu = EnumHelper.GetEnumFromDescription<ProcessMenuOption>(tabItem.Header.ToString());

            switch (tabItem.Header.ToString())
            {
                case "Administration":
                    if (tabItem.Content == null)
                        SelectedTab.Content = new ViewMobileAdmin();
                    break;
                case "Accounts":
                    if (tabItem.Content == null)
                        SelectedTab.Content = new ViewAccount(_eventAggregator);
                    break;
                case "Billing":
                    if (tabItem.Content == null)
                        SelectedTab.Content = new ViewBilling();
                    break;
                case "Activity Log":
                    if (tabItem.Content == null)
                        SelectedTab.Content = new ViewAuditLog();
                    break;
                case "Reports":
                    if (tabItem.Content == null)
                        SelectedTab.Content = new ViewReports();
                    break;
                case "System Tools":
                    if (tabItem.Content == null)
                        SelectedTab.Content = new ViewSystemTools();
                    break;
                case "Configuration":
                    if (tabItem.Content == null)
                        SelectedTab.Content = new ViewConfig();
                    break;
            }

            _selectedTabIndex = tabItem.TabIndex;
        }

        /// <summary>
        /// Read the app settings from the database
        /// </summary>
        private async void ReadAppSettingsAsync()
        {
            try
            {
                AppSetting appSetting = null;

                await Task.Run(() => appSetting = new AppSettingModel(_eventAggregator).ReadAppSettings());

                if (appSetting != null)
                {
                    //BillingPeriod = appSetting.BillingPeriod;
                    //BillingPeriodOpen = appSetting.IsBillingPeriodOpen ? "(Open)" : "(Closed)";
                    //MobileManagerEnvironment.BillingPeriod = appSetting.BillingPeriod;
                    //MobileManagerEnvironment.IsBillingPeriodOpen = appSetting.IsBillingPeriodOpen;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewMobileManagerViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadAppSettingsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion
    }
}
