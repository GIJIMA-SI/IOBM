using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewBasicSearchViewModel : BindableBase
    {
        #region Properties & Attributes

        private SearchEngineModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand SearchCommand { get; set; }
        public DelegateCommand SearchKeyDownCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the entered search criteria
        /// </summary>
        public string SearchCriteria
        {
            get { return _searchCriteria; }
            set { SetProperty(ref _searchCriteria, value); }
        }
        private string _searchCriteria = string.Empty;

        /// <summary>
        /// Holds the selected client result
        /// </summary>
        public Client SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                SetProperty(ref _selectedClient, value);
                if (value != null)
                    _eventAggregator.GetEvent<SearchResultEvent>().Publish(value.pkClientID);
            }
        }
        private Client _selectedClient = null;

        /// <summary>
        /// Holds the flag if there are multiple result entities
        /// </summary>
        public bool MultipleResults
        {
            get { return _multipleResults; }
            set { SetProperty(ref _multipleResults, value); }
        }
        private bool _multipleResults = false;

        /// <summary>
        /// 
        /// The collection of client search results
        /// </summary>
        public ObservableCollection<Client> ClientResults
        {
            get { return _clientResults; }
            set { SetProperty(ref _clientResults, value); }
        }
        private ObservableCollection<Client> _clientResults = null;

        #region Required Fields

        #endregion

        #region View Lookup Data Collections

        #endregion

        #region Input Validation

        #endregion

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewBasicSearchViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseSearchView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseSearchView()
        {
            _model = new SearchEngineModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => SearchCriteria);
            SearchCommand = new DelegateCommand(ExecuteSearch, CanExecute).ObservesProperty(() => SearchCriteria);
            SearchKeyDownCommand = new DelegateCommand(ExecuteSearch, CanExecute).ObservesProperty(() => SearchCriteria);
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SearchCriteria = string.Empty;
            ClientResults = null;
        }

        #region Lookup Data Loading

        #endregion

        #region Command Execution

        /// <summary>
        /// Validate if the save functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecute()
        {
            return !string.IsNullOrWhiteSpace(SearchCriteria);
        }

        /// <summary>
        /// Execute the cancel process
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Execute when the search command button is clicked 
        /// </summary>
        private async void ExecuteSearch()
        {
            try
            {
                ObservableCollection<Client> results = ClientResults = null;

                if (SearchCriteria.Trim().Length == 8)
                    results = await Task.Run(() => _model.SearchForClient(SearchCriteria.ToUpper().Trim(), SearchEntity.EmployeeNumber, false));

                if ((results == null || results.Count == 0) && (SearchCriteria.Trim().Length == 10 || SearchCriteria.Trim().Length == 14))
                    results = await Task.Run(() => _model.SearchForClient(SearchCriteria.ToUpper().Trim(), SearchEntity.PrimaryCellNumber, false));

                if ((results == null || results.Count == 0) && SearchCriteria.Trim().Length == 13)
                    results = await Task.Run(() => _model.SearchForClient(SearchCriteria.ToUpper().Trim(), SearchEntity.IDNumber, false));

                if ((results == null || results.Count == 0) && SearchCriteria.Trim().Contains("@"))
                    results = await Task.Run(() => _model.SearchForClient(SearchCriteria.ToUpper().Trim(), SearchEntity.Email, false));

                if (results == null || results.Count == 0)
                    results = await Task.Run(() => _model.SearchForClient(SearchCriteria.ToUpper().Trim(), SearchEntity.Other, false));

                // Publish the event so the cellular view can load the client data if found else send no sults found message
                if (results != null && results.Count == 1)
                    SelectedClient = results[0];
                else if (results != null && results.Count > 1)
                    ClientResults = results;
                else
                    SearchCriteria = "NO RESULTS FOUND!";

                MultipleResults = results != null && results.Count > 1 ? true : false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewBasicSearchViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ExecuteSearch",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #endregion
    }
}
