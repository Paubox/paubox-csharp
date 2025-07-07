using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class CreateDynamicTemplateTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private EmailLibrary _emailLibrary;
    private string _projectDir;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _emailLibrary = new EmailLibrary("testApiKey", "testApiUser", _mockApiHelper.Object);

        _projectDir = Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory);
        while (_projectDir != null && !Directory.Exists(Path.Combine(_projectDir, "Fixtures")))
        {
            _projectDir = Path.GetDirectoryName(_projectDir);
        }
    }

    [Test]
    public void TestCanSuccessfullyCreateADynamicTemplate()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        string templateName = "Example Template";
        string templatePath = Path.Combine(_projectDir, "Fixtures", "ExampleTemplate.hbs");
        CreateDynamicTemplateResponse result = _emailLibrary.CreateDynamicTemplate(templateName, templatePath);

        Assert.IsNotNull(result);
        Assert.AreEqual("Template Example Template created!", result.Message);
        Assert.AreEqual("Example Template", result.Params.Name);
        Assert.AreEqual("ExampleTemplate.hbs", result.Params.Body.OriginalFilename);
        Assert.AreEqual("text/x-handlebars-template", result.Params.Body.ContentType);
        Assert.AreEqual("Content-Disposition: form-data; name=\"data[body]\"; filename=\"Example Template.hbs\"\r\nContent-Type: text/x-handlebars-template\r\n", result.Params.Body.Headers);
    }

    [Test]
    public void TestThrowsExceptionForValidationErrors()
    {
        string apiResponse = ValidationErrorResponse();
        MockApiResponse(apiResponse);

        string templateName = "";
        string templatePath = Path.Combine(_projectDir, "Fixtures", "ExampleTemplate.hbs");

        Assert.Throws<SystemException>(() => _emailLibrary.CreateDynamicTemplate(templateName, templatePath));
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
                "POST",
                It.IsAny<string>(),
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new CreateDynamicTemplateResponse
        {
            Message = "Template Example Template created!",
            Params = new CreateDynamicTemplateParams
            {
                Name = "Example Template",
                Body = new CreateDynamicTemplateParamsBody {
                    Tempfile = "...",
                    OriginalFilename = "ExampleTemplate.hbs",
                    ContentType = "text/x-handlebars-template",
                    Headers = "Content-Disposition: form-data; name=\"data[body]\"; filename=\"Example Template.hbs\"\r\nContent-Type: text/x-handlebars-template\r\n"
                }
            }
        });
    }

    private string ValidationErrorResponse()
    {
        return JsonConvert.SerializeObject(new CreateDynamicTemplateResponse
        {
            Error = "Validation failed: Name can't be blank",
        });
    }
}
