using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories
{
    public class PermissionHelperClassRepos : IPermissionHelperRepos
    {
        private readonly DbBug _bug;
            
        public PermissionHelperClassRepos(DbBug bug)
        {
            _bug = bug;            
        }

        public string HasAccess(string tabName, int roleId)
        {
            var currentTabId = _bug.TblTabs.Where(x => x.TabName == tabName).Select(y => y.TabId).FirstOrDefault();
            var currentTabPermissions = GetTabByRole(roleId, currentTabId);
            return currentTabPermissions?.PermissionType;
        }
         
        public SidebarModel GetTabByRole(int roleId, int currentTabId)
        {
            return _bug.Permissions.Where(x => x.Roleid == roleId && x.Isactive == true && x.Tabid == currentTabId).Select(y => new SidebarModel
            {
                TabId = y.Tabid,
                IsActive = y.Isactive,
                PermissionType = y.PermissionType
            }).FirstOrDefault();
        }
    }
}
