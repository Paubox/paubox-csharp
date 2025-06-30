using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetEmailDispositionTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private EmailLibrary _emailLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _emailLibrary = new EmailLibrary("testApiKey", "testApiUser", _mockApiHelper.Object);
    }

    [Test]
    public void TestGetEmailDispositionHappyCaseReturnsTheSourceTrackingIdAndData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        string sourceTrackingId = "6e1cf9a4-7bde-4834-8200-ed424b50c8a7";
        GetEmailDispositionResponse result = _emailLibrary.GetEmailDisposition(sourceTrackingId);

        Assert.IsNotNull(result);
        Assert.AreEqual("6e1cf9a4-7bde-4834-8200-ed424b50c8a7", result.SourceTrackingId);
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data.Message);
        Assert.AreEqual("<6e1cf9a4-7bde-4834-8200-ed424b50c8a7@authorized_domain.com>", result.Data.Message.Id);
        Assert.IsNull(result.Errors);
    }

    [Test]
    public void TestGetEmailDispositionNotFoundThrowsSystemException()
    {
        string apiResponse = NotFoundResponse();
        MockApiResponse(apiResponse);

        string sourceTrackingId = "6e1cf9a4-7bde-4834-8200-ed424b50c8a7";
        var exception = Assert.Throws<SystemException>(() => _emailLibrary.GetEmailDisposition(sourceTrackingId));

        Assert.IsNotNull(exception.Message);
        Assert.IsTrue(exception.Message.Length > 0);
        StringAssert.Contains("Message was not found", exception.Message);
        StringAssert.Contains("Message with this tracking id was not found", exception.Message);
    }

    [Test]
    public void TestGetEmailDispositionEmptyResponseThrowsSystemException()
    {
        string apiResponse = EmptyResponse();
        MockApiResponse(apiResponse);

        string sourceTrackingId = "6e1cf9a4-7bde-4834-8200-ed424b50c8a7";
        var exception = Assert.Throws<SystemException>(() => _emailLibrary.GetEmailDisposition(sourceTrackingId));

        Assert.IsNotNull(exception);
    }

    // ------------------------------------------------------------
    // Helper methods
    //
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

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["SourceTrackingId"] = "6e1cf9a4-7bde-4834-8200-ed424b50c8a7",
            ["Data"] = new Dictionary<string, object>
            {
                ["Message"] = new Dictionary<string, object>
                {
                    ["Id"] = "<6e1cf9a4-7bde-4834-8200-ed424b50c8a7@authorized_domain.com>",
                    ["MessageDeliveries"] = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            ["Recipient"] = "recipient@host.com",
                            ["Status"] = new Dictionary<string, object>
                            {
                                ["DeliveryStatus"] = "delivered",
                                ["DeliveryTime"] = "Mon, 23 Apr 2018 13:27:34 -0700",
                                ["OpenedStatus"] = "opened",
                                ["OpenedTime"] = "Mon, 23 Apr 2018 13:27:51 -0700"
                            }
                        }
                    }
                }
            }
        });
    }

    private string NotFoundResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["SourceTrackingId"] = "6e1cf9a4-7bde-4834-8200-ed424b50c8a7",
            ["Errors"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["Code"] = 404,
                    ["Title"] = "Message was not found",
                    ["Details"] = "Message with this tracking id was not found"
                }
            }
        });
    }

    private string EmptyResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["SourceTrackingId"] = null,
            ["Data"] = null,
            ["Errors"] = null
        });
    }
}
