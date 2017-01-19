using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gijima.DataImport.MSOffice
{
    public class MSOfficeHelper
    {
        /// <summary>
        /// Retrieve all the workbook sheet from the specfied excel workbook
        /// </summary>
        /// <param name="workBookName">The Excel file to retrieve the sheet from.</param>
        /// <returns>List of workbook sheets</returns>
        public IEnumerable<Sheet> ReadSheetsFromExcel(string workBookName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBookName, false))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                Workbook workbook = workbookPart.Workbook;
                List<Sheet> sheets = new List<Sheet>();

                foreach (Sheet sheet in workbook.Sheets)
                {
                    sheets.Add(sheet);
                }

                return sheets;
            }
        }

        /// <summary>
        /// Retrieve the sheet data for all the sheets in the specified workbook
        /// </summary>
        /// <param name="workBookName">The Excel file to retrieve the sheet from.</param>
        /// <returns>List of workbook sheets</returns>
        public IEnumerable<WorkSheetInfo> ReadWorkBookInfoFromExcel(string workBookName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBookName, false))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                Workbook workbook = workbookPart.Workbook;
                List<WorkSheetInfo> workSheets = new List<WorkSheetInfo>();
                WorkSheetInfo sheetInfo = new WorkSheetInfo();

                foreach (Sheet sheet in workbook.Sheets)
                {
                    sheetInfo = ReadSheetInfoFromExcel(workBookName, sheet);
                    sheetInfo.SheetName = sheet.Name;
                    workSheets.Add(sheetInfo);
                }

                return workSheets;
            }
        }

        /// <summary>
        /// Retrieve the sheet info for the specified sheets in the specified workbook
        /// (ColCount, RowCount, ColNames)
        /// </summary>
        /// <param name="workBook">The Excel file to retrieve the sheet from.</param>
        /// <param name="workSheet">The workbook sheet to read data from.</param>
        /// <returns>WorkSheetInfo</returns>
        private WorkSheetInfo ReadSheetInfoFromExcel(string workBook, Sheet workSheet)
        {
            WorkSheetInfo sheetInfo = new WorkSheetInfo();
            List<string> columnNames = new List<string>();
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBook, false))
                {
                    // Get the work book
                    WorkbookPart workbookPart = document.WorkbookPart;
                    Workbook workbook = workbookPart.Workbook;

                    // Get the sheet - for the name
                    WorksheetPart worksheetPart = workbookPart.GetPartById(workSheet.Id) as WorksheetPart;

                    // Get the workseet and its data - for the sheet
                    Worksheet worksheet = worksheetPart.Worksheet;
                    SheetData sheetData = worksheet.Elements<SheetData>().First();

                    // Get all the sheet info
                    if (sheetData.Count() > 0)
                    {
                        IEnumerable<Row> rows = sheetData.Elements<Row>();

                        foreach (Cell cell in rows.ElementAt(0))
                        {
                            columnNames.Add(GetCellValue(document, cell));
                        }

                        sheetInfo.WorkBookName = workBook;
                        sheetInfo.ColumnCount = worksheet.Elements<Columns>().First().Count();
                        sheetInfo.RowCount = sheetData.Elements<Row>().Count();
                        sheetInfo.ImportDuration = (int)Math.Round(duration.TotalSeconds, 0);
                        sheetInfo.ColumnNames = columnNames.ToList();
                    }
                }

                return sheetInfo;
            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException.Message);
            }
        }

        /// <summary>
        /// Retrieve the sheet data for the specified sheets in the specified workbook
        /// </summary>
        /// <param name="workBook">The Excel file to retrieve the sheet from.</param>
        /// <param name="workSheet">The workbook sheet to read data from.</param>
        /// <returns>DataTable</returns>
        public DataTable ReadSheetDataIntoDataTable(string workBook, string sheetName)
        {
            DataTable dt = new DataTable();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBook, false))
            {
                // Get the work book
                WorkbookPart workbookPart = document.WorkbookPart;
                Workbook workbook = workbookPart.Workbook;

                // Get the sheet - for the name
                Sheet sheet = workbook.Sheets.OfType<Sheet>().First((s) => s.Name == sheetName);
                WorksheetPart worksheetPart = workbookPart.GetPartById(sheet.Id) as WorksheetPart;

                // Get the workseet and its data - for the sheet
                Worksheet worksheet = worksheetPart.Worksheet;
                SheetData sheetData = worksheet.Elements<SheetData>().First();

                if (sheetData.Count() > 0)
                {
                    // Get the used dimensions of the worksheet
                    int columnCount = worksheet.Elements<Columns>().First().Count();
                    IEnumerable<Row> rows = sheetData.Elements<Row>();

                    // Create the data table from the sheet columns
                    foreach (Cell cell in rows.ElementAt(0))
                    {
                        dt.Columns.Add(GetCellValue(document, cell));
                    }

                    // Read the sheet data into the data table
                    foreach (Row row in rows)
                    {
                        DataRow dataRow = dt.NewRow();
                        int colIdx = 0;

                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));
                            cellColumnIndex--;

                            if (colIdx < cellColumnIndex)
                            {
                                do
                                {
                                    dataRow[colIdx] = "";
                                    colIdx++;
                                }
                                while (colIdx < cellColumnIndex);
                            }

                            dataRow[colIdx] = GetCellValue(document, cell);
                            colIdx++;
                        }

                        dt.Rows.Add(dataRow);
                    }

                    // Remove the header row
                    dt.Rows.RemoveAt(0);
                }
            }

            return dt;
        }

        /// <summary>
        /// Get the value of the specified cell
        /// </summary>
        /// <param name="document">The spreadsheet containing the cell.</param>
        /// <param name="cell">The cell to get value of.</param>
        /// <returns>Cell Value</returns>
        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell.CellValue == null)
                return string.Empty;

            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            else
                return value;
        }

        /// <summary>
        /// Given a cell name, parses the specified cell to get the column name.
        /// </summary>
        /// <param name="cellReference">Address of the cell (ie. B2)</param>
        /// <returns>Column Name (ie. B)</returns>
        public string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        }

        /// <summary>
        /// Given just the column name (no row index), it will return the zero based column index.
        /// Note: This method will only handle columns with a length of up to two (ie. A to Z and AA to ZZ). 
        /// A length of three can be implemented when needed.
        /// </summary>
        /// <param name="columnName">Column Name (ie. A or AB)</param>
        /// <returns>Zero based index if the conversion was successful; otherwise null</returns>
        public int? GetColumnIndexFromName(string columnName)
        {
            //return columnIndex;
            string name = columnName;
            int number = 0;
            int pow = 1;

            for (int i = name.Length - 1; i >= 0; i--)
            {
                number += (name[i] - 'A' + 1) * pow;
                pow *= 26;
            }
            return number;
        }

        /// <summary>
        /// Export the data in the data table to excel
        /// </summary>
        /// <param name="dt">The data table to export.</param>
        /// <param name="destination">The excel file to export to.</param>
        public void ExportDataTableToExcel(DataTable dt, string destination)
        {
            using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();

                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                sheetPart.Worksheet = new Worksheet(sheetData);

                Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = dt.TableName };
                sheets.Append(sheet);
                Row headerRow = new Row();
                List<String> columns = new List<string>();

                foreach (DataColumn column in dt.Columns)
                {
                    columns.Add(column.ColumnName);
                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(column.ColumnName);
                    headerRow.AppendChild(cell);
                }

                sheetData.AppendChild(headerRow);

                foreach (DataRow dsrow in dt.Rows)
                {
                    Row newRow = new Row();

                    foreach (String col in columns)
                    {
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(dsrow[col].ToString()); 
                        newRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(newRow);
                }
            }
        }
    }
}
