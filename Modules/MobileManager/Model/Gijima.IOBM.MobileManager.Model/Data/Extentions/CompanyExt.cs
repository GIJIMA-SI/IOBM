using Gijima.IOBM.MobileManager.Model.Models;

namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class Company
    {
        /// <summary>
        /// Returns the number of employees linked to the company
        /// </summary>
        /// <returns>Employee Count</returns>
        public int EmployeeCount
        {
            get { return new ClientModel().EmployeeCountForCompany(pkCompanyID); }
        }
    }
}
