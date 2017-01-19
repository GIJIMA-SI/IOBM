namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class Suburb
    {
        /// <summary>
        /// Returns the Suburb name and postal code combined
        /// </summary>
        /// <returns>Suburb name and postal code</returns>
        public string SuburbDescription
        {
            get { return pkSuburbID > 0 ? string.Format("{0} ({1})", SuburbName, PostalCode) : SuburbName; }
        }
    }
}
