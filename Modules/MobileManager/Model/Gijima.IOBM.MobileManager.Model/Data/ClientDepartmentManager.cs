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
    
    public partial class ClientDepartmentManager
    {
        public int pkClientDepartmentManagerID { get; set; }
        public int fkClientID { get; set; }
        public Nullable<int> fkDepartmentID { get; set; }
        public Nullable<int> fkLineManagerID { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual Department Department { get; set; }
        public virtual LineManager LineManager { get; set; }
    }
}
