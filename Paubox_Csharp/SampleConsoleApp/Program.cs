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
            Message message = CreateValidMessage("Send Message Test");

            Console.WriteLine("🧪 SendMessage Test");
            SendMessageResponse response = paubox.SendMessage(message);

            string trackingId = response.SourceTrackingId;
            Console.WriteLine("🔍 Tracking ID: " + trackingId);

            Console.WriteLine("ℹ️ Getting email disposition for tracking ID: " + trackingId);
            GetEmailDispositionResponse dispositionResponse = paubox.GetEmailDisposition(trackingId);

            Console.WriteLine("ℹ️ Status: " + dispositionResponse.Data.Message.Message_Deliveries[0].Status.DeliveryStatus);

            Console.WriteLine("✉️ Creating a list of 2 Messages to test the SendBulkMessages method...");
            Message[] messages = new Message[] {
                CreateValidMessage("Send Bulk Message Test (First Message)"),
                CreateValidMessage("Send Bulk Message Test (Second Message)")
            };

            Console.WriteLine("🧪 SendBulkMessages Test");
            SendBulkMessagesResponse bulkResponse = paubox.SendBulkMessages(messages);

            Console.WriteLine("🔍 Tracking ID for first message: " + bulkResponse.Messages[0].SourceTrackingId);
            Console.WriteLine("🔍 Tracking ID for second message: " + bulkResponse.Messages[1].SourceTrackingId);
        }

        static Message CreateValidMessage(string title)
        {
            return new Message() {
                Recipients = new string[] { Configuration["ToEmail"] },
                Cc = new string[] { },
                Bcc = new string[] { },
                Header = new Header() {
                    Subject = $"{title} - {DateTime.Now:MMM dd HH:mm:ss}",
                    From = Configuration["FromEmail"],
                    CustomHeaders = new Dictionary<string, string> {
                        { "X-Custom-Header", "Custom Value" },
                        { "X-Another-Header", "Another Value" }
                    }
                },
                Content = new Content() {
                    PlainText = $"{title} - {DateTime.Now:MMM dd HH:mm:ss}",
                    HtmlText = $"<html><body><h1>{title}</h1><p>This is a test email sent from the Paubox C# SDK.</p></body></html>"
                },
                Attachments = new List<Attachment>() {
                    new Attachment() {
                        FileName = "hello_world.txt",
                        ContentType = "text/plain",
                        Content = "SGVsbG8gV29ybGQh\n"
                    }
                }
            };
        }
    }
}
