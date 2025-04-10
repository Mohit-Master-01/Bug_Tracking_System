using System.Data;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IExportRepos
    {
        byte[] ExportToExcel(DataTable dataTable, string sheetName);

    }
}
