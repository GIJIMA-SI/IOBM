using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gijima.DataImport.MSOffice
{
    public class ImportExceptionInfo
    {
        public string WorkBookName { get; set; }
        public string SheetName { get; set; }
        public int RowNumber { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public string Message { get; set; }
    }
}
