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
    
    public partial class ExternalBillingData
    {
        public int pkExternalBillingDataID { get; set; }
        public string TableName { get; set; }
        public string SheetName { get; set; }
        public string BillingPeriod { get; set; }
        public bool PropertyValidationPassed { get; set; }
        public bool DataValidationPassed { get; set; }
        public string DataFileLocation { get; set; }
        public string DataFileName { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime DateModified { get; set; }
    }
}
