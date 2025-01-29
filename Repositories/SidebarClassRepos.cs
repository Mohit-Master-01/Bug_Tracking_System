using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories
{
    public class SidebarClassRepos : ISidebarRepos
    {
        private readonly DbBug _dbBug;

        public SidebarClassRepos(DbBug dbBug)
        {
            _dbBug = dbBug;            
        }

        public async Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId)
        {
            var tabs = await (from t in _dbBug.TblTabs
                              join p in _dbBug.Permissions on t.TabId equals p.Tabid
                              where p.Roleid == roleId orderby t.SortOrder
                              select new SidebarModel
                              {
                                  TabId = t.TabId,
                                  TabName = t.TabName,
                                  ParentId = t.ParentId,
                                  TabUrl = t.TabUrl,
                                  IconPath = t.IconPath,
                                  //IsActive = t.IsActive

                              }).ToListAsync();
                              
            //Group the tabs into a hierarchical structure (parent-child)
                var tabHierarchy = tabs.Where(tab => tab.ParentId == null).Select(tab => new SidebarModel
            {
                TabId = tab.TabId,
                TabName = tab.TabName,
                ParentId = tab.ParentId,
                TabUrl = tab.TabUrl,
                IconPath = tab.IconPath,
                //IsActive = tab.IsActive,
                SubTabs = tabs.Where(sub => sub.ParentId == tab.TabId).ToList()
            }).ToList();
                 
            return tabHierarchy;
        }
    }
}
