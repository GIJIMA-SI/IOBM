using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Views;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewSystemToolsViewModel : BindableBase
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper;
        private int _selectedTabIndex = 0;

        #region Properties

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

        public ViewSystemToolsViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _securityHelper = new SecurityHelper(eventAggreagator);

            // Add application functionality based on security roles
            TabCollection = new ObservableCollection<TabItem>();
            if (_securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || 
                _securityHelper.IsUserInRole(SecurityRole.DataManager.Value()))
            {
                TabCollection.Add(new TabItem() { Header = "Data Validation" });
                TabCollection.Add(new TabItem() { Header = "Data Import" });
                TabCollection.Add(new TabItem() { Header = "Data Update" });
            }
        }

        /// <summary>
        /// When the user select a menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetTabContent(TabItem tabItem)
        {
            if (tabItem.Content == null)
            {
                switch (tabItem.Header.ToString())
                {
                    case "Data Validation":
                        SelectedTab.Content = new ViewDataValidation();
                        break;
                    case "Data Import":
                        SelectedTab.Content = new ViewDataImportInt();
                        break;
                    case "Data Update":
                        SelectedTab.Content = new ViewDataUpdate();
                        break;
                }

            }

            _selectedTabIndex = tabItem.TabIndex;
            MobileManagerEnvironment.SelectedToolsMenu = EnumHelper.GetEnumFromDescription<ToolsMenuOption>(tabItem.Header.ToString());

            // Publish this event to show the data validation control header
            _eventAggregator.GetEvent<DataValiationHeaderEvent>().Publish(true);
        }
    }
}
