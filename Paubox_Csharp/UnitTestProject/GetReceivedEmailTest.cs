using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class GetReceivedEmailTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private ReceivingLibrary _receivingLibrary;

    private const string EmailId = "0192f0c4-0000-7000-8000-000000000001";
    private const string AttachmentId = "0192f0c4-0000-7000-8000-0000000000a1";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _receivingLibrary = new ReceivingLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestGetReceivedEmailReturnsData()
    {
        MockApiResponse(DetailResponse());

        ReceivedEmailResponse result = _receivingLibrary.GetReceivedEmail(EmailId);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        ReceivedEmail email = result.Data;
        Assert.AreEqual(EmailId, email.Id);
        Assert.AreEqual(1, email.FromAddresses.Count);
        Assert.AreEqual("Sender Name", email.FromAddresses[0].Name);
        Assert.AreEqual("sender@example.com", email.FromAddresses[0].Address);
        Assert.AreEqual(2, email.ToAddresses.Count);
        Assert.IsNull(email.ToAddresses[0].Name);
        Assert.AreEqual("info@acme.paubox.net", email.ToAddresses[0].Address);
        Assert.AreEqual("Support", email.ToAddresses[1].Name);
        Assert.AreEqual(1, email.CcAddresses.Count);
        Assert.AreEqual("cc@example.com", email.CcAddresses[0].Address);
        Assert.AreEqual("Test email", email.Subject);
        Assert.AreEqual("2026-03-15T05:00:00-05:00", email.Date);
        Assert.AreEqual(new DateTime(2026, 3, 15, 10, 0, 5, DateTimeKind.Utc), email.ReceivedAt.ToUniversalTime());
        CollectionAssert.AreEqual(new List<string> { "<abc123@example.com>" }, email.MessageId);
        CollectionAssert.AreEqual(new List<string> { "<parent@example.com>" }, email.InReplyTo);
        CollectionAssert.AreEqual(new List<string> { "<root@example.com>", "<parent@example.com>" }, email.References);
        Assert.IsFalse(email.Spam);
        Assert.AreEqual(1.5, email.SpamScore);
        Assert.AreEqual("Hello world", email.BodyText);
        Assert.AreEqual("<p>Hello world</p>", email.BodyHtml);
        Assert.AreEqual(208000, email.Size);
        Assert.AreEqual("pass", email.Authentication.Spf);
        Assert.AreEqual("pass", email.Authentication.Dkim);
        Assert.AreEqual("fail", email.Authentication.Dmarc);
        Assert.AreEqual("acme.paubox.net", email.Domain);
        Assert.AreEqual(2, email.Headers.Count);
        Assert.AreEqual("X-Mailer", email.Headers[1].Name);
        Assert.AreEqual("ExampleMailer 1.0", email.Headers[1].Value);
    }

    [Test]
    public void TestGetReceivedEmailSendsCorrectRequest()
    {
        MockApiResponse(DetailResponse());

        _receivingLibrary.GetReceivedEmail(EmailId);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == $"receiving/{EmailId}"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestGetReceivedEmailWithAttachments()
    {
        MockApiResponse(DetailResponse());

        ReceivedEmailResponse result = _receivingLibrary.GetReceivedEmail(EmailId);

        Assert.IsNotNull(result.Data.Attachments);
        Assert.AreEqual(1, result.Data.Attachments.Count);
        ReceivedAttachment attachment = result.Data.Attachments[0];
        Assert.AreEqual(AttachmentId, attachment.Id);
        Assert.AreEqual("report.pdf", attachment.Filename);
        Assert.AreEqual("application/pdf", attachment.ContentType);
        Assert.AreEqual(204800, attachment.Size);
        Assert.AreEqual("report@example.com", attachment.ContentId);
        Assert.AreEqual($"https://api.paubox.com/v1/email/receiving/{EmailId}/attachments/{AttachmentId}", attachment.DownloadUrl);
    }

    [Test]
    public void TestGetReceivedEmailToleratesNullableFields()
    {
        MockApiResponse(NullableFieldsResponse());

        ReceivedEmailResponse result = _receivingLibrary.GetReceivedEmail(EmailId);

        ReceivedEmail email = result.Data;
        Assert.AreEqual(EmailId, email.Id);
        Assert.IsNull(email.FromAddresses[0].Name);
        Assert.IsNull(email.FromAddresses[0].Address);
        Assert.IsNull(email.Subject);
        Assert.IsNull(email.Date);
        Assert.AreEqual(default(DateTime), email.ReceivedAt);
        Assert.IsNull(email.MessageId);
        Assert.IsNull(email.InReplyTo);
        Assert.IsNull(email.References);
        Assert.IsTrue(email.Spam);
        Assert.IsNull(email.SpamScore);
        Assert.IsNull(email.BodyText);
        Assert.IsNull(email.BodyHtml);
        Assert.IsNull(email.Size);
        Assert.IsNull(email.Headers);
        Assert.AreEqual(0, email.CcAddresses.Count);

        ReceivedAttachment attachment = email.Attachments[0];
        Assert.AreEqual(AttachmentId, attachment.Id);
        Assert.IsNull(attachment.Filename);
        Assert.IsNull(attachment.ContentType);
        Assert.AreEqual(0, attachment.Size);
        Assert.IsNull(attachment.ContentId);
    }

#pragma warning disable CS0618
    [Test]
    public void TestObsoleteAddressAccessorsReadFromAddressObjects()
    {
        MockApiResponse(DetailResponse());

        ReceivedEmail email = _receivingLibrary.GetReceivedEmail(EmailId).Data;

        Assert.AreEqual("sender@example.com", email.From);
        CollectionAssert.AreEqual(new List<string> { "info@acme.paubox.net", "support@acme.paubox.net" }, email.To);
        CollectionAssert.AreEqual(new List<string> { "cc@example.com" }, email.Cc);
    }

    [Test]
    public void TestObsoleteAddressSettersWriteAddressObjects()
    {
        var email = new ReceivedEmail
        {
            From = "sender@example.com",
            To = new List<string> { "info@acme.paubox.net" },
            Cc = null
        };

        Assert.AreEqual("sender@example.com", email.FromAddresses[0].Address);
        Assert.AreEqual("info@acme.paubox.net", email.ToAddresses[0].Address);
        Assert.IsNull(email.CcAddresses);
    }
#pragma warning restore CS0618

    [Test]
    public void TestGetReceivedEmailThrowsWhenIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.GetReceivedEmail(""));
    }

    [Test]
    public void TestGetReceivedEmailThrowsWhenIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.GetReceivedEmail(null));
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "GET",
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private static string DetailResponse()
    {
        return $$"""
        {
          "data": {
            "email_id": "{{EmailId}}",
            "from": [{ "name": "Sender Name", "address": "sender@example.com" }],
            "to": [
              { "name": null, "address": "info@acme.paubox.net" },
              { "name": "Support", "address": "support@acme.paubox.net" }
            ],
            "cc": [{ "name": null, "address": "cc@example.com" }],
            "subject": "Test email",
            "date": "2026-03-15T05:00:00-05:00",
            "received_at": "2026-03-15T10:00:05Z",
            "message_id": ["<abc123@example.com>"],
            "in_reply_to": ["<parent@example.com>"],
            "references": ["<root@example.com>", "<parent@example.com>"],
            "spam": false,
            "spam_score": 1.5,
            "text_body": "Hello world",
            "html_body": "<p>Hello world</p>",
            "attachments": [
              {
                "id": "{{AttachmentId}}",
                "filename": "report.pdf",
                "content_type": "application/pdf",
                "size": 204800,
                "content_id": "report@example.com",
                "download_url": "https://api.paubox.com/v1/email/receiving/{{EmailId}}/attachments/{{AttachmentId}}"
              }
            ],
            "size": 208000,
            "authentication": { "spf": "pass", "dkim": "pass", "dmarc": "fail" },
            "domain": "acme.paubox.net",
            "headers": [
              { "name": "Subject", "value": "Test email" },
              { "name": "X-Mailer", "value": "ExampleMailer 1.0" }
            ]
          }
        }
        """;
    }

    private static string NullableFieldsResponse()
    {
        return $$"""
        {
          "data": {
            "email_id": "{{EmailId}}",
            "from": [{ "name": null, "address": null }],
            "to": [{ "name": null, "address": "info@acme.paubox.net" }],
            "cc": [],
            "subject": null,
            "date": null,
            "received_at": null,
            "message_id": null,
            "in_reply_to": null,
            "references": null,
            "spam": true,
            "spam_score": null,
            "text_body": null,
            "html_body": null,
            "attachments": [
              {
                "id": "{{AttachmentId}}",
                "filename": null,
                "content_type": null,
                "size": null,
                "content_id": null,
                "download_url": "https://api.paubox.com/v1/email/receiving/{{EmailId}}/attachments/{{AttachmentId}}"
              }
            ],
            "size": null,
            "authentication": { "spf": "none", "dkim": "none", "dmarc": "none" },
            "domain": "acme.paubox.net",
            "headers": null
          }
        }
        """;
    }
}
