using CollegeGuideApi.Helpers;

namespace CollegeGuideApi.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest, CancellationToken cancellationToken = default);

    }
}
