using Gijima.IOBM.MobileManager.Views;
using Prism.Events;
using Prism.Mvvm;
using System.Windows;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataImportIntViewModel : BindableBase
    {
        #region Properties & Attributes

        private IEventAggregator _eventAggregator;

        #region Commands
        #endregion

        #region Properties       

        /// <summary>
        /// Show the system data is import view
        /// </summary>
        public bool SystemData
        {
            get { return _systemData; }
            set
            {
                SetProperty(ref _systemData, value);
                if (value)
                    ImportContent = new ViewDataImport();
            }
        }
        private bool _systemData;

        /// <summary>
        /// Show the external data import view
        /// </summary>
        public bool ExternalData
        {
            get { return _externalData; }
            set
            {
                SetProperty(ref _externalData, value);
                if (value)
                    ImportContent = new ViewDataImportExt();
            }
        }
        private bool _externalData;

        /// <summary>
        /// Show the system data data import view
        /// </summary>
        public object ImportContent
        {
            get { return _importContent; }
            set { SetProperty(ref _importContent, value); }
        }
        private object _importContent;
        
        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        #endregion

        #region Input Validation  

        #endregion

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDataImportIntViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            SystemData = true;
        }

        #endregion
    }
}
