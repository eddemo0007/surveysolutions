using OfficeOpenXml;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class ExcelExportFile : ExportFile
    {
        public override byte[] GetFileBytes(ReportView report)
        {
            var headers = report.Headers;
            var data = report.Data;

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add(report.Name ?? "Data");

                // setting headers
                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                {
                    var cell = worksheet.Cells[1, columnIndex + 1];
                    cell.Value = headers[columnIndex];
                    cell.Style.Font.Bold = true;
                }

                // setting table data
                for (int rowIndex = 0; rowIndex < data.Length; rowIndex++)
                {
                    var rowData = data[rowIndex];

                    for (int columnIndex = 0; columnIndex < rowData.Length; columnIndex++)
                    {
                        var cell = worksheet.Cells[rowIndex + 2, columnIndex + 1];
                        var value = rowData[columnIndex];

                        SetCellValue(value, cell);
                    }
                }

                // setting table totals if exists
                if (report.Totals != null)
                {
                    var rowIndex = 1 /* header */ + data.Length /* data rows count*/ + 1 /* total row */; 
                    for (int columnIndex = 0; columnIndex < report.Totals.Length; columnIndex++)
                    {
                        var cell = worksheet.Cells[rowIndex, columnIndex + 1];
                        cell.Style.Font.Bold = true;
                        var value = report.Totals[columnIndex];

                        SetCellValue(value, cell);
                    }
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                {
                    worksheet.Column(columnIndex + 1).AutoFit();
                }

                return excelPackage.GetAsByteArray();
            }
        }

        private static void SetCellValue(object value, ExcelRange cell)
        {
            switch (value)
            {
                case long longValue:
                    cell.Value = longValue;
                    break;
                case int intValue:
                    cell.Value = intValue;
                    break;
                case double doubleValue:
                    cell.Value = doubleValue;
                    break;
                case float floatValue:
                    cell.Value = floatValue;
                    break;
                case decimal decimalValue:
                    cell.Value = decimalValue;
                    break;
                default:
                    cell.Value = value?.ToString() ?? "";
                    break;
            }
        }

        public override string MimeType => @"application/vnd.oasis.opendocument.spreadsheet";
        public override string FileExtension => @".xlsx";
    }
}
