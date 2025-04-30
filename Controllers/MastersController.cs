using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Controllers
{
    public class MastersController : BaseController
    {
        private readonly DbBug _bug;
        private readonly IPermissionHelperRepos _permission;

        public MastersController(DbBug bug, IPermissionHelperRepos permission ,ISidebarRepos sidebar) : base(sidebar) 
        {
            _bug = bug;
            _permission = permission; 
        }

        //Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }

        [HttpGet]
        public IActionResult Roles()
        {
            ViewBag.PageTitle = "Roles";
            ViewBag.Breadcrumb = "Manage Roles";


            string permissionType = GetUserPermission("Roles");

            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                var roles = _bug.Roles.Where(x =>  x.RoleId != 4 && x.IsDelete == false).ToList();
                return View(roles);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public IActionResult SaveRoles([FromBody] Role role)
        {
            if (role.RoleId == 0)
            {
                role.IsDelete = false; // Ensure it's 0 when inserting new role too
                _bug.Roles.Add(role);
            }
            else
            {
                var existingRole = _bug.Roles.FirstOrDefault(r => r.RoleId == role.RoleId);
                if (existingRole != null)
                {
                    existingRole.RoleName = role.RoleName;
                    existingRole.IsActive = role.IsActive;
                    existingRole.IsDelete = false; // Explicitly set IsDelete to 0 when updating
                    _bug.Roles.Update(existingRole);
                }
            }
            _bug.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult DeleteRoles(int id)
        {
            var roles = _bug.Roles.Where(x => x.RoleId == id).FirstOrDefault();

            roles.IsDelete = true;
            roles.IsActive = false;
            _bug.Update(roles);
            _bug.SaveChanges();
            return RedirectToAction("Roles");
        }

        [HttpGet]
        public IActionResult Access()
        {
            ViewBag.PageTitle = "Permissions";
            ViewBag.Breadcrumb = "Manage Access";

            string permissionType = GetUserPermission("Manage Access");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                var tabs = _bug.TblTabs.Where(x => x.IsActive == true).ToList();
                var roles = _bug.Roles.Where(x => x.IsActive == true && x.RoleId != 4).ToList();

                ViewBag.Tabs = tabs;
                ViewBag.Roles = roles;
                return View(new List<Permission>());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public JsonResult GetRolePermissions(int roleId)
        {
            var permissions = _bug.Permissions
                .Where(p => p.Roleid == roleId)
                .Select(p => new {
                    p.Permissionid,
                    p.Tabid,
                    TabName = _bug.TblTabs.Where(t => t.TabId == p.Tabid).Select(t => t.TabName).FirstOrDefault(),
                    p.PermissionType,
                    p.Isactive
                }).ToList();
            return Json(permissions);
        }

        [HttpPost]
        public IActionResult SavePermission([FromBody] Permission permission)
        {
            string msg = "";
            bool isSuccess = true;

            var alreadyExists = _bug.Permissions.Any(x => x.Tabid == permission.Tabid && x.Roleid == permission.Roleid && x.Permissionid != permission.Permissionid);

            if (alreadyExists && permission.Permissionid == 0)
            {
                isSuccess = false;
                msg = "This Permission already exists. You can edit it from the list.";
            }
            else if (permission.Permissionid == 0)
            {
                _bug.Permissions.Add(permission);
                msg = "Permission Added Successfully";
            }
            else
            {
                var existingPermission = _bug.Permissions.FirstOrDefault(x => x.Permissionid == permission.Permissionid);
                if (existingPermission != null)
                {
                    existingPermission.Tabid = permission.Tabid;
                    existingPermission.PermissionType = permission.PermissionType;
                    existingPermission.Isactive = permission.Isactive;
                    msg = "Permission Updated Successfully";
                }
                else
                {
                    isSuccess = false;
                    msg = "Permission not found.";
                }
            }

            _bug.SaveChanges();
            return Ok(new { success = isSuccess, message = msg });
        }


        //[HttpPost]
        //public IActionResult SavePermission([FromBody] Permission permission)
        //{
        //    string msg = "";
        //    bool isSuccess = true;
        //    bool alreadyExists = _bug.Permissions.Any(x => x.Tabid == permission.Tabid && x.Roleid == permission.Roleid);
        //    if (alreadyExists && permission.Permissionid == 0)
        //    {
        //        isSuccess = false;
        //        msg = "This Permission already exists you can edit it from the list";
        //    }
        //    else if (permission.Permissionid == 0)
        //    {
        //        _bug.Permissions.Add(permission);
        //        msg = "Permission Added Successfully";
        //    }
        //    else
        //    {
        //        _bug.Permissions.Update(permission);
        //        msg = "Permission Updated Successfully";
        //    }
        //    _bug.SaveChanges();
        //    return Ok(new { success = isSuccess, message = msg });
        //}

        [HttpGet]
        public IActionResult DeletePermission(int id)
        {
            var permission = _bug.Permissions.Where(x => x.Permissionid == id).FirstOrDefault();
            _bug.Permissions.Remove(permission);
            _bug.SaveChanges();
            return RedirectToAction("Access");
        }
    }
}
