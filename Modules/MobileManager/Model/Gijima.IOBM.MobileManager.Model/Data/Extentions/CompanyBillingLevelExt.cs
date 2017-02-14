using Gijima.IOBM.MobileManager.Common.Structs;

namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class CompanyBillingLevel
    {
        /// <summary>
        /// Returns the property name from the BillingLevelType enum.
        /// </summary>
        /// <returns></returns>
        public string TypeDescription
        {
            get { return ((BillingLevelType)enBillingLevelType).ToString(); }
        }
    }
}
