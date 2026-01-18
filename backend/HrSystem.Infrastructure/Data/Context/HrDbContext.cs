using HrSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using HrSystem.Application.Common.Interfaces;

namespace HrSystem.Infrastructure.Data.Context;

/// <summary>
/// EF Core DbContext for the HR System
/// </summary>
public class HrDbContext : DbContext, IApplicationDbContext
{
    public HrDbContext(DbContextOptions<HrDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<OvertimeRequest> OvertimeRequests => Set<OvertimeRequest>();
    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Department entity
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure LeaveRequest entity
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
        });

        // Configure OvertimeRequest entity
        modelBuilder.Entity<OvertimeRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
        });

        // Configure ApprovalLog entity
        modelBuilder.Entity<ApprovalLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestType).HasMaxLength(50);
        });

        // Configure AttendanceLog entity
        modelBuilder.Entity<AttendanceLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
        });
    }
}
