using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class UpdateDynamicTemplateTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private EmailLibrary _emailLibrary;
    private string _projectDir;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _emailLibrary = new EmailLibrary("testApiKey", _mockApiHelper.Object);

        _projectDir = Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory);
        while (_projectDir != null && !Directory.Exists(Path.Combine(_projectDir, "Fixtures")))
        {
            _projectDir = Path.GetDirectoryName(_projectDir);
        }
    }

    [Test]
    public void TestCanSuccessfullyUpdateBothTheFileAndTheName()
    {
        string apiResponse = SuccessUpdateFileAndNameResponse();
        MockApiResponse(apiResponse);

        int templateId = 123;
        string templateName = "Updated Example Template";
        string templatePath = Path.Combine(_projectDir, "Fixtures", "ExampleTemplate.hbs");
        DynamicTemplateResponse result = _emailLibrary.UpdateDynamicTemplate(templateId, templateName, templatePath);

        Assert.IsNotNull(result);
        Assert.AreEqual(123, result.Id);
        Assert.AreEqual("Updated Example Template", result.Name);
        Assert.AreEqual("<p>Bye {{name}}</p>", result.Body);
        Assert.IsNull(result.Error);
    }

    [Test]
    public void CanSuccessfullyUpdateTheTemplateNameOnly()
    {
        string apiResponse = SuccessUpdateNameOnlyResponse();
        MockApiResponse(apiResponse);

        int templateId = 123;
        string templateName = "Updated Example Template";

        DynamicTemplateResponse result = _emailLibrary.UpdateDynamicTemplate(templateId, templateName, null);

        Assert.IsNotNull(result);
        Assert.AreEqual(123, result.Id);
        Assert.AreEqual("Updated Example Template", result.Name);
        Assert.IsNull(result.Error);
    }

    [Test]
    public void ThrowsAnExceptionWhenTheTemplateIsNotFound()
    {
        string apiResponse = NotFoundErrorResponse();
        MockApiResponse(apiResponse);

        int templateId = -15;
        string templateName = "Updated Example Template";
        string templatePath = Path.Combine(_projectDir, "Fixtures", "ExampleTemplate.hbs");

        var exception = Assert.Throws<SystemException>(() => _emailLibrary.UpdateDynamicTemplate(templateId, templateName, templatePath));

        Assert.IsNotNull(exception.Message);
        Assert.IsTrue(exception.Message.Length > 0);
        StringAssert.Contains("Couldn't find DynamicTemplate", exception.Message);
    }

    [Test]
    public void ThrowsAnExceptionWhenTheTemplateNameIsNotProvided()
    {
        string apiResponse = ValidationErrorResponse();
        MockApiResponse(apiResponse);

        int templateId = 123;
        string templateName = "";
        string templatePath = Path.Combine(_projectDir, "Fixtures", "ExampleTemplate.hbs");

        var exception = Assert.Throws<SystemException>(() => _emailLibrary.UpdateDynamicTemplate(templateId, templateName, templatePath));

        Assert.IsNotNull(exception.Message);
        Assert.IsTrue(exception.Message.Length > 0);
        StringAssert.Contains("Validation failed: Name can't be blank", exception.Message);
    }

    // ------------------------------------------------------------
    // Helper methods
    //
    private void MockApiResponse(string response)
    {
        _mockApiHelper.Setup(
            x => x.UploadTemplate(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "PATCH",
                It.IsAny<string>(),
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessUpdateFileAndNameResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object> { ["id"] = 123, ["api_customer_id"] = 56789, ["name"] = "Updated Example Template", ["body"] = "<p>Bye {{name}}</p>", ["metadata"] = null, ["created_at"] = "2026-10-13T17:02:11.123Z", ["updated_at"] = "2026-10-13T17:02:11.123Z" });
    }

    private string SuccessUpdateNameOnlyResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object> { ["id"] = 123, ["api_customer_id"] = 56789, ["name"] = "Updated Example Template", ["body"] = "<p>Hello {{name}}</p>", ["metadata"] = null, ["created_at"] = "2026-10-13T17:02:11.123Z", ["updated_at"] = "2026-10-13T17:02:11.123Z" });
    }

    private string ValidationErrorResponse()
    {
        return JsonConvert.SerializeObject(new DynamicTemplateResponse
        {
            Error = "Validation failed: Name can't be blank",
        });
    }

    private string NotFoundErrorResponse()
    {
        return JsonConvert.SerializeObject(new DynamicTemplateResponse
        {
            Error = "Couldn't find DynamicTemplate with 'id'=-15 [WHERE \"dynamic_templates\".\"api_customer_id\" = $1]",
        });
    }
}
