using Bug_Tracking_System.Repositories.Interfaces;
using ClosedXML.Excel;
using System.Data;
using System.IO;
using System.Linq;

namespace Bug_Tracking_System.Repositories
{
    public class ExportClassRepos : IExportRepos
    {
        public byte[] ExportToExcel(DataTable dataTable, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).InsertTable(dataTable);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
