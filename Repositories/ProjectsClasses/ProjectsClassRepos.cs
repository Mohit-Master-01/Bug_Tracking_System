using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using X.PagedList.Extensions;
using X.PagedList;
using System;
using DocumentFormat.OpenXml.InkML;

namespace Bug_Tracking_System.Repositories.ProjectsClasses
{
    public class ProjectsClassRepos : IProjectsRepos
    {

        private readonly DbBug _dbBug;
        private readonly IEmailSenderRepos _emailSender;

        public ProjectsClassRepos(DbBug bug, IEmailSenderRepos emailSender)
        {
            _dbBug = bug;
            _emailSender = emailSender;
        }

        public async Task<object> AddOrEditProject(Project projects)
        {
            try
            {
                var existingProject = _dbBug.Projects.FirstOrDefault(p => p.ProjectId == projects.ProjectId);
                if (existingProject != null)
                {
                    // Edit Project
                    existingProject.ProjectName = projects.ProjectName;
                    existingProject.Description = projects.Description;
                    existingProject.IsActive = true;
                    existingProject.CreatedDate = DateTime.Now;

                    _dbBug.Projects.Update(existingProject);
                }
                else
                {
                    // Add new project
                    projects.CreatedDate = DateTime.Now;
                    projects.CreatedBy = 2018; // Hardcoded for now
                    projects.IsActive = true;
                    projects.Status = "New";
                    projects.Completion = 0;

                    _dbBug.Projects.Add(projects);
                }

                await _dbBug.SaveChangesAsync();
                return new { success = true, message = "Project saved successfully!" };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        //public async Task<bool> AssignProjectToDeveloper(int projectId, int developerId, int assignedBy)
        //{
        //    var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == developerId);
        //    if(user == null) return false;

        //    var project = await _dbBug.Projects.FindAsync(projectId);
        //    if (project == null) return false;


        //    // Assign project and update project Status
        //    user.ProjectId = projectId;
        //    project.Status = "Assigned";


        //    await _dbBug.SaveChangesAsync();
        //    return true;

        //}

        public async Task<bool> AssignProjectToDeveloper(int projectId, int developerId, int assignedBy)
        {
            // ✅ Check if developer exists
            var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == developerId);
            if (user == null) return false;

            // ✅ Check if project exists
            var project = await _dbBug.Projects.FindAsync(projectId);
            if (project == null) return false;

            // ✅ Check if this assignment already exists (to avoid duplicate assignment)
            var isAlreadyAssigned = await _dbBug.UserProjects
                                        .AnyAsync(up => up.ProjectId == projectId && up.UserId == developerId);
            if (!isAlreadyAssigned)
            {
                // ✅ Add mapping to UserProject table
                _dbBug.UserProjects.Add(new UserProject
                {
                    UserId = developerId,
                    ProjectId = projectId
                });
            }

            // ✅ Optional: Set project status to "Assigned"
            project.Status = "Assigned";

            await _dbBug.SaveChangesAsync();
            return true;
        }



        public Project checkExistance(string ProjectName, int ProjectId)
        {
            return _dbBug.Projects.FirstOrDefault(p => p.ProjectName == ProjectName || p.ProjectId == ProjectId);            
        }

        public async Task<bool> DeleteProject(int projectId)
        {
            var project = await _dbBug.Projects.FindAsync(projectId);
            if(project != null)
            {
                _dbBug.Projects.Remove(project);
                await _dbBug.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Project>> GetAllProjects()
        {
            var project =  await _dbBug.Projects.Include(p => p.CreatedByNavigation).Where(p => p.Status != "New").Where(p => (bool)p.IsActive).OrderByDescending(p => p.CreatedDate).ToListAsync();

            return project;
        }

        public async Task<List<User>> GetDevelopers()
        {
            return await _dbBug.Users
                        .Where(u => u.RoleId == 2) // ✅ Adjust based on your user roles
                        .ToListAsync();
        }

        public async Task<Project> GetProjectById(int projectId)
        {
            var project = await _dbBug.Projects
                .Include(p => p.Users) // Includes assigned developers
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            return project;
        }

        public async Task<List<Project>> GetProjectByUser(int userId)
        {
            return await _dbBug.UserProjects
                        .Where(up => up.UserId == userId)
                        .Select(up => up.Project)
                        .Distinct()
                        .ToListAsync();
        }



        //public async Task<Project> GetProjectById(int projectId)
        //{
        //    var project = await _dbBug.Projects                    
        //            .FirstOrDefaultAsync(p => p.ProjectId == projectId);


        //        return new Project
        //        {
        //            ProjectId = project.ProjectId,
        //            ProjectName = project.ProjectName,
        //            Description = project.Description,

        //            CreatedDate = project.CreatedDate,
        //            Status = project.Status,
        //            Completion = project.Completion,

        //        };


        //}

        public async Task<List<Project>> GetUnassignedProjects()
        {
            var project = await _dbBug.Projects.Include(p => p.CreatedByNavigation).Where(p => (bool)p.IsActive).Where(p => p.Status == "New").OrderByDescending(p => p.CreatedDate).ToListAsync();

            return project;
        }

        public async Task<bool> IsProjectExist(string projectname, int? projectId = null)
        {
            return await _dbBug.Projects
                .AnyAsync(p => p.ProjectName == projectname && (!projectId.HasValue || p.ProjectId != projectId.Value));
        }

        public async Task<object> UpdateStatus(int projectId, bool status)
        {
            var project = await _dbBug.Projects.FindAsync(projectId);
            if(project != null)
            {
                project.IsActive = status;
                await _dbBug.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
