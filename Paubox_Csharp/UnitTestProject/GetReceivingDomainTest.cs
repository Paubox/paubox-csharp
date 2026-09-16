using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetReceivingDomainTest
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
    public void TestGetReceivingDomainReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        ReceivingDomainResponse result = _receivingLibrary.GetReceivingDomain(1);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Id);
        Assert.AreEqual("acme", result.Data.Slug);
        Assert.AreEqual("acme.paubox.net", result.Data.Domain);
        Assert.IsTrue(result.Data.Verified);
    }

    [Test]
    public void TestGetReceivingDomainSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.GetReceivingDomain(42);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains/42"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestGetReceivingDomainIncludesDnsRecords()
    {
        string apiResponse = ResponseWithDnsRecords();
        MockApiResponse(apiResponse);

        ReceivingDomainResponse result = _receivingLibrary.GetReceivingDomain(1);

        Assert.IsNotNull(result.Data.DnsRecords);
        Assert.AreEqual(1, result.Data.DnsRecords.Count);
        Assert.AreEqual("MX", result.Data.DnsRecords[0].Type);
        Assert.AreEqual("acme.paubox.net", result.Data.DnsRecords[0].Name);
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
            ["data"] = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["slug"] = "acme",
                ["domain"] = "acme.paubox.net",
                ["verified"] = true,
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T10:00:00Z"
            }
        });
    }

    private string ResponseWithDnsRecords()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["data"] = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["slug"] = "acme",
                ["domain"] = "acme.paubox.net",
                ["verified"] = false,
                ["dns_records"] = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["type"] = "MX",
                        ["name"] = "acme.paubox.net",
                        ["value"] = "mx.paubox.net",
                        ["priority"] = 10
                    }
                },
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T10:00:00Z"
            }
        });
    }
}
