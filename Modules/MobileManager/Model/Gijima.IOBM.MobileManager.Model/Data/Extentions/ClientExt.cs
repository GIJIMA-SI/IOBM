namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class Client
    {
        /// <summary>
        /// Returns the client detail as the search result
        /// </summary>
        /// <returns>ClientName, CellNumber, State</returns>
        public string SearchResult
        {
            get { return string.Format("{0}, {1}, {2}", ClientName, PrimaryCellNumber, IsActive ? "Active" : "In-Active"); }
        }
    }
}
