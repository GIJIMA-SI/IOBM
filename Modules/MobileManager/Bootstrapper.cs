using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Unity;
using Gijima.IOBM.MobileManager.Views;

namespace Gijima.IOBM.MobileManager
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            //Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterTypeForNavigation<ViewMobileAdmin>("ViewMobileAdmin");
        }
    }

    public static class UnityExtentions
    {
        public static void RegisterTypeForNavigation<T>(this IUnityContainer container, string name)
        {
            container.RegisterType(typeof(object), typeof(T), name);
        }
    }
}
