using testAPI.api.application.ServiceInterfaces;
using ClosedXML.Excel;

namespace testAPI.api.application.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public byte[] ExportToExcel<T>(List<T> data, List<string> headers, string sheetName = "Sheet1")
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.RightToLeft = true;

            headers ??= new List<string>();

            for (int i = 0; i < headers.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                cell.Style.Font.FontColor = XLColor.DarkBlue;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            if (data != null && data.Any())
            {
                var properties = typeof(T).GetProperties();

                for (int row = 0; row < data.Count; row++)
                {
                    if (data[row] == null) continue;

                    for (int col = 0; col < properties.Length && col < headers.Count; col++)
                    {
                        var value = properties[col].GetValue(data[row]);
                        var cell = worksheet.Cell(row + 2, col + 1);
                        cell.Value = value?.ToString() ?? "";
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                }
            }

            worksheet.Columns().AdjustToContents();
            foreach (var col in worksheet.ColumnsUsed())
            {
                if (col.Width < 15)
                    col.Width = 15;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
