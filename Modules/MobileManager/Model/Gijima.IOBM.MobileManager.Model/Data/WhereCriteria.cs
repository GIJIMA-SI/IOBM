using Gijima.IOBM.MobileManager.Model.Data;

namespace Gijima.IOBM.MobileManager.Common.Structs
{
    public class WhereCriteria
    {
        /// <summary>
        /// The AdvancedSearchField row
        /// </summary>
        public AdvancedSearchField SearchField
        {
            get { return _searchField; }
            set { _searchField = value; }
        }
        private AdvancedSearchField _searchField;
        /// <summary>
        /// The operator value eg."PreFix"
        /// </summary>
        public string OperatorValue
        {
            get { return _operatorValue; }
            set { _operatorValue = value; }
        }
        private string _operatorValue;
        /// <summary>
        /// The actual value the user entered
        /// </summary>
        public string SearchValue
        {
            get { return _searchValue; }
            set { _searchValue = value; }
        }
        private string _searchValue;

        /// <summary>
        /// Constructor
        /// </summary>
        public WhereCriteria()
        {

        }
    }
}
