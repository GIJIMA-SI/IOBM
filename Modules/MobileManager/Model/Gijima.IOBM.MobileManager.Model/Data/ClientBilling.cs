//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gijima.IOBM.MobileManager.Model.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class ClientBilling
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientBilling()
        {
            this.Clients = new HashSet<Client>();
        }
    
        public int pkClientBillingID { get; set; }
        public Nullable<int> fkCompanyBillingLevelID { get; set; }
        public bool IsBillable { get; set; }
        public bool IsSplitBilling { get; set; }
        public bool SplitBillingException { get; set; }
        public Nullable<decimal> WDPAllowance { get; set; }
        public decimal VoiceAllowance { get; set; }
        public Nullable<decimal> SPLimit { get; set; }
        public Nullable<decimal> AllowanceLimit { get; set; }
        public bool InternationalDailing { get; set; }
        public bool PermanentIntDailing { get; set; }
        public bool InternationalRoaming { get; set; }
        public bool PermanentIntRoaming { get; set; }
        public string CountryVisiting { get; set; }
        public Nullable<System.DateTime> RoamingFromDate { get; set; }
        public Nullable<System.DateTime> RoamingToDate { get; set; }
        public Nullable<System.DateTime> StopBillingFromDate { get; set; }
        public Nullable<System.DateTime> StopBillingToDate { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    
        public virtual CompanyBillingLevel CompanyBillingLevel { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Client> Clients { get; set; }
    }
}
