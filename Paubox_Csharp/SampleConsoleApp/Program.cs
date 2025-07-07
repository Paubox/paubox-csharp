using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SampleConsoleApp
{
    class Program
    {
        private static readonly IConfiguration Configuration;
        private static readonly string templatePath;

        static Program()
        {
            string exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Configuration = new ConfigurationBuilder()
                .SetBasePath(exeDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Paubox_Csharp",
                "SampleConsoleApp",
                "ExampleTemplate.hbs"
            );
        }

        static void Main(string[] args)
        {
            Console.WriteLine("▶️ Starting QA Console App...");

            Console.WriteLine("🧰 Constructing the EmailLibrary...");
            EmailLibrary paubox = new EmailLibrary(Configuration);

            // -----------------------------------------------------------------------
            // Send Message, Get Email Disposition, and Send Bulk Messages Tests

            string trackingId = TestSendMessage(paubox);
            TestGetEmailDisposition(paubox, trackingId);
            TestSendBulkMessages(paubox);

            // -----------------------------------------------------------------------
            // Dynamic Template Tests

            string templateName = "Example Template " + DateTime.Now.ToString("MMM dd HH:mm:ss");

            TestCreateTemplate(paubox, templateName);
            int templateId = TestListTemplates(paubox, templateName);
            TestGetTemplate(paubox, templateId, templateName);
            string updatedTemplateName = TestUpdateTemplate(paubox, templateId, templateName);
            TestDeleteTemplate(paubox, templateId, updatedTemplateName);
            CleanUpTemplates(paubox);

            Console.WriteLine("\n----------------------------------------------------------------");
            Console.WriteLine("💚 All QA Tests completed successfully! 💚");
            Console.WriteLine("----------------------------------------------------------------");
        }

        static string TestSendMessage(EmailLibrary paubox)
        {
            Console.WriteLine("🧪 Test Send Message with attachments and custom headers...");
            Message message = CreateValidMessage("Send Message Test");

            SendMessageResponse response = paubox.SendMessage(message);

            if (response.Errors != null)
            {
                throw new Exception("❌ Errors when sending the message: " + response.Errors);
            }

            if (response.SourceTrackingId == null)
            {
                throw new Exception("❌ No tracking ID returned when sending the message.");
            }

            if (response.Data != "Service OK")
            {
                throw new Exception("❌ Unexpected data (expected 'Service OK'): " + response.Data);
            }

            if (response.CustomHeaders == null)
            {
                throw new Exception("❌ No custom headers returned when sending the message.");
            }

            if (response.CustomHeaders.Count != 2)
            {
                throw new Exception("❌ Unexpected number of custom headers (expected 2): " + response.CustomHeaders.Count);
            }

            if (response.CustomHeaders["X-Custom-Header"] != "Custom Value")
            {
                throw new Exception("❌ Unexpected custom header (expected 'X-Custom-Header'): " + response.CustomHeaders["X-Custom-Header"]);
            }

            if (response.CustomHeaders["X-Another-Header"] != "Another Value")
            {
                throw new Exception("❌ Unexpected custom header (expected 'X-Another-Header'): " + response.CustomHeaders["X-Another-Header"]);
            }

            string trackingId = response.SourceTrackingId;

            Console.WriteLine("✅ Message sent successfully.");
            return trackingId;
        }

        static void TestGetEmailDisposition(EmailLibrary paubox, string trackingId)
        {
            Console.WriteLine("🧪 Test Get Email Disposition for tracking ID: " + trackingId + "...");
            GetEmailDispositionResponse dispositionResponse = paubox.GetEmailDisposition(trackingId);

            if (dispositionResponse.Errors != null)
            {
                throw new Exception("❌ Errors when getting the email disposition: " + dispositionResponse.Errors);
            }

            if (dispositionResponse.SourceTrackingId != trackingId)
            {
                throw new Exception("❌ Unexpected tracking ID: " + dispositionResponse.SourceTrackingId);
            }

            if (dispositionResponse.Data == null)
            {
                throw new Exception("❌ No data returned when getting the email disposition.");
            }

            if (dispositionResponse.Data.Message == null)
            {
                throw new Exception("❌ No message returned when getting the email disposition.");
            }

            if (dispositionResponse.Data.Message.Message_Deliveries == null)
            {
                throw new Exception("❌ No message deliveries returned when getting the email disposition.");
            }

            if (dispositionResponse.Data.Message.Message_Deliveries.Count != 1)
            {
                throw new Exception("❌ Unexpected number of message deliveries (expected 1): " + dispositionResponse.Data.Message.Message_Deliveries.Count);
            }

            if (dispositionResponse.Data.Message.Message_Deliveries[0].Recipient != Configuration["ToEmail"])
            {
                throw new Exception("❌ Unexpected recipient (expected '" + Configuration["ToEmail"] + "'): " + dispositionResponse.Data.Message.Message_Deliveries[0].Recipient);
            }

            Console.WriteLine("✅ Email disposition retrieved successfully.");
        }

        static void TestSendBulkMessages(EmailLibrary paubox)
        {
            Console.WriteLine("🧪 Test Send Bulk Messages with 2 messages...");
            Message[] messages = new Message[] {
                CreateValidMessage("Send Bulk Message Test (First Message)"),
                CreateValidMessage("Send Bulk Message Test (Second Message)")
            };

            SendBulkMessagesResponse bulkResponse = paubox.SendBulkMessages(messages);

            if (bulkResponse.Messages.Count != 2)
            {
                throw new Exception("❌ Unexpected number of messages (expected 2): " + bulkResponse.Messages.Count);
            }

            foreach (BulkMessageResponse message in bulkResponse.Messages)
            {
                if (message.SourceTrackingId == null)
                {
                    throw new Exception("❌ No tracking ID returned for the message.");
                }
            }

            Console.WriteLine("✅ Bulk messages sent successfully.");
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

        static void TestCreateTemplate(EmailLibrary paubox, string templateName)
        {
            Console.WriteLine("🧪 Creating a new Dynamic Template named " + templateName + "...");

            DynamicTemplateResponse createResult = paubox.CreateDynamicTemplate(templateName, templatePath);

            if (createResult.Error != null)
            {
                throw new Exception("❌ Failed to create the template: " + createResult.Error);
            }

            if (createResult.Message != $"Template {templateName} created!")
            {
                throw new Exception("❌ Unexpected message: " + createResult.Message);
            }

            if (createResult.Params.Name != templateName)
            {
                throw new Exception("❌ Unexpected template name in Params.Name: " + createResult.Params.Name);
            }

            if (createResult.Params.Body.Tempfile == null)
            {
                throw new Exception("❌ Unexpected Params.Body.Tempfile: " + createResult.Params.Body.Tempfile);
            }

            if (createResult.Params.Body.Headers == null)
            {
                throw new Exception("❌ Unexpected Params.Body.Headers: " + createResult.Params.Body.Headers);
            }

            if (createResult.Params.Body.OriginalFilename != "ExampleTemplate.hbs")
            {
                throw new Exception("❌ Unexpected Params.Body.OriginalFilename: " + createResult.Params.Body.OriginalFilename);
            }

            if (createResult.Params.Body.ContentType != "text/x-handlebars-template")
            {
                throw new Exception("❌ Unexpected Params.Body.ContentType: " + createResult.Params.Body.ContentType);
            }

            Console.WriteLine("✅ Template created successfully.");
        }

        static int TestListTemplates(EmailLibrary paubox, string templateName)
        {
            Console.WriteLine("🧪 Listing all Dynamic Templates...");
            List<DynamicTemplateSummary> listResult = paubox.ListDynamicTemplates();

            if (listResult.Count == 0)
            {
                throw new Exception("❌ No templates found in the list.");
            }

            int templateId = listResult.FirstOrDefault(t => t.Name == templateName)?.Id ?? 0;
            if (templateId == 0)
            {
                throw new Exception("❌ Failed to find the new template in the list.");
            }

            Console.WriteLine("✅ Templates listed and we found the new template (id " + templateId + ") in the list.");
            return templateId;
        }

        static void TestGetTemplate(EmailLibrary paubox, int templateId, string templateName)
        {
            Console.WriteLine("🧪 Getting the new Template (id = " + templateId + ")...");
            GetDynamicTemplateResponse getResult = paubox.GetDynamicTemplate(templateId);

            if (getResult.Error != null)
            {
                throw new Exception("❌ Error when getting the template: " + getResult.Error);
            }

            if (getResult.Id != templateId)
            {
                throw new Exception("❌ Unexpected template ID: " + getResult.Id);
            }

            if (getResult.Name != templateName)
            {
                throw new Exception("❌ Unexpected template name: " + getResult.Name);
            }

            if (getResult.Body != File.ReadAllText(templatePath))
            {
                throw new Exception("❌ Unexpected template body: " + getResult.Body);
            }

            if (getResult.CreatedAt == null)
            {
                throw new Exception("❌ Unexpected CreatedAt: " + getResult.CreatedAt);
            }

            if (getResult.UpdatedAt == null)
            {
                throw new Exception("❌ Unexpected UpdatedAt: " + getResult.UpdatedAt);
            }

            Console.WriteLine("✅ Template retrieved successfully.");
        }

        static string TestUpdateTemplate(EmailLibrary paubox, int templateId, string templateName)
        {
            Console.WriteLine("🧪 Updating the Template with ID " + templateId + "...");

            string newTemplateName = "Updated " + templateName;

            DynamicTemplateResponse updateResult = paubox.UpdateDynamicTemplate(templateId, newTemplateName, templatePath);

            if (updateResult.Error != null)
            {
                throw new Exception("❌ Error when updating the template: " + updateResult.Error);
            }

            if (updateResult.Message != $"Template {newTemplateName} updated!")
            {
                throw new Exception("❌ Unexpected message: " + updateResult.Message);
            }

            if (updateResult.Params.Name != newTemplateName)
            {
                throw new Exception("❌ Unexpected template name: " + updateResult.Params.Name);
            }

            if (updateResult.Params.Body.Tempfile == null)
            {
                throw new Exception("❌ Unexpected Params.Body.Tempfile: " + updateResult.Params.Body.Tempfile);
            }

            if (updateResult.Params.Body.Headers == null)
            {
                throw new Exception("❌ Unexpected Params.Body.Headers: " + updateResult.Params.Body.Headers);
            }

            if (updateResult.Params.Body.OriginalFilename != "ExampleTemplate.hbs")
            {
                throw new Exception("❌ Unexpected Params.Body.OriginalFilename: " + updateResult.Params.Body.OriginalFilename);
            }

            if (updateResult.Params.Body.ContentType != "text/x-handlebars-template")
            {
                throw new Exception("❌ Unexpected Params.Body.ContentType: " + updateResult.Params.Body.ContentType);
            }

            Console.WriteLine("✅ Template updated successfully.");

            return newTemplateName;
        }

        static void TestDeleteTemplate(EmailLibrary paubox, int templateId, string updatedTemplateName)
        {
            Console.WriteLine("🧪 Deleting the Template with ID " + templateId + "...");
            DeleteDynamicTemplateResponse deleteResult = paubox.DeleteDynamicTemplate(templateId);

            if (deleteResult.Error != null)
            {
                throw new Exception("❌ Error when deleting the template: " + deleteResult.Error);
            }

            if (deleteResult.Message != $"Template {updatedTemplateName} deleted!")
            {
                throw new Exception("❌ Unexpected message: " + deleteResult.Message);
            }

            Console.WriteLine("✅ Template deleted successfully.");
        }

        static void CleanUpTemplates(EmailLibrary paubox)
        {
            Console.WriteLine("🧹 Cleaning up all templates...");
            List<DynamicTemplateSummary> listResult = paubox.ListDynamicTemplates();

            foreach (DynamicTemplateSummary template in listResult)
            {
                DeleteDynamicTemplateResponse deleteResult = paubox.DeleteDynamicTemplate(template.Id);

                if (deleteResult.Error != null)
                {
                    throw new Exception("❌ Error when deleting the template: " + deleteResult.Error);
                }
                else {
                    Console.WriteLine("✅ Template " + template.Name + " deleted successfully.");
                }
            }

            Console.WriteLine("✅ All templates cleaned up.");
        }
    }
}
