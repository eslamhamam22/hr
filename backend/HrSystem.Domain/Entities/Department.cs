using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities;

/// <summary>
/// Department entity
/// </summary>
public class Department : Entity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}
