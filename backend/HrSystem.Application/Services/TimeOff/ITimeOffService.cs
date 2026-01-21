using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.TimeOff;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Services.TimeOff;

public interface ITimeOffService
{
    Task<PaginatedResult<TimeOffRequestDto>> GetTimeOffRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default);

    Task<TimeOffRequestDto?> GetTimeOffRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TimeOffRequestDto> CreateTimeOffRequestAsync(Guid userId, CreateTimeOffRequestDto dto, CancellationToken cancellationToken = default);

    Task<bool> SubmitTimeOffRequestAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteTimeOffRequestAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> ApproveTimeOffRequestAsync(Guid id, Guid approverId, CancellationToken cancellationToken = default);

    Task<bool> RejectTimeOffRequestAsync(Guid id, Guid approverId, string reason, CancellationToken cancellationToken = default);

    Task<IEnumerable<TimeOffRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TimeOffRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default);
}
