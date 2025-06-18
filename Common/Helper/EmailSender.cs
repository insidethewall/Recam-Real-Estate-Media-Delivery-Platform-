using System.Net.Mail;


namespace RecamSystemApi.Helper;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        string mail = _configuration["EmailSettings:Mail"] ?? throw new ArgumentNullException("EmailSettings:Mail is not configured.");
        string password = _configuration["EmailSettings:Password"] ?? throw new ArgumentNullException("EmailSettings:Password is not configured.");
        string emailServer = _configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("EmailSettings:SmtpServer is not configured.");
        int port = int.Parse(_configuration["EmailSettings:Port"] ?? "587") ;
        var smtpClient = new SmtpClient(emailServer)
        {
            Credentials = new System.Net.NetworkCredential(mail, password),
            Port = port,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false

        };
        
        await smtpClient.SendMailAsync(new MailMessage
        (
            from: mail,
            to: email,
            subject,
            message

        ));
    }
}