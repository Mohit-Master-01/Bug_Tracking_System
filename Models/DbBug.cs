using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Models;

public partial class DbBug : DbContext
{
    public DbBug()
    {
    }

    public DbBug(DbContextOptions<DbBug> options)
        : base(options)
    {
    }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Bug> Bugs { get; set; }

    public virtual DbSet<BugStatus> BugStatuses { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TaskAssignment> TaskAssignments { get; set; }

    public virtual DbSet<TblTab> TblTabs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProject> UserProjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Bug_Tracking_System;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__Attachme__442C64BE80BE9043");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.UploadedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Bug).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.BugId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachment_Bug");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E54864816F63166");

            entity.Property(e => e.Action).HasMaxLength(500);
            entity.Property(e => e.ActionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModuleName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Bug>(entity =>
        {
            entity.HasKey(e => e.BugId).HasName("PK__Bugs__7812D762E9DC0C02");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);


            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Bugs)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bugs_CreatedBy");

            entity.HasOne(d => d.Project).WithMany(p => p.Bugs)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Bugs_Project");

            entity.HasOne(d => d.Status).WithMany(p => p.Bugs)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bugs_Status");
        });

        modelBuilder.Entity<BugStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__BugStatu__C8EE2063FA95DD67");

            entity.ToTable("BugStatus");

            entity.HasIndex(e => e.StatusName, "UQ__BugStatu__05E7698AAAD5C4AF").IsUnique();

            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12A1B9592C");

            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.ModuleType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NotificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RelatedId).HasColumnName("RelatedID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Permissionid).HasName("PK__Permissi__D8200EA449E506EF");

            entity.ToTable("Permission");

            entity.Property(e => e.Permissionid).HasColumnName("permissionid");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.PermissionType).HasMaxLength(40);
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Tabid).HasColumnName("tabid");

            entity.HasOne(d => d.Role).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permissio__rolei__17036CC0");

            entity.HasOne(d => d.Tab).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.Tabid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permissio__tabid__17F790F9");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Projects__761ABEF0326FB046");

            entity.HasIndex(e => e.ProjectName, "UQ__Projects__BCBE781C2D5F68A1").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProjectName).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(15);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projects_CreatedBy");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AC3AE7423");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160FF94F8FF").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__TaskAssi__7C6949B13A042DB9");

            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CompletionDate).HasColumnType("datetime");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.TaskAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_AssignedBy");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TaskAssignmentAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_AssignedTo");

            entity.HasOne(d => d.Bug).WithMany(p => p.TaskAssignments)
                .HasForeignKey(d => d.BugId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Task_Bug");

            entity.HasOne(d => d.Project).WithMany(p => p.TaskAssignments)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_TaskAssignments_Project");

            entity.HasOne(d => d.ProjectManager).WithMany(p => p.TaskAssignmentProjectManagers)
                .HasForeignKey(d => d.ProjectManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_ProjectManager");
        });

        modelBuilder.Entity<TblTab>(entity =>
        {
            entity.HasKey(e => e.TabId).HasName("PK__tblTabs__80E37C1890796E7E");

            entity.ToTable("tblTabs");

            entity.Property(e => e.IconPath).IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasDefaultValue(1);
            entity.Property(e => e.TabName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TabUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C808D0975");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053446426178").IsUnique();

            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime")
                .IsRequired(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.GitHubProfile).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsGoogleAccount).HasDefaultValue(false);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.LinkedInProfile).HasMaxLength(255);
            entity.Property(e => e.Otp)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.OtpExpiry).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.ProfileImage).HasMaxLength(1000);
            entity.Property(e => e.Skills).HasMaxLength(1000);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.Bug).WithMany(p => p.Users)
                .HasForeignKey(d => d.BugId)
                .HasConstraintName("FK_Users_Bug");

            entity.HasOne(d => d.Project).WithMany(p => p.Users)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Users_Project");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<UserProject>(entity =>
        {
            entity.HasKey(e => e.UserProjectId).HasName("PK__UserProj__5F7DD4974019BF7A");

            entity.ToTable("UserProject");

            entity.HasOne(d => d.Project).WithMany(p => p.UserProjects)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_UserProject_Project");

            entity.HasOne(d => d.User).WithMany(p => p.UserProjects)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserProject_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
