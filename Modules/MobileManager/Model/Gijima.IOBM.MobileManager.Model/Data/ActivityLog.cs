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
    
    public partial class ActivityLog
    {
        public int pkActivityLogID { get; set; }
        public string ActivityGroup { get; set; }
        public string ActivityDescription { get; set; }
        public string ActivityComment { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public System.DateTime ActivityDate { get; set; }
    }
}
