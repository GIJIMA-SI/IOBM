using Gijima.IOBM.MobileManager.Common.Structs;

namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class ValidationRule
    {
        /// <summary>
        /// Returns a string representing the entity name.
        /// </summary>
        /// <returns>Entity Name</returns>
        public string EntityName { get; set; }

        /// <summary>
        /// Returns a string representing the data name.
        /// </summary>
        /// <returns>Data Name</returns>
        public string RuleDataName { get; set; }

        /// <summary>
        /// Returns a string representing the data type.
        /// </summary>
        /// <returns>Data Type</returns>
        public string RuleDataTypeName { get; set; }
    }
}
