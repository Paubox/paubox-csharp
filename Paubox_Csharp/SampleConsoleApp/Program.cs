using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Paubox;

namespace SampleConsoleApp
{
    class Program
    {
        private static readonly IConfiguration Configuration;

        static Program()
        {
            string exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Configuration = new ConfigurationBuilder()
                .SetBasePath(exeDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("▶️ Starting QA Console App...");

            Console.WriteLine("🧰 Initializing EmailLibrary...");
            EmailLibrary.Initialize(Configuration);

            string trackingId = SendMessage();

            Console.WriteLine("📧 Tracking ID: " + trackingId);

            GetEmailDispositionResponse response = EmailLibrary.GetEmailDisposition(trackingId);

            Console.WriteLine("📧 Response: " + response);
        }

        static string SendMessage()
        {
            Console.WriteLine("📧 Sending message to " + Configuration["ToEmail"] + "...");

            Message message = new Message();
            message.Recipients = new string[] { Configuration["ToEmail"] };

            Header header = new Header();
            header.Subject = "Test Mail from C#";
            header.From = Configuration["FromEmail"];
            message.Header = header;

            Content content = new Content();
            content.PlainText = "Hello Again";
            message.Content = content;

            Attachment attachment = new Attachment();
            List<Attachment> listAttachments = new List<Attachment>();
            attachment.FileName = "hello_world.txt";
            attachment.ContentType = "text/plain";
            attachment.Content = "SGVsbG8gV29ybGQh\n";
            listAttachments.Add(attachment);

            message.Attachments = listAttachments;

            SendMessageResponse response = EmailLibrary.SendMessage(message);

            return response.SourceTrackingId;
        }
    }
}
