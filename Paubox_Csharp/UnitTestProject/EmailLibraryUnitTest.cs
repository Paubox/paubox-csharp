using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class EmailLibraryUnitTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private EmailLibrary _emailLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _emailLibrary = new EmailLibrary(_mockApiHelper.Object);
        // Set up static config if needed
        EmailLibrary.Initialize("testUser", "testKey");
    }

    [Test]
    public void TestSendMessage_ReturnSuccess()
    {
        // Arrange
        var testMsg = new Message { /* ... fill with test data ... */ };
        var fakeApiResponse = "{\"SourceTrackingId\":\"abc123\",\"Data\":\"some data\",\"Errors\":null}";
        _mockApiHelper.Setup(x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "POST", It.IsAny<string>()))
                      .Returns(fakeApiResponse);

        // Act
        var result = _emailLibrary.SendMessageInstance(testMsg);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("abc123", result.SourceTrackingId);
    }
}
