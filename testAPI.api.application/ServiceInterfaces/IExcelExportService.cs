namespace testAPI.api.application.ServiceInterfaces
{
    public interface IExcelExportService
    {
        byte[] ExportToExcel<T>(List<T> data, List<string> headers, string sheetName = "Sheet1");
    }
}
