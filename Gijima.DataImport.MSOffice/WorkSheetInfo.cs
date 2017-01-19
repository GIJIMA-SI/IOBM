using System.Collections.Generic;

namespace Gijima.DataImport.MSOffice
{
    public class WorkSheetInfo
    {
        public string WorkBookName { get; set; }
        public string SheetName { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public IEnumerable<string> ColumnNames { get; set; }
        public int ImportDuration { get; set; }
    }
}
