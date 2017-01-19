using System;
using System.Windows;
using System.Windows.Threading;

namespace Gijima.IOBM.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Was creating startup exceptions when deployed
                base.OnStartup(e);

                // Handle unhandled exceptions
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

                var bootStrapper = new Bootstrapper();
                bootStrapper.Run();
            }
            catch (Exception ex)
            {

            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
        }
    }
}
