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
    
    public partial class AuditLog
    {
        public int pkAuditLogID { get; set; }
        public string AuditGroup { get; set; }
        public string AuditDescription { get; set; }
        public string AuditComment { get; set; }
        public Nullable<int> EntityID { get; set; }
        public string ChangedValue { get; set; }
        public Nullable<System.DateTime> AuditDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}