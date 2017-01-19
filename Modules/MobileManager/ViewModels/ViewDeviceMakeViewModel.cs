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
    public class ViewDeviceMakeViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DevicesMakeModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
       
        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) device make entity
        /// </summary>
        public DeviceMake SelectedDeviceMake
        {
            get { return _selectedDeviceMake; }
            set
            {
                if (value != null)
                {
                    SelectedMakeDescription = value.MakeDescription;
                    MakeState = value.IsActive;
                    SetProperty(ref _selectedDeviceMake, value);
                }
            }
        }
        private DeviceMake _selectedDeviceMake = new DeviceMake();

        /// <summary>
        /// The selected DeviceMake state
        /// </summary>
        public bool MakeState
        {
            get { return _makeState; }
            set { SetProperty(ref _makeState, value); }
        }
        private bool _makeState;

        /// <summary>
        /// The collection of DeviceMakees from the database
        /// </summary>
        public ObservableCollection<DeviceMake> DeviceMakeCollection
        {
            get { return _DeviceMakeCollection; }
            set { SetProperty(ref _DeviceMakeCollection, value); }
        }
        private ObservableCollection<DeviceMake> _DeviceMakeCollection = null;

        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered DeviceMake description
        /// </summary>
        public string SelectedMakeDescription
        {
            get { return _selectedMakeDescription; }
            set { SetProperty(ref _selectedMakeDescription, value); }
        }
        private string _selectedMakeDescription = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidMakeDescription
        {
            get { return _validMakeDescription; }
            set { SetProperty(ref _validMakeDescription, value); }
        }
        private Brush _validMakeDescription = Brushes.Red;

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
                    case "SelectedMakeDescription":
                        ValidMakeDescription = string.IsNullOrEmpty(SelectedMakeDescription) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewDeviceMakeViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseDeviceMakeView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDeviceMakeView()
        {
            _model = new DevicesMakeModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => SelectedMakeDescription);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => SelectedMakeDescription);

            // Load the view data
            await ReadDeviceMakesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedDeviceMake = new DeviceMake();
        }

        /// <summary>
        /// Load all the device makes from the database
        /// </summary>
        private async Task ReadDeviceMakesAsync()
        {
            try
            {
                DeviceMakeCollection = await Task.Run(() => _model.ReadDeviceMakes(false, true));
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
            return !string.IsNullOrWhiteSpace(SelectedMakeDescription);
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
            SelectedDeviceMake.MakeDescription = SelectedMakeDescription.ToUpper();
            SelectedDeviceMake.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedDeviceMake.ModifiedDate = DateTime.Now;
            SelectedDeviceMake.IsActive = MakeState;

            if (SelectedDeviceMake.pkDeviceMakeID == 0)
                result = _model.CreateDeviceMake(SelectedDeviceMake);
            else
                result = _model.UpdateDeviceMake(SelectedDeviceMake);

            if (result)
            {
                InitialiseViewControls();
                await ReadDeviceMakesAsync();
            }
        }

        #endregion

        #endregion
    }
}
