using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ListReceivingDomainsTest
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
    public void TestListReceivingDomainsReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        ReceivingDomainListResponse result = _receivingLibrary.ListReceivingDomains();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
        Assert.AreEqual(1, result.Data[0].Id);
        Assert.AreEqual("acme", result.Data[0].Slug);
        Assert.AreEqual("acme.paubox.net", result.Data[0].Domain);
        Assert.IsTrue(result.Data[0].Verified);
    }

    [Test]
    public void TestListReceivingDomainsSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.ListReceivingDomains();

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestListReceivingDomainsEmptyListReturnsEmptyData()
    {
        string apiResponse = EmptyResponse();
        MockApiResponse(apiResponse);

        ReceivingDomainListResponse result = _receivingLibrary.ListReceivingDomains();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Data.Count);
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

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["data"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["id"] = 1,
                    ["slug"] = "acme",
                    ["domain"] = "acme.paubox.net",
                    ["verified"] = true,
                    ["created_at"] = "2026-01-15T10:00:00Z",
                    ["updated_at"] = "2026-01-15T10:00:00Z"
                },
                new Dictionary<string, object>
                {
                    ["id"] = 2,
                    ["slug"] = "widgets",
                    ["domain"] = "widgets.paubox.net",
                    ["verified"] = false,
                    ["created_at"] = "2026-02-20T14:30:00Z",
                    ["updated_at"] = "2026-02-20T14:30:00Z"
                }
            }
        });
    }

    private string EmptyResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["data"] = new List<object>()
        });
    }
}
