using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class DeleteReceivingDomainTest
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
    public void TestDeleteReceivingDomainReturnsMessage()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        DeleteReceivingDomainResponse result = _receivingLibrary.DeleteReceivingDomain(1);

        Assert.IsNotNull(result);
        Assert.AreEqual("Domain deleted", result.Message);
    }

    [Test]
    public void TestDeleteReceivingDomainSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.DeleteReceivingDomain(42);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains/42"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "DELETE"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "DELETE",
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["message"] = "Domain deleted"
        });
    }
}
