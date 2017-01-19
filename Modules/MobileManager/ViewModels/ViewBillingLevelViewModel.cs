using Gijima.IOBM.Infrastructure.Events;
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
    public class ViewBillingLevelViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private BillingLevelModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) billing level entity
        /// </summary>
        public BillingLevel SelectedBillingLevel
        {
            get { return _selectedBillingLevel; }
            set
            {
                if (value != null)
                {
                    BillingLevelDescription = value.LevelDescription;
                    BillingLevelState = value.IsActive;
                    SetProperty(ref _selectedBillingLevel, value);
                }
            }
        }
        private BillingLevel _selectedBillingLevel = new BillingLevel();

        /// <summary>
        /// The selected billing level state
        /// </summary>
        public bool BillingLevelState
        {
            get { return _billingLevelState; }
            set { SetProperty(ref _billingLevelState, value); }
        }
        private bool _billingLevelState;

        /// <summary>
        /// The collection of billing Levels from the database
        /// </summary>
        public ObservableCollection<BillingLevel> BillingLevelCollection
        {
            get { return _billingLevelCollection; }
            set { SetProperty(ref _billingLevelCollection, value); }
        }
        private ObservableCollection<BillingLevel> _billingLevelCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered billing level description
        /// </summary>
        public string BillingLevelDescription
        {
            get { return _billingLevelDescription; }
            set { SetProperty(ref _billingLevelDescription, value); }
        }
        private string _billingLevelDescription = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidBillingLevel
        {
            get { return _requiredFieldColour; }
            set { SetProperty(ref _requiredFieldColour, value); }
        }
        private Brush _requiredFieldColour = Brushes.Red;

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
                    case "BillingLevelDescription":
                        ValidBillingLevel = string.IsNullOrEmpty(BillingLevelDescription) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewBillingLevelViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseBillingLevelView();
        }

        private async void InitialiseBillingLevelView()
        {
            _model = new BillingLevelModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => BillingLevelDescription);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => BillingLevelDescription);

            // Load the view data
            await ReadBillingLevelsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedBillingLevel = new BillingLevel();
        }

        /// <summary>
        /// Load all the billing levels from the database
        /// </summary>
        private async Task ReadBillingLevelsAsync()
        {
            try
            {
                BillingLevelCollection = await Task.Run(() => _model.ReadBillingLevels(false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading
        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecute()
        {
            return !string.IsNullOrWhiteSpace(BillingLevelDescription);
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
            SelectedBillingLevel.LevelDescription = BillingLevelDescription.ToUpper();
            SelectedBillingLevel.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedBillingLevel.ModifiedDate = DateTime.Now;
            SelectedBillingLevel.IsActive = BillingLevelState;

            if (SelectedBillingLevel.pkBillingLevelID == 0)
                result = _model.CreateBillingLevel(SelectedBillingLevel);
            else
                result = _model.UpdateBillingLevel(SelectedBillingLevel);

            if (result)
            {
                InitialiseViewControls();
                await ReadBillingLevelsAsync();
            }
        }

        #endregion

        #endregion
    }
}
