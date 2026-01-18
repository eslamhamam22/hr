using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.Data.Seeder
{
    public class DataSeeder
    {
        public static async Task SeedAsync(HrDbContext context)
        {
            try
            {
                // Seed Departments
                if (!await context.Departments.AnyAsync())
                {
                    var departments = new List<Department>
                    {
                        new Department
                        {
                            Id = Guid.NewGuid(),
                            Name = "Information Technology",
                            Description = "IT department responsible for system infrastructure and development",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Department
                        {
                            Id = Guid.NewGuid(),
                            Name = "Human Resources",
                            Description = "HR department managing employee relations and recruitment",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Department
                        {
                            Id = Guid.NewGuid(),
                            Name = "Finance",
                            Description = "Finance department handling budgets and financial operations",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Department
                        {
                            Id = Guid.NewGuid(),
                            Name = "Operations",
                            Description = "Operations department managing day-to-day business activities",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Departments.AddRangeAsync(departments);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"Seeded {departments.Count} departments");
                }

                // Seed Users
                if (!await context.Users.AnyAsync())
                {
                    var departments = await context.Departments.ToListAsync();
                    var itDept = departments.FirstOrDefault(d => d.Name == "Information Technology");
                    var hrDept = departments.FirstOrDefault(d => d.Name == "Human Resources");
                    var financeDept = departments.FirstOrDefault(d => d.Name == "Finance");
                    var opsDept = departments.FirstOrDefault(d => d.Name == "Operations");

                    // Create users hierarchy: Admin -> HR -> Managers -> Employees
                    var adminId = Guid.NewGuid();
                    var hrManagerId = Guid.NewGuid();
                    var itManagerId = Guid.NewGuid();
                    var financeManagerId = Guid.NewGuid();

                    var users = new List<User>
                    {
                        // Admin User
                        new User
                        {
                            Id = adminId,
                            Username = "admin",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                            FullName = "System Administrator",
                            Email = "admin@hrsystem.com",
                            Role = RoleType.Admin,
                            ManagerId = null,
                            DepartmentId = itDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        // HR Manager
                        new User
                        {
                            Id = hrManagerId,
                            Username = "hr.manager",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Hr@123"),
                            FullName = "Sarah Johnson",
                            Email = "sarah.johnson@hrsystem.com",
                            Role = RoleType.HR,
                            ManagerId = adminId,
                            DepartmentId = hrDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        // IT Manager
                        new User
                        {
                            Id = itManagerId,
                            Username = "it.manager",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                            FullName = "John Smith",
                            Email = "john.smith@hrsystem.com",
                            Role = RoleType.Manager,
                            ManagerId = adminId,
                            DepartmentId = itDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        // Finance Manager
                        new User
                        {
                            Id = financeManagerId,
                            Username = "finance.manager",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                            FullName = "Emily Davis",
                            Email = "emily.davis@hrsystem.com",
                            Role = RoleType.Manager,
                            ManagerId = adminId,
                            DepartmentId = financeDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        // IT Employees
                        new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "developer1",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                            FullName = "Michael Brown",
                            Email = "michael.brown@hrsystem.com",
                            Role = RoleType.Employee,
                            ManagerId = itManagerId,
                            DepartmentId = itDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "developer2",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                            FullName = "Lisa Anderson",
                            Email = "lisa.anderson@hrsystem.com",
                            Role = RoleType.Employee,
                            ManagerId = itManagerId,
                            DepartmentId = itDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        // HR Employees
                        new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "hr.specialist",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                            FullName = "David Wilson",
                            Email = "david.wilson@hrsystem.com",
                            Role = RoleType.Employee,
                            ManagerId = hrManagerId,
                            DepartmentId = hrDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        // Finance Employees
                        new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "accountant",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                            FullName = "Jennifer Martinez",
                            Email = "jennifer.martinez@hrsystem.com",
                            Role = RoleType.Employee,
                            ManagerId = financeManagerId,
                            DepartmentId = financeDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        // Operations Employees
                        new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "ops.coordinator",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                            FullName = "Robert Taylor",
                            Email = "robert.taylor@hrsystem.com",
                            Role = RoleType.Employee,
                            ManagerId = adminId,
                            DepartmentId = opsDept?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"Seeded {users.Count} users");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding data: {ex.Message}");
                throw;
            }
        }
    }
}
