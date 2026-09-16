using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class DownloadAttachmentTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private ReceivingLibrary _receivingLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _receivingLibrary = new ReceivingLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestDownloadAttachmentReturnsResponseBody()
    {
        string expectedContent = "raw-attachment-content";
        MockApiResponse(expectedContent);

        string result = _receivingLibrary.DownloadAttachment("msg-001", "blob-abc");

        Assert.AreEqual(expectedContent, result);
    }

    [Test]
    public void TestDownloadAttachmentSendsCorrectRequest()
    {
        MockApiResponse("content");

        _receivingLibrary.DownloadAttachment("msg-001", "blob-abc");

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/msg-001/attachments/blob-abc"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestDownloadAttachmentThrowsWhenEmailIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachment("", "blob-abc"));
    }

    [Test]
    public void TestDownloadAttachmentThrowsWhenEmailIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachment(null, "blob-abc"));
    }

    [Test]
    public void TestDownloadAttachmentThrowsWhenBlobIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachment("msg-001", ""));
    }

    [Test]
    public void TestDownloadAttachmentThrowsWhenBlobIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachment("msg-001", null));
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
}
