using Gijima.IOBM.MobileManager.Common.Structs;

namespace Gijima.IOBM.Infrastructure.Structs
{
    public static class MobileManagerEnvironment
    {
        #region Properties & Attributes

        /// <summary>
        /// The selected process menu option.
        /// </summary>
        public static ProcessMenuOption SelectedProcessMenu { get; set; }

        /// <summary>
        /// The selected system tools menu option.
        /// </summary>
        public static ToolsMenuOption SelectedToolsMenu { get; set; }

        /// <summary>
        /// The selected configuration menu option.
        /// </summary>
        public static ConfigMenuOption SelectedConfigMenu { get; set; }

        /// <summary>
        /// The selected client ID.
        /// </summary>
        public static int ClientID { get; set; }

        /// <summary>
        /// The selected contract ID.
        /// </summary>
        public static int ClientContractID { get; set; }

        /// <summary>
        /// The selected client company ID.
        /// </summary>
        public static int ClientCompanyID { get; set; }

        /// <summary>
        /// The selected client primary cell number.
        /// </summary>
        public static string ClientPrimaryCell { get; set; }

        /// <summary>
        /// The current billing period.
        /// </summary>
        public static string BillingPeriod { get; set; }
        
        /// <summary>
        /// Indicate if the billing period is open.
        /// </summary>
        public static bool IsBillingPeriodOpen { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        static MobileManagerEnvironment()
        {
        }

        #endregion
    }
}
