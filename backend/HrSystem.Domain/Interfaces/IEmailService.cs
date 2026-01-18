namespace HrSystem.Domain.Interfaces;

/// <summary>
/// Email service abstraction for notifications
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendBatchAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken = default);
}
