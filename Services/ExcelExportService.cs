using System.Data;
using ClosedXML.Excel;

namespace PredatorWeb.Services;

public class ExcelExportService
{
    public byte[] ExportToExcel(DataTable dataTable, string sheetName = "Datos")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Insertar el DataTable en la hoja (con encabezados)
        worksheet.Cell(1, 1).InsertTable(dataTable);

        // Autoajustar columnas
        worksheet.Columns().AdjustToContents();

        // Formatear encabezados
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Convertir a bytes
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
