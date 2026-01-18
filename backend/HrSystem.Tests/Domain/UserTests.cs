using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using Xunit;

namespace HrSystem.Tests.Domain;

/// <summary>
/// Unit tests for User domain entity
/// </summary>
public class UserTests
{
    [Fact]
    public void CreateUser_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            Username = "john.doe",
            FullName = "John Doe",
            Email = "john.doe@example.com",
            Role = RoleType.Employee,
            PasswordHash = "hashed_password"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("john.doe", user.Username);
        Assert.Equal("John Doe", user.FullName);
        Assert.Equal(RoleType.Employee, user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void CreateUser_WithManagerId_ShouldSetApprovalChain()
    {
        // Arrange
        var managerId = Guid.NewGuid();

        // Act
        var user = new User
        {
            Username = "jane.smith",
            FullName = "Jane Smith",
            Email = "jane.smith@example.com",
            Role = RoleType.Employee,
            ManagerId = managerId,
            PasswordHash = "hashed_password"
        };

        // Assert
        Assert.Equal(managerId, user.ManagerId);
    }
}
