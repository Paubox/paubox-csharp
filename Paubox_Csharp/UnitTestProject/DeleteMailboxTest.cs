using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class DeleteMailboxTest
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
    public void TestDeleteMailboxReturnsMessage()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        DeleteMailboxResponse result = _receivingLibrary.DeleteMailbox(1, 10);

        Assert.IsNotNull(result);
        Assert.AreEqual("Mailbox deleted", result.Message);
    }

    [Test]
    public void TestDeleteMailboxSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.DeleteMailbox(42, 99);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains/42/mailboxes/99"),
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
            ["message"] = "Mailbox deleted"
        });
    }
}
