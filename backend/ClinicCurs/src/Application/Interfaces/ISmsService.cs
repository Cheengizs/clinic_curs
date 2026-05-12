namespace Application.Interfaces;

public interface ISmsService
{
    Task SendVerificationCodeAsync(string phoneNumber, string code);
}
