namespace Bug_Tracking_System.Models
{
    public class SidebarModel
    {
        public int TabId { get; set; }
        public string TabName { get; set; }
        public string TabUrl { get; set; }
        public string? IconPath { get; set; }

        public string PermissionType { get; set; }

        public int? ParentId { get; set; }
        public bool IsActive { get; set; }
        public List<SidebarModel> SubTabs { get; set; } // Sub-tabs if any  
    }
}
