using Gijima.IOBM.Shell.ViewModels;
using Prism.Events;
using System;
using System.Windows;

namespace Gijima.IOBM.Shell
{
    /// <summary>
    /// Interaction logic for ViewIOBMShell.xaml
    /// </summary>
    public partial class ViewIOBMShell : Window
    {
        public ViewIOBMShell(IEventAggregator eventAggreagator)
        {
            ViewIOBMShellViewModel vm = new ViewIOBMShellViewModel(eventAggreagator);
            DataContext = vm;

            if (vm.CloseAction == null)
                vm.CloseAction = new Action(Close);

            InitializeComponent();
        }
    }
}
