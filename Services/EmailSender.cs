using System.Net;
using System.Net.Mail;

public class EmailSender
{
    private static IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

    public static bool SendEmail(string subject, string body, string sendto, string bcc, string cc)
    {
        string email = config.GetValue<string>($"Smtp:Email");
        string password = config.GetValue<string>($"Smtp:Password");
        string host = config.GetValue<string>("Smtp:Server");
        int port = config.GetValue<int>("Smtp:Port");
        string displayName = config.GetValue<string>("Smtp:DisplayName");

        sendto = sendto.Replace(";", ",");
        bcc = bcc.Replace(";", ",");
        cc = cc.Replace(";", ",");

        MailAddress from = new MailAddress(email, displayName);
        MailMessage message = new MailMessage();
        message.From = from;
        message.Subject = subject;
        message.IsBodyHtml = true;
        message.Body = body;

        if (sendto != "")
        {
            string[] sendto_list = sendto.Split(',');
            foreach (string sendto_email in sendto_list)
            {
                message.To.Add(new MailAddress(sendto_email));
            }
        }

        if (cc != "")
        {
            string[] cc_list = cc.Split(',');
            foreach (string cc_email in cc_list)
            {
                message.CC.Add(new MailAddress(cc_email));
            }
        }

        if (bcc != "")
        {
            string[] bcc_List = bcc.Split(',');
            foreach (string bcc_email in bcc_List)
            {
                message.Bcc.Add(new MailAddress(bcc_email));
            }
        }

        SmtpClient client = new SmtpClient();
        client.UseDefaultCredentials = false;
        client.Host = host;
        client.Port = port;
        client.Credentials = new NetworkCredential(email, password);
        client.EnableSsl = true;

        try
        {
            client.Send(message);
            return true;
        }
        catch
        {
            return false;
        }
    }
}