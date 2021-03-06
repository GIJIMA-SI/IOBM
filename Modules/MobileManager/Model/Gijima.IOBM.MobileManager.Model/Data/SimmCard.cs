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
    
    public partial class SimmCard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SimmCard()
        {
            this.Devices = new HashSet<Device>();
        }
    
        public int pkSimmCardID { get; set; }
        public string statusname { get; set; }
        public Nullable<int> fkContractID { get; set; }
        public Nullable<int> fkStatusID { get; set; }
        public string CellNumber { get; set; }
        public string CardNumber { get; set; }
        public string PinNumber { get; set; }
        public string PUKNumber { get; set; }
        public Nullable<System.DateTime> ReceiveDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Device> Devices { get; set; }
        public virtual Status Status { get; set; }
        public virtual Contract Contract { get; set; }
    }
}
