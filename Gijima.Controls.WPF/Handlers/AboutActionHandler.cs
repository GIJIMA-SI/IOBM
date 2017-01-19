using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.Util;

namespace Gijima.Controls.WPF
{
    [ActionHandler("Gijima.Controls.WPF.AboutAction")]
    public class AboutActionHandler : IActionHandler
    {
        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nexUpdate)
        {
            // return true/false to enable/disable this action
            return true;
        }
        
        public void Execute(IDataContext context,DelegateExecute nextExecute)
        {
            MessageBox.ShowInfo("About information for the Gijima.Controls.WPF plugin goes here.");
        }        
    }
}