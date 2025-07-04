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
            return new Message() {
                Recipients = new string[] { Configuration["ToEmail"] },
                Cc = new string[] { },
                Bcc = new string[] { },
                Header = new Header() {
                    Subject = "Test Mail from C#",
                    From = Configuration["FromEmail"],
                    CustomHeaders = new Dictionary<string, string> {
                        { "X-Custom-Header", "Custom Value" },
                        { "X-Another-Header", "Another Value" }
                    }
                },
                Content = new Content() {
                    PlainText = "Hello Again",
                    HtmlText = "<html><body><h1>Hello Again</h1></body></html>"
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
