using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using Xunit;

namespace HrSystem.Tests.Domain;

/// <summary>
/// Unit tests for LeaveRequest domain entity
/// </summary>
public class LeaveRequestTests
{
    [Fact]
    public void CreateLeaveRequest_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.Now.AddDays(5);
        var endDate = DateTime.Now.AddDays(10);

        // Act
        var leaveRequest = new LeaveRequest
        {
            UserId = userId,
            LeaveType = LeaveType.Vacation,
            StartDate = startDate,
            EndDate = endDate,
            Reason = "Summer vacation",
            Status = RequestStatus.Draft
        };

        // Assert
        Assert.NotEqual(Guid.Empty, leaveRequest.Id);
        Assert.Equal(userId, leaveRequest.UserId);
        Assert.Equal(LeaveType.Vacation, leaveRequest.LeaveType);
        Assert.Equal(RequestStatus.Draft, leaveRequest.Status);
    }

    [Fact]
    public void CreateLeaveRequest_ShouldHaveCreatedAtTimestamp()
    {
        // Arrange & Act
        var leaveRequest = new LeaveRequest
        {
            UserId = Guid.NewGuid(),
            LeaveType = LeaveType.Vacation,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(1),
            Reason = "Test"
        };

        // Assert
        Assert.NotEqual(default, leaveRequest.CreatedAt);
        Assert.True(leaveRequest.CreatedAt <= DateTime.UtcNow);
    }
}
