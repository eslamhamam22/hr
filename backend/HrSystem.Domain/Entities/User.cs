using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities;

/// <summary>
/// User entity representing employees, managers, HR, and admins
/// </summary>
public class User : Entity, IAggregateRoot
{
    public string Username { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public RoleType Role { get; set; }
    
    /// <summary>
    /// Reference to the manager - defines approval chain
    /// </summary>
    public Guid? ManagerId { get; set; }
    
    public Guid? DepartmentId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastLoginAt { get; set; }
}
