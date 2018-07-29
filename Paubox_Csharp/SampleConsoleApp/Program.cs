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
            EmailLib.EmailLibrary.GetEmailDisposition("ce1e2143-474d-43ba-b829-17a26b8005e5");

            //Message message = new Message();
            //message.Recipients = new string[] { "vighneshtrivedi2004@gmail.com" };

            //Content content = new Content();
            //Header header = new Header();
            //Attachment attachment = new Attachment();
            //List<Attachment> listAttachments = new List<Attachment>();
            //attachment.FileName = "hello_world.txt";
            //attachment.ContentType = "text/plain";
            //attachment.Content = "SGVsbG8gV29ybGQh\n";

            //listAttachments.Add(attachment);


            //header.Subject = "Test Mail from C#";
            //header.From = "renee@undefeatedgames.com";
            //content.PlainText = "Hello Again";

            //message.Header = header;
            //message.Content = content;
            //message.Attachments = listAttachments;

            //EmailLib.EmailLibrary.SendMessage(message);
        }


    }
}
