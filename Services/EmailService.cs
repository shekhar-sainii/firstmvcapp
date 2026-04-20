using MailKit.Net.Smtp;
using MimeKit;
using System.Text;

namespace FirstMvcApp.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
    Task<bool> SendWelcomeEmailAsync(string email, string fullName);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderPassword;
    private readonly string _senderName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var emailConfig = configuration.GetSection("Email");
        _smtpServer = emailConfig["SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
        _senderEmail = emailConfig["SenderEmail"] ?? "";
        _senderPassword = emailConfig["SenderPassword"] ?? "";
        _senderName = emailConfig["SenderName"] ?? "FirstMvcApp";
        
        _logger.LogInformation($"EmailService initialized - SMTP: {_smtpServer}:{_smtpPort}, From: {_senderEmail}");
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null)
    {
        try
        {
            _logger.LogInformation($"Attempting to send email to {to} with subject: {subject}");
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _senderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var multipart = new Multipart("alternative");

            if (!string.IsNullOrEmpty(plainTextBody))
            {
                var textPart = new TextPart("plain")
                {
                    Text = plainTextBody
                };
                multipart.Add(textPart);
            }

            var htmlPart = new TextPart("html")
            {
                Text = htmlBody
            };
            multipart.Add(htmlPart);

            message.Body = multipart;

            using (var client = new SmtpClient())
            {
                _logger.LogInformation($"Connecting to SMTP server {_smtpServer}:{_smtpPort}...");
                await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                _logger.LogInformation("SMTP connection established");
                
                _logger.LogInformation($"Authenticating with {_senderEmail}...");
                await client.AuthenticateAsync(_senderEmail, _senderPassword);
                _logger.LogInformation("SMTP authentication successful");
                
                _logger.LogInformation($"Sending email message...");
                await client.SendAsync(message);
                _logger.LogInformation("Email message sent");
                
                await client.DisconnectAsync(true);
                _logger.LogInformation("SMTP connection closed");
            }

            _logger.LogInformation($"Email sent successfully to {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {to}: {ex.Message}");
            _logger.LogError($"Exception type: {ex.GetType().Name}");
            if (ex.StackTrace != null)
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string fullName)
    {
        try
        {
            var subject = "Welcome to FirstMvcApp! 🎉";
            var htmlBody = GenerateWelcomeEmailTemplate(fullName, email);

            return await SendEmailAsync(email, subject, htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending welcome email to {email}: {ex.Message}");
            return false;
        }
    }

    private string GenerateWelcomeEmailTemplate(string fullName, string email)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to FirstMvcApp</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px 20px;
            text-align: center;
        }}
        .header h1 {{
            font-size: 28px;
            margin-bottom: 10px;
            font-weight: 600;
        }}
        .header p {{
            font-size: 14px;
            opacity: 0.9;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .greeting {{
            font-size: 18px;
            color: #333;
            margin-bottom: 20px;
            line-height: 1.6;
        }}
        .greeting strong {{
            color: #667eea;
        }}
        .features {{
            margin: 30px 0;
        }}
        .features h3 {{
            font-size: 16px;
            color: #333;
            margin-bottom: 15px;
        }}
        .feature-list {{
            list-style: none;
        }}
        .feature-list li {{
            padding: 10px 0;
            padding-left: 30px;
            position: relative;
            color: #555;
            font-size: 14px;
            line-height: 1.6;
        }}
        .feature-list li:before {{
            content: '✓';
            position: absolute;
            left: 0;
            color: #667eea;
            font-weight: bold;
            font-size: 16px;
        }}
        .cta-section {{
            margin: 30px 0;
            text-align: center;
        }}
        .cta-button {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: 600;
            font-size: 14px;
            transition: transform 0.2s, box-shadow 0.2s;
        }}
        .cta-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }}
        .divider {{
            height: 1px;
            background: #e0e0e0;
            margin: 30px 0;
        }}
        .support-text {{
            font-size: 13px;
            color: #999;
            line-height: 1.6;
        }}
        .footer {{
            background: #f9f9f9;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #e0e0e0;
        }}
        .footer p {{
            font-size: 12px;
            color: #999;
            margin: 5px 0;
        }}
        .social-links {{
            margin-top: 10px;
        }}
        .social-links a {{
            display: inline-block;
            margin: 0 5px;
            color: #667eea;
            text-decoration: none;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to FirstMvcApp!</h1>
            <p>Your Journey Starts Here</p>
        </div>

        <div class='content'>
            <div class='greeting'>
                Hi <strong>{fullName}</strong>,<br><br>
                Welcome aboard! We're thrilled to have you join our community. Your account has been successfully created, and you're all set to get started.
            </div>

            <div class='features'>
                <h3>What You Can Do Now:</h3>
                <ul class='feature-list'>
                    <li>Access your personalized dashboard</li>
                    <li>Manage your profile and preferences</li>
                    <li>Explore our full range of features</li>
                    <li>Connect with other users in our community</li>
                    <li>Get support whenever you need it</li>
                </ul>
            </div>

            <div class='cta-section'>
                <a href='http://localhost:5068/Account/Profile' class='cta-button'>View Your Profile</a>
            </div>

            <div class='divider'></div>

            <div class='support-text'>
                <strong>Your Account Information:</strong><br>
                Email: {email}<br><br>
                If you have any questions or need assistance, don't hesitate to reach out to our support team. We're here to help!
            </div>
        </div>

        <div class='footer'>
            <p>&copy; 2026 FirstMvcApp. All rights reserved.</p>
            <p>You're receiving this email because you recently signed up for FirstMvcApp.</p>
            <div class='social-links'>
                <a href='#'>Facebook</a> | 
                <a href='#'>Twitter</a> | 
                <a href='#'>LinkedIn</a>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
