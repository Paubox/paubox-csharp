using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Paubox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


[TestFixture]
public class TestTemplatedMessage
{
    [Test]
    public void TestCanSuccessfullyCreateTemplatedMessageBySettingProperties()
    {
        TemplatedMessage message = new TemplatedMessage();

        // Set some root-level properties:
        message.TemplateName = "Example Template";
        message.TemplateData = new Dictionary<string, string> {
            { "first_name", "John" },
            { "last_name", "Doe" }
        };

        message.Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" };
        message.Cc = new string[] { "cc-recipient@domain.com" };
        message.Bcc = new string[] { "bcc-recipient@domain.com" };
        message.AllowNonTLS = true;
        message.ForceSecureNotification = "true";

        // Set the header:
        Header header = new Header();
        header.Subject = "Test Email";
        header.From = "you@yourdomain.com";
        header.ReplyTo = "reply-to@yourdomain.com";
        header.CustomHeaders = new Dictionary<string, string> {
            { "X-Custom-Header", "Custom Value" },
            { "X-Another-Header", "Another Value" }
        };
        message.Header = header;

        // Set the attachments:
        List<Attachment> attachments = new List<Attachment>();
        Attachment attachment = new Attachment();
        attachment.FileName = "hello_world.txt";
        attachment.ContentType = "text/plain";
        attachment.Content = "SGVsbG8gV29ybGQh\n";
        attachments.Add(attachment);
        message.Attachments = attachments;

        Assert.IsNotNull(message);
        Assert.AreEqual("Example Template", message.TemplateName);
        Assert.AreEqual(2, message.TemplateData.Count);
        Assert.AreEqual("John", message.TemplateData["first_name"]);
        Assert.AreEqual("Doe", message.TemplateData["last_name"]);
        Assert.AreEqual(2, message.Recipients.Length);
        Assert.AreEqual("cc-recipient@domain.com", message.Cc[0]);
        Assert.AreEqual("bcc-recipient@domain.com", message.Bcc[0]);
        Assert.IsTrue(message.AllowNonTLS);
        Assert.AreEqual("true", message.ForceSecureNotification);
        Assert.IsNotNull(message.Header);
        Assert.AreEqual("Test Email", message.Header.Subject);
        Assert.AreEqual("you@yourdomain.com", message.Header.From);
        Assert.AreEqual("reply-to@yourdomain.com", message.Header.ReplyTo);
        Assert.AreEqual(2, message.Header.CustomHeaders.Count);
        Assert.AreEqual("Custom Value", message.Header.CustomHeaders["X-Custom-Header"]);
        Assert.AreEqual("Another Value", message.Header.CustomHeaders["X-Another-Header"]);
    }

    [Test]
    public void TestCanSuccessfullyCreateTemplatedMessageByUsingObjectInitializer()
    {
        TemplatedMessage message = CreateValidMessage();

        Assert.IsNotNull(message);
        Assert.AreEqual("Example Template", message.TemplateName);
        Assert.AreEqual(2, message.TemplateData.Count);
        Assert.AreEqual("John", message.TemplateData["first_name"]);
        Assert.AreEqual("Doe", message.TemplateData["last_name"]);
        Assert.AreEqual(2, message.Recipients.Length);
        Assert.AreEqual("cc-recipient@domain.com", message.Cc[0]);
        Assert.AreEqual("bcc-recipient@domain.com", message.Bcc[0]);
        Assert.IsTrue(message.AllowNonTLS);
        Assert.AreEqual("true", message.ForceSecureNotification);
        Assert.IsNotNull(message.Header);
        Assert.AreEqual("Test Email", message.Header.Subject);
        Assert.AreEqual("you@yourdomain.com", message.Header.From);
        Assert.AreEqual("reply-to@yourdomain.com", message.Header.ReplyTo);
        Assert.AreEqual(2, message.Header.CustomHeaders.Count);
        Assert.AreEqual("Custom Value", message.Header.CustomHeaders["X-Custom-Header"]);
        Assert.AreEqual("Another Value", message.Header.CustomHeaders["X-Another-Header"]);
    }

    [Test]
    public void ToJsonReturnsTheExpectedJson()
    {
        TemplatedMessage message = CreateValidMessage();
        JObject messageJSON = message.ToJson();

        JObject expectedJSON = new JObject
        {
            ["recipients"] = new JArray { "someone@domain.com", "someoneelse@domain.com" },
            ["cc"] = new JArray { "cc-recipient@domain.com" },
            ["bcc"] = new JArray { "bcc-recipient@domain.com" },
            ["allowNonTLS"] = true,
            ["forceSecureNotification"] = true,
            ["headers"] = new JObject
            {
                ["subject"] = "Test Email",
                ["from"] = "you@yourdomain.com",
                ["reply-to"] = "reply-to@yourdomain.com",
                ["X-Custom-Header"] = "Custom Value",
                ["X-Another-Header"] = "Another Value"
            },
            ["attachments"] = new JArray
            {
                new JObject
                {
                    ["fileName"] = "hello_world.txt",
                    ["contentType"] = "text/plain",
                    ["content"] = "SGVsbG8gV29ybGQh\n"
                }
            }
        };

        Assert.IsTrue(
            JToken.DeepEquals(messageJSON, expectedJSON),
            "JSON objects are not equal. Actual: " + messageJSON.ToString() + "\nExpected: " + expectedJSON.ToString()
        );
    }

    // ------------------------------------------------------------
    // Helper methods
    //
    private TemplatedMessage CreateValidMessage()
    {
        return new TemplatedMessage() {
            TemplateName = "Example Template",
            TemplateData = new Dictionary<string, string> {
                { "first_name", "John" },
                { "last_name", "Doe" }
            },
            Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" },
            Cc = new string[] { "cc-recipient@domain.com" },
            Bcc = new string[] { "bcc-recipient@domain.com" },
            AllowNonTLS = true,
            ForceSecureNotification = "true",
            Header = new Header() {
                Subject = "Test Email",
                From = "you@yourdomain.com",
                ReplyTo = "reply-to@yourdomain.com",
                CustomHeaders = new Dictionary<string, string> {
                    { "X-Custom-Header", "Custom Value" },
                    { "X-Another-Header", "Another Value" }
                }
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
