using FluentValidation;
using HrSystem.Application.DTOs.Requests;

namespace HrSystem.Application.Validators;

/// <summary>
/// Validator for CreateLeaveRequestDto
/// </summary>
public class CreateLeaveRequestDtoValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestDtoValidator()
    {
        RuleFor(x => x.LeaveTypeId)
            .GreaterThan(0)
            .WithMessage("Leave type is required");
        
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Start date must be today or in the future");
        
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date");
        
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}
