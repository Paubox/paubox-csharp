using System;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class DownloadAttachmentTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private ReceivingLibrary _receivingLibrary;

    private const string EmailId = "0192f0c4-0000-7000-8000-000000000001";
    private const string AttachmentId = "0192f0c4-0000-7000-8000-0000000000a1";

    private static readonly byte[] BinaryContent = { 0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF, 0xFE, 0x0D, 0x0A, 0x1A };

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _receivingLibrary = new ReceivingLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestDownloadAttachmentBytesReturnsRawBytes()
    {
        MockBinaryResponse(BinaryContent);

        byte[] result = _receivingLibrary.DownloadAttachmentBytes(EmailId, AttachmentId);

        Assert.AreEqual(BinaryContent, result);
    }

    [Test]
    public void TestDownloadAttachmentBytesSendsCorrectRequest()
    {
        MockBinaryResponse(BinaryContent);

        _receivingLibrary.DownloadAttachmentBytes(EmailId, AttachmentId);

        _mockApiHelper.Verify(
            x => x.CallToAPIBinary(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == $"receiving/{EmailId}/attachments/{AttachmentId}"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET")
            ),
            Times.Once
        );
    }

    [Test]
    public void TestDownloadAttachmentBytesDoesNotParseOrDecodeTheBody()
    {
        MockBinaryResponse(BinaryContent);

        _receivingLibrary.DownloadAttachmentBytes(EmailId, AttachmentId);

        _mockApiHelper.Verify(
            x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
        _mockApiHelper.Verify(
            x => x.CallToAPIBytes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Test]
    public void TestDownloadAttachmentBytesPropagatesApiErrors()
    {
        _mockApiHelper.Setup(
            x => x.CallToAPIBinary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "GET")
        ).Throws(new PauboxApiException(404, "GET", $"receiving/{EmailId}/attachments/{AttachmentId}", "{\"error\":\"attachment not found\"}"));

        var ex = Assert.Throws<PauboxApiException>(() => _receivingLibrary.DownloadAttachmentBytes(EmailId, AttachmentId));
        Assert.AreEqual(404, ex.StatusCode);
    }

    [Test]
    public void TestDownloadAttachmentBytesThrowsWhenEmailIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachmentBytes("", AttachmentId));
    }

    [Test]
    public void TestDownloadAttachmentBytesThrowsWhenEmailIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachmentBytes(null, AttachmentId));
    }

    [Test]
    public void TestDownloadAttachmentBytesThrowsWhenAttachmentIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachmentBytes(EmailId, ""));
    }

    [Test]
    public void TestDownloadAttachmentBytesThrowsWhenAttachmentIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachmentBytes(EmailId, null));
    }

    [Test]
    public void TestDownloadAttachmentIsObsolete()
    {
        var method = typeof(ReceivingLibrary).GetMethod("DownloadAttachment", new[] { typeof(string), typeof(string) });

        Assert.IsNotNull(method);
        Assert.IsNotNull(Attribute.GetCustomAttribute(method, typeof(ObsoleteAttribute)));
        Assert.IsNotNull(Attribute.GetCustomAttribute(
            typeof(IReceivingLibrary).GetMethod("DownloadAttachment", new[] { typeof(string), typeof(string) }),
            typeof(ObsoleteAttribute)));
    }

#pragma warning disable CS0618
    [Test]
    public void TestLegacyDownloadAttachmentStillAcceptsNamedArguments()
    {
        MockStringResponse("content");

        string result = _receivingLibrary.DownloadAttachment(emailId: EmailId, blobId: AttachmentId);

        Assert.AreEqual("content", result);
        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == $"receiving/{EmailId}/attachments/{AttachmentId}"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestLegacyDownloadAttachmentThrowsWhenEmailIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachment("", AttachmentId));
    }

    [Test]
    public void TestLegacyDownloadAttachmentThrowsWhenAttachmentIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.DownloadAttachment(EmailId, null));
    }
#pragma warning restore CS0618

    private void MockBinaryResponse(byte[] response)
    {
        _mockApiHelper.Setup(
            x => x.CallToAPIBinary(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "GET"
            )
        ).Returns(response);
    }

    private void MockStringResponse(string response)
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
