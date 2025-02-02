using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using X.PagedList.Extensions;
using X.PagedList;

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
                    //Edit Project
                    existingProject.ProjectName = projects.ProjectName;
                    existingProject.Description = projects.Description;
                    existingProject.IsActive = projects.IsActive;
                    _dbBug.Projects.Update(existingProject);
                }
                else
                {
                    //Add new project
                    projects.CreatedDate = DateTime.Now;
                    projects.CreatedBy = 2018;
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

        public async Task<IPagedList<Project>> GetAllProjects(int pageNumber, int pageSize)
        {
            var project =  await _dbBug.Projects.Include(p => p.CreatedByNavigation).Where(p => (bool)p.IsActive).OrderByDescending(p => p.CreatedDate).ToListAsync();

            return project.ToPagedList(pageNumber, pageSize);
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
