using System.Collections.Generic;
using UnityEngine;
using Syrus.Plugins.DFV2Client;
using System;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security;
using System.IO;

public class EmailUtils : MonoBehaviour
{
    public static DialogFlowV2Client client;
        
    public static Action EmailResponse;

    public static void CheckEmailSettings()
    {
        // check if user set the email account
        if (string.IsNullOrEmpty(SystemManager.Instance.emailAddress) || string.IsNullOrEmpty(SystemManager.Instance.password))
        {
            client.DetectIntentFromEvent("EmailNoAccount", null);
        }
        else
        {
            client.DetectIntentFromEvent("EmailHasAccount", null);
            EmailResponse?.Invoke();
        }
    }

    // Gmail no longer allow less secure apps to access Gmail
    // solution: https://www.dotblogs.com.tw/anmlab/2022/06/14/153437
    public static void SendEmail(string to, string subject, string body, List<string> attachments)
    {
        string address = SystemManager.Instance.emailAddress + "@gmail.com";
        string password = SystemManager.Instance.GetDecodedPassword();

        // encrypt password
        SecureString securePassword = new SecureString();
        foreach (char c in password)
        {
            securePassword.AppendChar(c);
        }
        securePassword.MakeReadOnly();

        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(address);
        mail.To.Add(to);
        mail.Subject = subject;
        mail.Body = body;
        mail.IsBodyHtml = true;

        // add attached files if they exist
        foreach (string attachment in attachments)
        {
            if (File.Exists(attachment))
            {
                mail.Attachments.Add(new Attachment(attachment));
            }
        }

        SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
        smtpClient.Port = 587;
        smtpClient.Credentials = new NetworkCredential(address, securePassword);
        smtpClient.EnableSsl = true;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true; 
            };

        try
        {
            smtpClient.Send(mail);
            client.DetectIntentFromEvent("EmailSuccess", null);
        }
        catch (SmtpException ex) // user email address or password is wrong
        {
            Debug.Log(ex);
            client.DetectIntentFromEvent("EmailFail", null);
        }
        finally
        {
            securePassword.Dispose(); // free memory
        }
    }
}
