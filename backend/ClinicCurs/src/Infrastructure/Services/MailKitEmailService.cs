using Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Services;

public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public MailKitEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        var host = _configuration["Smtp:Host"];
        var port = int.Parse(_configuration["Smtp:Port"]!);
        var useSsl = bool.Parse(_configuration["Smtp:UseSsl"] ?? "false");
        var senderEmail = _configuration["Smtp:SenderEmail"]!;
        var senderName = _configuration["Smtp:SenderName"]!;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Подтверждение Email - Clinic Curs";

        // Формируем красивое HTML-сообщение
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Добро пожаловать в клинику!</h2>
                <p>Пожалуйста, подтвердите свой адрес электронной почты, перейдя по ссылке ниже:</p>
                <p><a href='{verificationLink}' style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Подтвердить Email</a></p>
                <p>Если кнопка не работает, скопируйте эту ссылку в браузер:</p>
                <p>{verificationLink}</p>
                <hr/>
                <p style='color: #888; font-size: 12px;'>Если вы не регистрировались на нашем сайте, просто проигнорируйте это письмо.</p>
            </div>";

        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(host, port, useSsl);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
            throw;
        }
    }
    
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_configuration["Smtp:SenderName"], _configuration["Smtp:SenderEmail"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Сброс пароля - Clinic Curs";

        var htmlBody = $@"
        <div style='font-family: Arial, sans-serif; padding: 20px;'>
            <h2>Восстановление доступа</h2>
            <p>Вы получили это письмо, потому что запросили сброс пароля. Нажмите на кнопку ниже, чтобы установить новый пароль:</p>
            <p><a href='{resetLink}' style='padding: 10px 20px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px;'>Сбросить пароль</a></p>
            <p>Ссылка действительна в течение 1 часа.</p>
        </div>";

        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]!), false);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    
    public async Task SendStaffCredentialsEmailAsync(string toEmail, string password)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_configuration["Smtp:SenderName"], _configuration["Smtp:SenderEmail"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Ваши учетные данные - Clinic Curs";

        var htmlBody = $@"
        <div style='font-family: Arial, sans-serif; padding: 20px;'>
            <h2>Добро пожаловать в команду!</h2>
            <p>Для вас был создан аккаунт сотрудника. Используйте следующие данные для входа:</p>
            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; border: 1px solid #dee2e6;'>
                <p><strong>Email:</strong> {toEmail}</p>
                <p><strong>Пароль:</strong> {password}</p>
            </div>
            <p style='color: #dc3545;'><strong>Внимание:</strong> Пожалуйста, смените пароль после первого входа в систему.</p>
        </div>";

        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]!), false);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
