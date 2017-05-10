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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewAccountViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private bool _isNewInvoice = false;
        private bool _isNewInvoiceItem = false;
        private InvoiceModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private DateTime _validBillingDate = DateTime.ParseExact(string.Format("01/{0}/{1}", MobileManagerEnvironment.BillingPeriod.Substring(5, 2)
                                                                                           , MobileManagerEnvironment.BillingPeriod.Substring(0, 4))
                                                                                           , "dd/MM/yyyy", CultureInfo.InvariantCulture);

        #region Commands

        public DelegateCommand CancelInvoiceCommand { get; set; }
        public DelegateCommand AddInvoiceCommand { get; set; }
        public DelegateCommand SaveInvoiceCommand { get; set; }
        public DelegateCommand CancelItemCommand { get; set; }
        public DelegateCommand AddItemCommand { get; set; }
        public DelegateCommand DeleteItemCommand { get; set; }
        public DelegateCommand SupplierCommand { get; set; }
        public DelegateCommand CancelFilterCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The selected client primary key
        /// </summary>
        private int SelectedClientID
        {
            get { return _selectedClientID; }
            set { SetProperty(ref _selectedClientID, value); }
        }
        private int _selectedClientID = 0;

        /// <summary>
        /// Holds the selected (current) invoice entity
        /// </summary>
        public Invoice SelectedInvoice
        {
            get { return _selectedInvoice; }
            set
            {
                if (_isNewInvoice || (value != null && value.pkInvoiceID > 0))
                {
                    if (_isNewInvoice)
                        InitialiseInvoiceControls();

                    SetProperty(ref _selectedInvoice, value);
                    _isNewInvoice = false;

                    // Invoice required fields
                    SelectedInvoiceNumber = value != null && value.InvoiceNumber != null ? value.InvoiceNumber : string.Empty;
                    SelectedInvoiceDate = value != null && value.InvoiceDate != null && value.InvoiceDate.Value.Date >= _validBillingDate.Date ? value.InvoiceDate.Value : _validBillingDate;
                    SelectedPrivateDue = value != null && value.PrivateDue != null ? value.PrivateDue.Value : 0;
                    SelectedCompanyDue = value != null && value.CompanyDue != null ? value.CompanyDue.Value : 0;
                    SelectedService = value != null && ServiceCollection != null && value.Service1 != null ? ServiceCollection.Where(p => p.pkServiceID == value.fkServiceID).FirstOrDefault() :
                                      ServiceCollection != null ? ServiceCollection.Where(p => p.pkServiceID == 0).FirstOrDefault() : null;

                    // Publish the event to execute the ShowInvoiceReport method on the Accounts View
                    InvoiceReportEventArgs eventArgs = new InvoiceReportEventArgs();
                    eventArgs.InvoiceID = value != null ? value.pkInvoiceID : 0;
                    eventArgs.ServiceDescription = SelectedService != null ? SelectedService.ServiceDescription : string.Empty;
                    _eventAggregator.GetEvent<ShowInvoiceReportEvent>().Publish(eventArgs);

                    // Read all the invoice items linked 
                    // to the selected invoice
                    ReadClientInvoiceItemsAsync();
                }
            }
        }
        private Invoice _selectedInvoice;

        /// <summary>
        /// The selected invoice item index
        /// </summary>
        public int SelectedInvoiceItemIndex
        {
            get { return _selectedInvoiceItemIndex; }
            set { SetProperty(ref _selectedInvoiceItemIndex, value); }
        }
        private int _selectedInvoiceItemIndex;

        /// <summary>
        /// Holds the selected invoice item entity
        /// </summary>
        public InvoiceDetail SelectedInvoiceItem
        {
            get { return _selectedInvoiceItem; }
            set
            {
                _isNewInvoiceItem = false;
                SetProperty(ref _selectedInvoiceItem, value);

                // Invoice item required fields
                if (value != null && value.fkServiceProviderID > 0)
                {
                    SelectedSupplier = SupplierCollection != null && value.ServiceProvider != null ?
                                       SupplierCollection.Where(p => p.pkServiceProviderID == value.fkServiceProviderID).FirstOrDefault() :
                                       SupplierCollection != null ? SupplierCollection.Where(p => p.pkServiceProviderID == 0).FirstOrDefault() : null;
                    SelectedPrivateBilling = value.IsPrivate;
                    SelectedItemDescription = value.ItemDescription != null ? value.ItemDescription : string.Empty;
                    SelectedItemReference = value.ReferenceNumber != null ? value.ReferenceNumber : string.Empty;
                    SelectedItemAmount = value.ItemAmount.ToString();
                }
            }
        }
        private InvoiceDetail _selectedInvoiceItem;

        /// <summary>
        /// The selected invoice number
        /// </summary>
        public string SelectedInvoiceNumber
        {
            get { return _selectedInvoiceNumber; }
            set { SetProperty(ref _selectedInvoiceNumber, value); }
        }
        private string _selectedInvoiceNumber = string.Empty;

        /// <summary>
        /// The calculated private due amount incl VAT
        /// </summary>
        public decimal SelectedPrivateDue
        {
            get { return _selectedPrivateDue; }
            set { SetProperty(ref _selectedPrivateDue, value); }
        }
        private decimal _selectedPrivateDue = 0;

        /// <summary>
        /// The calculated company due amount incl VAT
        /// </summary>
        public decimal SelectedCompanyDue
        {
            get { return _selectedCompanyDue; }
            set { SetProperty(ref _selectedCompanyDue, value); }
        }
        private decimal _selectedCompanyDue = 0;

        /// <summary>
        /// The selected private or company billing option
        /// </summary>
        public bool SelectedPrivateBilling
        {
            get { return _selectedPrivateBilling; }
            set { SetProperty(ref _selectedPrivateBilling, value); }
        }
        private bool _selectedPrivateBilling = false;

        /// <summary>
        /// The service filter option
        /// </summary>
        public bool SelectedServiceGroup
        {
            get { return _selectedServiceGroup; }
            set
            {
                SetProperty(ref _selectedServiceGroup, value);

                if (value)
                {
                    ShowPeriodFilter = Visibility.Collapsed;
                    ShowServiceFilter = Visibility.Visible;
                }
                else
                {
                    ShowServiceFilter = Visibility.Collapsed;
                }
            }
        }
        private bool _selectedServiceGroup = false;

        /// <summary>
        /// The account period filter option
        /// </summary>
        public bool SelectedPeriodGroup
        {
            get { return _selectedPeriodGroup; }
            set
            {
                SetProperty(ref _selectedPeriodGroup, value);
                if (value)
                {
                    ShowPeriodFilter = Visibility.Visible;
                    ShowServiceFilter = Visibility.Collapsed;

                    // Populate the account period filters
                    ReadAccountPeriodsAsync();
                }
                else
                {
                    ShowPeriodFilter = Visibility.Collapsed;
                }
            }
        }
        private bool _selectedPeriodGroup = true;

        /// <summary>
        /// The flag to hide or show the service filter
        /// </summary>
        public Visibility ShowServiceFilter
        {
            get { return _showServiceFilter; }
            set { SetProperty(ref _showServiceFilter, value); }
        }
        private Visibility _showServiceFilter = Visibility.Collapsed;

        /// <summary>
        /// The flag to hide or show the period filter
        /// </summary>
        public Visibility ShowPeriodFilter
        {
            get { return _selectedPeriodFilter; }
            set { SetProperty(ref _selectedPeriodFilter, value); }
        }
        private Visibility _selectedPeriodFilter = Visibility.Visible;

        /// <summary>
        /// The selected filter
        /// </summary>
        public Service SelectedServiceFilter
        {
            get { return _selectedServiceFilter; }
            set
            {
                SetProperty(ref _selectedServiceFilter, value);
                Task.Run(() => ReadClientInvoicesAsync());
            }
        }
        private Service _selectedServiceFilter = null;

        /// <summary>
        /// The entered reference
        /// </summary>
        public string SelectedAccountMonth
        {
            get { return _selectedAccountMonth; }
            set { SetProperty(ref _selectedAccountMonth, value); }
        }
        private string _selectedAccountMonth = string.Empty;
        
        /// <summary>
        /// The entered reference
        /// </summary>
        public string SelectedAccountYear
        {
            get { return _selectedAccountYear; }
            set
            {
                SetProperty(ref _selectedAccountYear, value);
                Task.Run(() => ReadClientInvoicesAsync());
            }
        }
        private string _selectedAccountYear = string.Empty;

        /// <summary>
        /// The collection of invoices from the database
        /// </summary>
        public ObservableCollection<Invoice> InvoiceCollection
        {
            get { return _invoiceCollection; }
            set { SetProperty(ref _invoiceCollection, value); }
        }
        private ObservableCollection<Invoice> _invoiceCollection = null;

        /// <summary>
        /// The collection of invoice items from the database
        /// </summary>
        public ObservableCollection<InvoiceDetail> InvoiceItemsCollection
        {
            get { return _invoiceItemsCollection; }
            set { SetProperty(ref _invoiceItemsCollection, value); }
        }
        private ObservableCollection<InvoiceDetail> _invoiceItemsCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of service providers from the database
        /// </summary>
        public ObservableCollection<ServiceProvider> SupplierCollection
        {
            get { return _supplierCollection; }
            set { SetProperty(ref _supplierCollection, value); }
        }
        private ObservableCollection<ServiceProvider> _supplierCollection = null;

        /// <summary>
        /// The collection of services from the database
        /// </summary>
        public ObservableCollection<Service> ServiceCollection
        {
            get { return _serviceCollection; }
            set { SetProperty(ref _serviceCollection, value); }
        }
        private ObservableCollection<Service> _serviceCollection = null;

        /// <summary>
        /// The collection of accounting months
        /// </summary>
        public List<string> AccountMonthCollection
        {
            get { return _accountMonthCollection; }
            set { SetProperty(ref _accountMonthCollection, value); }
        }
        private List<string> _accountMonthCollection = null;

        /// <summary>
        /// The collection of accounting years
        /// </summary>
        public List<string> AccountYearCollection
        {
            get { return _accountYearCollection; }
            set { SetProperty(ref _accountYearCollection, value); }
        }
        private List<string> _accountYearCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected invoice make
        /// </summary>
        public DateTime SelectedInvoiceDate
        {
            get { return _selectedInvoiceDate; }
            set { SetProperty(ref _selectedInvoiceDate, value); }
        }
        private DateTime _selectedInvoiceDate;

        /// <summary>
        /// The selected service
        /// </summary>
        public Service SelectedService
        {
            get { return _selectedService; }
            set { SetProperty(ref _selectedService, value); }
        }
        private Service _selectedService = null;

        /// <summary>
        /// The selected service provider
        /// </summary>
        public ServiceProvider SelectedSupplier
        {
            get { return _selectedSupplier; }
            set { SetProperty(ref _selectedSupplier, value); }
        }
        private ServiceProvider _selectedSupplier = null;

        /// <summary>
        /// The entered item description
        /// </summary>
        public string SelectedItemDescription
        {
            get { return _selectedItemDescription; }
            set { SetProperty(ref _selectedItemDescription, value); }
        }
        private string _selectedItemDescription = string.Empty;

        /// <summary>
        /// The entered reference
        /// </summary>
        public string SelectedItemReference
        {
            get { return _selectedItemReference; }
            set { SetProperty(ref _selectedItemReference, value); }
        }
        private string _selectedItemReference = string.Empty;

        /// <summary>
        /// The entered item amount incl VAT
        /// </summary>
        public string SelectedItemAmount
        {
            get { return _selectedItemAmount; }
            set { SetProperty(ref _selectedItemAmount, value); }
        }
        private string _selectedItemAmount = "0";

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidInvoiceDate
        {
            get { return _validInvoiceDate; }
            set { SetProperty(ref _validInvoiceDate, value); }
        }
        private Brush _validInvoiceDate = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidService
        {
            get { return _validService; }
            set { SetProperty(ref _validService, value); }
        }
        private Brush _validService = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSupplier
        {
            get { return _validSupplier; }
            set { SetProperty(ref _validSupplier, value); }
        }
        private Brush _validSupplier = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidItemDescription
        {
            get { return _validItemDescription; }
            set { SetProperty(ref _validItemDescription, value); }
        }
        private Brush _validItemDescription = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidItemReference
        {
            get { return _validItemReference; }
            set { SetProperty(ref _validItemReference, value); }
        }
        private Brush _validItemReference = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidItemAmount
        {
            get { return _validItemAmount; }
            set { SetProperty(ref _validItemAmount, value); }
        }
        private Brush _validItemAmount = Brushes.Red;

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
                    case "SelectedInvoiceDate":
                        ValidInvoiceDate = SelectedInvoiceDate.Date < _validBillingDate.Date ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedService":
                        ValidService = SelectedService != null && SelectedService.pkServiceID > 0 && SelectedService.ServicePreFix == "EQP" ? Brushes.Silver : Brushes.Red; break;
                    case "SelectedSupplier":
                        ValidSupplier = SelectedSupplier == null || SelectedSupplier.pkServiceProviderID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedItemDescription":
                        ValidItemDescription = string.IsNullOrEmpty(SelectedItemDescription) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedItemReference":
                        ValidItemReference = string.IsNullOrEmpty(SelectedItemReference) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedItemAmount":
                        ValidItemAmount = string.IsNullOrWhiteSpace(SelectedItemAmount) && Convert.ToDecimal(SelectedItemAmount) <= 0 ? Brushes.Red : Brushes.Silver; break;

                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Read the client data for the client ID
        /// </summary>
        /// <param name="sender">The client ID.</param>
        private async void ReadClientInvoices_Event(int sender)
        {
            if (MobileManagerEnvironment.ClientID == 0)
                MobileManagerEnvironment.ClientID = sender;

            await ReadClientInvoicesAsync();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewAccountViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _securityHelper = new SecurityHelper(_eventAggregator);
            InitialiseInvoiceView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseInvoiceView()
        {
            _model = new InvoiceModel(_eventAggregator);

            // Initialise the view commands
            CancelInvoiceCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedInvoiceDate)
                                                                                       .ObservesProperty(() => SelectedService);
            AddInvoiceCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedClientID);
            SaveInvoiceCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedInvoiceDate)
                                                                                 .ObservesProperty(() => SelectedService)
                                                                                 .ObservesProperty(() => InvoiceItemsCollection);
            CancelItemCommand = new DelegateCommand(ExecuteItemCancel, CanExecuteItemCancel).ObservesProperty(() => SelectedSupplier);
            AddItemCommand = new DelegateCommand(ExecuteItemAdd, CanExecuteItemAdd).ObservesProperty(() => SelectedSupplier)
                                                                                   .ObservesProperty(() => SelectedItemDescription)
                                                                                   .ObservesProperty(() => SelectedItemReference)
                                                                                   .ObservesProperty(() => SelectedItemAmount);
            DeleteItemCommand = new DelegateCommand(ExecuteItemDelete, CanExecuteItemDelete).ObservesProperty(() => SelectedInvoiceItem);
            SupplierCommand = new DelegateCommand(ExecuteShowSPView, CanExecuteMaintenace);
            CancelFilterCommand = new DelegateCommand(ExecuteCancelFilter);

            // Subscribe to this event to read invoices linked to selectede client
            _eventAggregator.GetEvent<ReadInvoicesEvent>().Subscribe(ReadClientInvoices_Event, true);

            // Load the view data
            await ReadClientInvoicesAsync();
            await ReadServiceProvidersAsync();
            await ReadServicesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseInvoiceControls()
        {
            SelectedInvoiceNumber = string.Empty;
            SelectedInvoiceDate = DateTime.Now.Date < _validBillingDate.Date ? _validBillingDate : DateTime.Now;
            SelectedService = ServiceCollection != null ? ServiceCollection.Where(p => p.pkServiceID == 0).FirstOrDefault() : null;
            SelectedServiceGroup = SelectedPeriodGroup = false;
            SelectedPrivateDue = SelectedCompanyDue = 0;
            InvoiceItemsCollection = null;
            _isNewInvoice = false;
            InitialiseInvoiceItemsControls();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseInvoiceItemsControls()
        {
            SelectedInvoiceItemIndex = -1;
            SelectedSupplier = SupplierCollection != null ? SupplierCollection.Where(p => p.pkServiceProviderID == 0).FirstOrDefault() : null;
            SelectedItemDescription = SelectedItemReference = string.Empty;
            SelectedItemAmount = "0";
            SelectedPrivateBilling = false;
            _isNewInvoiceItem = true;
        }

        /// <summary>
        /// Load all the invoice linked to the selected client from the database
        /// </summary>
        private async Task ReadClientInvoicesAsync()
        {
            try
            {
                string accPeriodFilter = "None";
                int serviceFilter = 0;

                if (SelectedPeriodGroup && !string.IsNullOrEmpty(SelectedAccountMonth) && !string.IsNullOrEmpty(SelectedAccountYear))
                    accPeriodFilter = string.Format("{0}/{1}", SelectedAccountYear, SelectedAccountMonth);

                if (SelectedServiceGroup && SelectedServiceFilter != null && SelectedServiceFilter.pkServiceID > 0)
                    serviceFilter = SelectedServiceFilter.pkServiceID;

                SelectedClientID = MobileManagerEnvironment.ClientID;
                InvoiceCollection = await Task.Run(() => _model.ReadInvoicesForClient(MobileManagerEnvironment.ClientID, accPeriodFilter, serviceFilter));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadClientInvoicesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the invoice items linked to the selected invoice from the database
        /// </summary>
        private async void ReadClientInvoiceItemsAsync()
        {
            try
            {
                if (SelectedInvoice != null)
                    InvoiceItemsCollection = await Task.Run(() => _model.ReadInvoiceItems(SelectedInvoice.pkInvoiceID));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadClientInvoiceItemsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the service providers from the database
        /// </summary>
        private async Task ReadServiceProvidersAsync()
        {
            try
            {
                SupplierCollection = await Task.Run(() => new ServiceProviderModel(_eventAggregator).ReadServiceProviders(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadServiceProvidersAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the services from the database
        /// </summary>
        private async Task ReadServicesAsync()
        {
            try
            {
                ServiceCollection = await Task.Run(() => _model.ReadServices(true));

                // For creating invoices only the equipment service is allowed
                // TO DO - Make this configurable
                //ServiceCollection = new ObservableCollection<Service>(ServiceCollection.Where(p => p.ServicePreFix == "DEF" || p.ServicePreFix == "EQP").ToList());
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadServicesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the account periods from the InvoiceCollection
        /// </summary>
        private void ReadAccountPeriodsAsync()
        {
            try
            {
                if (InvoiceCollection.Count > 0)
                {
                    List<string> accountMonths = new List<string>();
 
                    for (int i = 1; i <= 12; i++)
                    {
                        accountMonths.Add(i.ToString().PadLeft(2, '0'));
                    }

                    AccountYearCollection = (from invoice in InvoiceCollection
                                             select invoice.InvoicePeriod.Substring(0, 4).ToString()).Distinct()
                                                                                                     .ToList();
                    accountMonths.Add(string.Empty);
                    AccountMonthCollection = accountMonths;
                    AccountYearCollection.Add(string.Empty);
                    SelectedAccountMonth = SelectedAccountYear = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadAccountPeriodsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Validate if the invoice cancel process can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteCancel()
        {
            return !string.IsNullOrEmpty(SelectedInvoiceNumber) ? true : false;
        }

        /// <summary>
        /// Execute when the cancel process
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseInvoiceControls();
        }

        /// <summary>
        /// Validate if the add invoice data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteAdd()
        {
            // Validate if the logged-in user can administrate the company the client is linked to
            return SelectedClientID > 0 && _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;
        }

        /// <summary>
        /// Execute the add new invoice process
        /// </summary>
        private void ExecuteAdd()
        {
            _isNewInvoice = true;
            SelectedInvoice = new Invoice();
        }

        /// <summary>
        /// Validate if the save invoice data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = SelectedClientID > 0 && _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result)
                result= SelectedInvoiceDate.Date > DateTime.MinValue.Date && InvoiceItemsCollection != null &&
                        InvoiceItemsCollection.Count > 0 && SelectedService != null && SelectedService.pkServiceID > 0 && SelectedService.ServicePreFix == "EQP" &&
                        (SelectedInvoice != null && SelectedInvoice.pkInvoiceID > 0 ? SelectedInvoice.IsPeriodClosed != null && !SelectedInvoice.IsPeriodClosed.Value : true);

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
                int invoiceID = SelectedInvoice.pkInvoiceID;

                SelectedInvoice.fkClientID = MobileManagerEnvironment.ClientID;
                SelectedInvoice.fkServiceID = SelectedService.pkServiceID;
                SelectedInvoice.PrivateDue = SelectedPrivateDue;
                SelectedInvoice.CompanyDue = _selectedCompanyDue;
                SelectedInvoice.InvoiceDate = SelectedInvoiceDate;
                SelectedInvoice.InvoicePeriod = string.Format("{0}/{1}", SelectedInvoiceDate.Year.ToString(), SelectedInvoiceDate.Month.ToString().PadLeft(2, '0'));
                SelectedInvoice.ModifiedBy = SecurityHelper.LoggedInUserFullName;
                SelectedInvoice.ModifiedDate = DateTime.Now;

                if (SelectedInvoice.pkInvoiceID == 0)
                    result = await Task.Run(() => _model.CreateInvoice(SelectedInvoice, InvoiceItemsCollection, SelectedService.ServicePreFix));
                else
                    result = await Task.Run(() => _model.UpdateInvoice(SelectedInvoice, InvoiceItemsCollection));

                if (result)
                {
                    await ReadClientInvoicesAsync();
                    SelectedInvoice = InvoiceCollection.Where(p => p.pkInvoiceID == invoiceID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteSave",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
}

        /// <summary>
        /// Validate the invoice item cancel process can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteItemCancel()
        {
            return SelectedSupplier != null && SelectedSupplier.pkServiceProviderID > 0;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private void ExecuteItemCancel()
        {
            InitialiseInvoiceItemsControls();
        }

        /// <summary>
        /// Validate the invoice item delete process can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteItemDelete()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = SelectedClientID > 0 && _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result)
                result = SelectedInvoice != null && SelectedInvoiceItem != null &&
                            (SelectedInvoice.pkInvoiceID > 0 ? SelectedInvoice.IsPeriodClosed != null && !SelectedInvoice.IsPeriodClosed.Value : true);

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private void ExecuteItemDelete()
        {
            try
            { 
                if (InvoiceItemsCollection.Contains(SelectedInvoiceItem))
                    InvoiceItemsCollection.Remove(SelectedInvoiceItem);

                SelectedPrivateDue = InvoiceItemsCollection.Where(p => p.IsPrivate).ToList().Sum(p => p.ItemAmount);
                SelectedCompanyDue = InvoiceItemsCollection.Where(p => !p.IsPrivate).ToList().Sum(p => p.ItemAmount);

                ExecuteItemCancel();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteItemDelete",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Validate if the add invoice item data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteItemAdd()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = SelectedClientID > 0 && _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result)
                result = SelectedSupplier != null && SelectedSupplier.pkServiceProviderID > 0 && !string.IsNullOrEmpty(SelectedItemDescription) &&
                         !string.IsNullOrEmpty(SelectedItemReference) && (!string.IsNullOrWhiteSpace(SelectedItemAmount) && Convert.ToDecimal(SelectedItemAmount) > 0) &&
                         (SelectedInvoice !=null && SelectedInvoice.pkInvoiceID > 0 ? SelectedInvoice.IsPeriodClosed != null && !SelectedInvoice.IsPeriodClosed.Value : true);

            return result;
        }

        /// <summary>
        /// Execute when the invoice item Add command button is clicked 
        /// </summary>
        private void ExecuteItemAdd()
        {
            try
            { 
                int invoiceItemID = 0;

                if (SelectedInvoiceItem == null)
                    SelectedInvoiceItem = new InvoiceDetail();

                if (InvoiceItemsCollection == null)
                    InvoiceItemsCollection = new ObservableCollection<InvoiceDetail>();

                if (SelectedInvoiceItem.ServiceProvider == null)
                    SelectedInvoiceItem.ServiceProvider = new ServiceProvider();

                // When an existing item was changed first remove it before adding
                if (!_isNewInvoiceItem && SelectedInvoiceItem != null)
                {
                    invoiceItemID = SelectedInvoiceItem.pkInvoiceItemID;
                    InvoiceItemsCollection.Remove(SelectedInvoiceItem);
                    SelectedInvoiceItem = new InvoiceDetail();
                    SelectedInvoiceItem.pkInvoiceItemID = invoiceItemID;
                }          

                SelectedInvoiceItem.fkInvoiceID = 0;
                SelectedInvoiceItem.fkServiceProviderID = SelectedSupplier.pkServiceProviderID;
                SelectedInvoiceItem.IsPrivate = SelectedPrivateBilling;
                SelectedInvoiceItem.ItemDescription = SelectedItemDescription.ToUpper();
                SelectedInvoiceItem.ReferenceNumber = SelectedItemReference.ToUpper();
                SelectedInvoiceItem.ItemAmount = Convert.ToDecimal(SelectedItemAmount);
                SelectedInvoiceItem.ModifiedBy = SecurityHelper.LoggedInUserFullName;
                SelectedInvoiceItem.ModifiedDate = DateTime.Now;
                SelectedInvoiceItem.ServiceProvider = SelectedSupplier;

                InvoiceItemsCollection.Add(SelectedInvoiceItem);
                InvoiceItemsCollection = new ObservableCollection<InvoiceDetail>(InvoiceItemsCollection);

                // Calculate the private and company due amounts
                SelectedPrivateDue = InvoiceItemsCollection.Where(p => p.IsPrivate).ToList().Sum(p => p.ItemAmount);
                SelectedCompanyDue = InvoiceItemsCollection.Where(p => !p.IsPrivate).ToList().Sum(p => p.ItemAmount);

                ExecuteItemCancel();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewAccountViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteItemAdd",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Execute when the accounts filer get cleared
        /// </summary>
        private void ExecuteCancelFilter()
        {
            SelectedPeriodGroup = SelectedServiceGroup = false;
            SelectedAccountMonth = SelectedAccountYear = string.Empty;
            SelectedServiceFilter = ServiceCollection.Where(p => p.pkServiceID == 0).FirstOrDefault();
        }

        /// <summary>
        /// Validate if the maintenace functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteMaintenace()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || _securityHelper.IsUserInRole(SecurityRole.AccountsUser.Value());
        }

        /// <summary>
        /// Execute when the show service provider command button is clicked 
        /// </summary>
        private async void ExecuteShowSPView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewServiceProvider(), "Service Provider Maintenance", PopupWindow.PopupButtonType.Close);
            //So width and heigh can be set auto for referencedata
            popupWindow.MaxHeight = popupWindow.MinHeight = 300;
            popupWindow.MaxWidth = popupWindow.MinWidth = 400;
            popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            popupWindow.ShowDialog();
            await ReadServiceProvidersAsync();
        }

        #endregion

        #endregion
    }
}
