namespace Iitrust.CRLPublication.Service.Notification
{
    using System;
    using System.IO;
    using System.Net.Mail;

    /// <summary>
    /// Класс, который предоставляет методы для отправки почты.
    /// </summary>
    public static class MailClient
    {
        /// <summary>
        /// Отправляет сообщение.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="attachment">Вложение письма, представляющее поток.</param>
        /// <param name="attachmentName">Наименование вложения письма.</param>
        public static void Send(String message, Stream attachment = null, String attachmentName = "attachment.crl")
        {
            var smtpClient = new SmtpClient();
            var mailMessage = new MailMessage();
            foreach (var to in ConfigurationHelper.SmtpTo)
            {
                mailMessage.To.Add(to);
            }

            mailMessage.Subject = ConfigurationHelper.SmtpSubject;
            mailMessage.Body = message;

            if (attachment != null)
            {
                mailMessage.Attachments.Add(new Attachment(attachment, attachmentName));
            }

            smtpClient.Send(mailMessage);

            mailMessage.Dispose();
            smtpClient.Dispose();
        }
    }
}