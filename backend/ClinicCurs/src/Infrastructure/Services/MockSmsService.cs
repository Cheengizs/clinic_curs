using Application.Interfaces;

namespace Infrastructure.Services;

public class MockSmsService : ISmsService
{
    public Task SendVerificationCodeAsync(string phoneNumber, string code)
    {
        Console.WriteLine("\n=================================================");
        Console.WriteLine($"📱 MOCK SMS SENDING TO: {phoneNumber}");
        Console.WriteLine($"🔢 Verification Code: {code}");
        Console.WriteLine("=================================================\n");
        return Task.CompletedTask;
    }
}
