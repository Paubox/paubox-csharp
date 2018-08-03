using EmailLib;
using System.Collections.Generic;

namespace SampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string trackingId = SendMessage();
            GetEmailDispositionResponse response = EmailLibrary.GetEmailDisposition(trackingId);
        }

        static string SendMessage()
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

            SendMessageResponse response = EmailLibrary.SendMessage(message);

            return response.SourceTrackingId;
        }

    }
}
