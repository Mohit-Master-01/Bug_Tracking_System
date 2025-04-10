namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IPermissionHelperRepos
    {
        public string HasAccess(string tabName, int roleId);

    }
}
