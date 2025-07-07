using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class DeleteDynamicTemplateTest
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
    public void CanSuccessfullyDeleteATemplate()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        string templateId = "123";
        DeleteDynamicTemplateResponse result = _emailLibrary.DeleteDynamicTemplate(templateId);

        Assert.IsNotNull(result);
        Assert.AreEqual("Template Example Template deleted!", result.Message);
    }

    [Test]
    public void ThrowsAnExceptionWhenTheTemplateIsNotFound()
    {
        string apiResponse = NotFoundResponse();
        MockApiResponse(apiResponse);

        string templateId = "-15";

        var exception = Assert.Throws<SystemException>(() => _emailLibrary.DeleteDynamicTemplate(templateId));

        Assert.IsNotNull(exception.Message);
        Assert.IsTrue(exception.Message.Length > 0);
        StringAssert.Contains("Couldn't find DynamicTemplate", exception.Message);
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
                "DELETE",
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new DeleteDynamicTemplateResponse
        {
            Message = "Template Example Template deleted!"
        });
    }

    private string NotFoundResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["Error"] = "Couldn't find DynamicTemplate with 'id'=-15 [WHERE \"dynamic_templates\".\"api_customer_id\" = $1]"
        });
    }
}
