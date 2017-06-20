using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Views;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewReferenceDataCFViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private IEventAggregator _eventAggregator;

        #region Commands

        #endregion

        #region Properties

        #region Required Fields

        /// <summary>
        /// The selected view to display
        /// </summary>
        public object SelectedView
        {
            get { return _selectedView; }
            set { SetProperty(ref _selectedView, value); }
        }
        private object _selectedView = null;

        /// <summary>
        /// The selected option to display
        /// in the dropdown
        /// </summary>
        public string SelectedViewName
        {
            get { return _selectedViewName; }
            set
            {
                SetProperty(ref _selectedViewName, value);
                SetSelectedView(value);
            }
        }
        private string _selectedViewName = null;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// Collection of all the reference data options
        /// </summary>
        public ObservableCollection<string> ReferenceOptionCollection
        {
            get { return _referenceOptionCollection; }
            set { SetProperty(ref _referenceOptionCollection, value); }
        }
        private ObservableCollection<string> _referenceOptionCollection;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidReferenceOption
        {
            get { return _validReferenceOption; }
            set { SetProperty(ref _validReferenceOption, value); }
        }
        private Brush _validReferenceOption = Brushes.Red;

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
                    case "SelectedViewName":
                        ValidReferenceOption = SelectedViewName == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
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
        public ViewReferenceDataCFViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            LoadReferenceOptions();
        }

        #region Lookup Data Loading

        /// <summary>
        /// Loads the available reference data options
        /// </summary>
        public void LoadReferenceOptions()
        {
            ReferenceOptionCollection = new ObservableCollection<string>();

            foreach (ReferenceDataOption referenceDataOption in Enum.GetValues(typeof(ReferenceDataOption)))
            {
                ReferenceOptionCollection.Add(EnumHelper.GetDescriptionFromEnum(referenceDataOption));
            }
            SelectedViewName = ReferenceOptionCollection[0];
        }

        /// <summary>
        /// Set the selected view object
        /// </summary>
        /// <param name="view"></param>
        public void SetSelectedView(string view)
        {
            ReferenceDataOption dataOption = EnumHelper.GetEnumFromDescription<ReferenceDataOption>(view);
            switch (dataOption)
            {
                case ReferenceDataOption.None:
                    SelectedView = null;
                    break;
                case ReferenceDataOption.ViewBillingLevel:
                    SelectedView = new ViewBillingLevel();
                    break;
                case ReferenceDataOption.ViewCity:
                    SelectedView = new ViewCity();
                    break;
                case ReferenceDataOption.ViewClientSite:
                    SelectedView = new ViewClientSite();
                    break;
                case ReferenceDataOption.ViewCompany:
                    SelectedView = new ViewCompany();
                    break;
                case ReferenceDataOption.ViewCompanyGroup:
                    SelectedView = new ViewCompanyGroup();
                    break;
                case ReferenceDataOption.ViewContractService:
                    SelectedView = new ViewContractService();
                    break;
                case ReferenceDataOption.ViewDeviceMake:
                    SelectedView = new ViewDeviceMake();
                    break;
                case ReferenceDataOption.ViewDeviceModel:
                    SelectedView = new ViewDeviceModel();
                    break;
                case ReferenceDataOption.ViewPackage:
                    SelectedView = new ViewPackage();
                    break;
                case ReferenceDataOption.ViewProvince:
                    SelectedView = new ViewProvince();
                    break;
                case ReferenceDataOption.ViewServiceProvider:
                    SelectedView = new ViewServiceProvider();
                    break;
                case ReferenceDataOption.ViewStatus:
                    SelectedView = new ViewStatus();
                    break;
                case ReferenceDataOption.ViewSuburb:
                    SelectedView = new ViewSuburb();
                    break;
                case ReferenceDataOption.ViewDepartment:
                    SelectedView = new ViewDepartment();
                    break;
                case ReferenceDataOption.ViewLineManager:
                    SelectedView = new ViewLineManager();
                    break;
                default:
                    SelectedView = null;
                    break;
            }
        }

        #endregion

        #region Command Execution

        #endregion

        #endregion


    }
}
