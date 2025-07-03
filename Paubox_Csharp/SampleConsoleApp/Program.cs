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

            Console.WriteLine("🧰 Constructing the EmailLibrary...");
            EmailLibrary paubox = new EmailLibrary(Configuration);

            Console.WriteLine("✉️ Creating a new valid Message object...");
            Message message = CreateValidMessage();

            Console.WriteLine("✉️ Sending message from " + Configuration["FromEmail"] + " to " + Configuration["ToEmail"] + "...");
            SendMessageResponse response = paubox.SendMessage(message);

            string trackingId = response.SourceTrackingId;
            Console.WriteLine("🔍 Tracking ID: " + trackingId);

            Console.WriteLine("ℹ️ Getting email disposition for tracking ID: " + trackingId);
            GetEmailDispositionResponse dispositionResponse = paubox.GetEmailDisposition(trackingId);

            Console.WriteLine("ℹ️ Response: " + dispositionResponse);
        }

        static Message CreateValidMessage()
        {
            Message message = new Message();
            message.Recipients = new string[] { Configuration["ToEmail"] };

            Header header = new Header();
            header.Subject = "Test Mail from C#";
            header.From = Configuration["FromEmail"];
            header.CustomHeaders = new Dictionary<string, string> {
                { "X-Custom-Header", "Custom Value" },
                { "X-Another-Header", "Another Value" }
            };
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

            return message;
        }
    }
}
