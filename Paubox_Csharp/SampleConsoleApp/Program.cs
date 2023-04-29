using Newtonsoft.Json;
using Paubox;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            WebHook();

            CreateDynamicTemplate();
            string trackingId = SendMessage().Result;
            GetEmailDispositionResponse response = EmailLibrary.GetEmailDisposition(trackingId).Result;
            Console.ReadLine();
        }

        static async Task<string> SendMessage()
        {
            Message message = new Message();
            message.Recipients = new string[] { "username@domain.com" };

            Content content = new Content();
            Header header = new Header();
            Attachment attachment = new Attachment();
            List<Attachment> listAttachments = new List<Attachment>();
            attachment.FileName = "hello_world.txt";
            attachment.ContentType = "text/plain";
            attachment.Content = "SGVsbG8gV29ybGQh\n";

            listAttachments.Add(attachment);


            header.Subject = "Test Mail from C#";
            header.From = "renee@undefeatedgames.com";
            content.PlainText = "Hello Again";

            message.Header = header;
            message.Content = content;
            message.Attachments = listAttachments;

            SendMessageResponse response = await EmailLibrary.SendMessage(message);

            return response.SourceTrackingId;
        }
        static async Task CreateDynamicTemplate()
        {
            var tempFilePath = ConfigurationManager.AppSettings["TemplateFilePath"];

            if (!File.Exists(tempFilePath))
            {
                File.WriteAllText(tempFilePath, "Test File");
            }

            var DynamicTemplate = new DynamicTemplateLibrary();
            var responseCreated = await DynamicTemplate.CreateDynamicTemplate(new DynamicTemplateRequest()
            {
                TemplateName = "Test Template",
                TemplatePath = tempFilePath
            });

            Console.WriteLine(JsonConvert.SerializeObject(responseCreated));

            var getAll = await DynamicTemplate.GetAllDynamicTemplate();

            Console.WriteLine($"dynamic templates count: {getAll.Count}");

            var lastID = getAll.Last().id;

            var responseSingle = await DynamicTemplate.GetDynamicTemplate(lastID);
            Console.WriteLine(JsonConvert.SerializeObject(responseSingle));

            var responseUpdated = await DynamicTemplate.UpdateDynamicTemplate(responseSingle.id, new DynamicTemplateRequest()
            {
                TemplateName = "Updated Test Template",
                TemplatePath = tempFilePath
            });
            Console.WriteLine(JsonConvert.SerializeObject(responseUpdated));

            var responseDeleted = await DynamicTemplate.DeleteDynamicTemplate(responseSingle.id);
            Console.WriteLine(JsonConvert.SerializeObject(responseDeleted));
        }

        static async Task WebHook()
        {
            var webhookLibrary = new WebhookLibrary();
            var responseCreated = await webhookLibrary.CreateWebhookEndpoint(new WebhookEndpointRequest()
            {
                target_url = "https://example.com",
                events = new[] { "api_mail_log_delivered" },
                active = true,
                signing_key = "Abc",
                api_key = "abc"
            });

            Console.WriteLine(JsonConvert.SerializeObject(responseCreated));

            var getAll = await webhookLibrary.GetAllWebhookEndpoints();

            Console.WriteLine($"Webhook endpoints count: {getAll.Count}");

            var responseSingle = await webhookLibrary.GetWebhookEndpoint(responseCreated.data.id);
            Console.WriteLine(JsonConvert.SerializeObject(responseSingle));

            var responseUpdated = await webhookLibrary.UpdateWebhookEndpoint(responseSingle.id, new WebhookEndpointRequest()
            {
                target_url = "https://example.com",
                events = new[] { "api_mail_log_delivered" },
                active = true,
                signing_key = "Abc",
                api_key = "abc"
            });
            Console.WriteLine(JsonConvert.SerializeObject(responseUpdated));

            var responseDeleted = await webhookLibrary.DeleteWebhookEndpoint(responseSingle.id);
            Console.WriteLine(JsonConvert.SerializeObject(responseDeleted));
        }
    }
}
