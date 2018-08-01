using EmailLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string trackingId = SendMessage();
            GetEmailDispositionResponse response = EmailLib.EmailLibrary.GetEmailDisposition(trackingId);
        }

        static string SendMessage()
        {
            Message message = new Message();
            message.Recipients = new string[] { "vighneshtrivedi2004@gmail.com" };

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

            SendMessageResponse response = EmailLib.EmailLibrary.SendMessage(message);

            return response.SourceTrackingId;
        }

    }
}
