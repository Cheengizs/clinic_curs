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
}
