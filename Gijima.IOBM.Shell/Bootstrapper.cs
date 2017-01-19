using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Unity;
using Prism.Modularity;
using Gijima.IOBM.MobileManager;
using Gijima.IOBM.Shell.ViewModels;

namespace Gijima.IOBM.Shell
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return base.CreateModuleCatalog();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<ViewIOBMShell>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }

        protected override void InitializeModules()
        {
            ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;
            moduleCatalog.AddModule(typeof(MobileManagerModule));
            base.InitializeModules();
        }

        #endregion
    }
}
