using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using CommonUtility.Extension;
using CommonUtility.Logging;
using MailKit.Net.Smtp;
using MimeKit;
using OfficeAutomationClient.OA;

namespace OfficeAutomationClient.Helper
{
    internal class CrashReporter
    {
        private static readonly ILogger Logger = LogHelper.GetLogger<CrashReporter>();

        private static readonly Dictionary<string, string> SpecialSmtpServers = new Dictionary<string, string>
        {
            {"win-stock.com.cn", "mail.win-stock.com.cn"}
        };

        /// <summary>
        ///     发信人地址
        /// </summary>
        public string From { get; set; }

        /// <summary>
        ///     收件人电子邮件集合
        /// </summary>
        public List<string> To { get; set; } = new List<string>();

        /// <summary>
        ///     抄送收件人地址集合
        /// </summary>
        public List<string> Cc { get; set; } = new List<string>();

        /// <summary>
        ///     密件抄送收件人地址集合
        /// </summary>
        public List<string> Bcc { get; set; } = new List<string>();

        /// <summary>
        ///     邮件主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        ///     附件集合
        /// </summary>
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

        /// <summary>
        ///     邮件正文的编码
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///     是否为Html格式
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        ///     优先级
        /// </summary>
        public MessagePriority Priority { get; set; }

        /// <summary>
        ///     Smtp 服务器地址
        /// </summary>
        public string SmtpServerHost { get; set; }

        /// <summary>
        ///     Smtp 服务器端口
        /// </summary>
        public int SmtpServerPort { get; set; }

        /// <summary>
        ///     是否使用安全套接字层加密连接
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        ///     发信人密码
        /// </summary>
        public SecureString Password { get; set; }


        private MimeMessage CreateMailMessage()
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(From));
            To.ForEach(email => msg.To.Add(new MailboxAddress(email)));
            Cc.ForEach(email => msg.Cc.Add(new MailboxAddress(email)));
            Bcc.ForEach(email => msg.Bcc.Add(new MailboxAddress(email)));

            var builder = new BodyBuilder();

            if (IsBodyHtml)
                builder.HtmlBody = Body;
            else
                builder.TextBody = Body;

            Attachments.ForEach(attachment =>
            {
                var contentType = ContentType.Parse(ParserOptions.Default, attachment.MediaType);
                if (string.IsNullOrEmpty(attachment.ContentId))
                {
                    builder.Attachments.Add(attachment.FileName, contentType);
                }
                else
                {
                    var att = builder.LinkedResources.Add(attachment.FileName, contentType);
                    att.ContentId = attachment.ContentId;
                }
            });
            msg.Body = builder.ToMessageBody();
            msg.Priority = Priority;
            msg.Subject = Subject;

            return msg;
        }

        public bool Send()
        {
            var smtp = new SmtpClient
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };

            var msg = CreateMailMessage();

            Exception ex;
            try
            {
                smtp.Connect(SmtpServerHost, SmtpServerPort, EnableSsl);

                if (Password != null && Password.Length > 0)
                    smtp.Authenticate(From, Password.CreateString());

                smtp.Send(msg);
                smtp.Disconnect(true);
                return true;
            }
            catch (Exception e)
            {
                ex = e;
            }
            finally
            {
                smtp.Dispose();
            }

            // 释放占用的log附件之后再写日志
            Logger.Error(ex.InnerException ?? ex, null, "发送邮件失败 From:{0} To:{1} Smtp:[{2}:{3}]", msg.From, msg.To,
                SmtpServerHost,
                SmtpServerPort);

            return false;
        }

        public static CrashReporter CreateSimpleCrashReporter(string from, bool enableSsl = false)
        {
            var reporter = new CrashReporter
            {
                From = from,
                Priority = MessagePriority.Urgent,
                Subject = $"{Business.AssemblyName.Name} Crash Report({DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss})",
                EnableSsl = enableSsl,
                SmtpServerHost = GetSmtpServerHost(from),
                SmtpServerPort = GetSmtpServerPort(enableSsl)
            };
            reporter.To.Add("oabugreport@sina.com");

            return reporter;
        }

        private static int GetSmtpServerPort(bool enableSsl)
        {
            return enableSsl ? 465 /* 587/994 */ : 25;
        }

        private static string GetSmtpServerHost(string email)
        {
            var index = email.IndexOf('@');
            if (index > 0 && email.Length > index + 1)
            {
                var domain = email.Substring(index + 1);
                if (SpecialSmtpServers.TryGetValue(domain, out var smtp))
                    return smtp;
                return $"smtp.{domain}";
            }

            return string.Empty;
        }
    }

    public class Attachment
    {
        /// <summary>
        ///     附件路径
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     MIME 内容标头信息
        ///     <see cref="System.Net.Mime.MediaTypeNames" />
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        ///     MIME 内容ID
        /// </summary>
        public string ContentId { get; set; }
    }
}