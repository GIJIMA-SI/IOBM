using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Views;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Gijima.IOBM.MobileManager
{
    public class MobileManagerModule : IModule
    {
        private IUnityContainer _container = null;
        private MobileManagerController _controller = null;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        public MobileManagerModule(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule members

        public void Initialize()
        {
            /// Create a controller for the module
            /// We are using the unity container to get a new MobileManagerController
            _controller = _container.Resolve<MobileManagerController>();

            /// Get a reference to the RegionManager from the unity container
            var regionManager = _container.Resolve<IRegionManager>();

            /// Register the relevant view with a region
            regionManager.RegisterViewWithRegion(RegionaNames.MainRegion, typeof(ViewMobileManager));
        }

        #endregion
    }
}
