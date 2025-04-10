namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IImportRepos
    {
        byte[] GenerateSampleBugExcel(bool bug);
        
     
        Task<byte[]> BugsExcelImport(IFormFile file);
        


    }
}
