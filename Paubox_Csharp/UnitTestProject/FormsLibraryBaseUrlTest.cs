using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class FormsLibraryBaseUrlTest
{
    private const string StagingBaseUrl = "https://apx.staging.paubox.com/forms/";

    [Test]
    public void ExplicitBaseUrlOverridesProdDefault()
    {
        var mock = new Mock<IAPIHelper>();
        string capturedBaseUrl = null;
        mock.Setup(x => x.CallToAPI(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => capturedBaseUrl = baseUrl)
            .Returns(EmptyResults());

        var library = new FormsLibrary(mock.Object, "test-key", StagingBaseUrl);
        library.ListForms(new FormsListParams { CustomerId = 20602 });

        Assert.AreEqual(StagingBaseUrl, capturedBaseUrl);
    }

    [Test]
    public void MissingTrailingSlashInBaseUrlIsAppended()
    {
        var mock = new Mock<IAPIHelper>();
        string capturedBaseUrl = null;
        mock.Setup(x => x.CallToAPI(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => capturedBaseUrl = baseUrl)
            .Returns(EmptyResults());

        var library = new FormsLibrary(mock.Object, "test-key", "https://apx.staging.paubox.com/forms");
        library.ListForms(new FormsListParams { CustomerId = 20602 });

        Assert.AreEqual(StagingBaseUrl, capturedBaseUrl);
    }

    [Test]
    public void DefaultConstructorUsesProdBaseUrl()
    {
        var mock = new Mock<IAPIHelper>();
        string capturedBaseUrl = null;
        mock.Setup(x => x.CallToAPI(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => capturedBaseUrl = baseUrl)
            .Returns(EmptyResults());

        var library = new FormsLibrary(mock.Object, "test-key");
        library.ListForms(new FormsListParams { CustomerId = 20147 });

        Assert.AreEqual("https://api.paubox.com/v1/forms/", capturedBaseUrl);
    }

    private static string EmptyResults()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["results"] = new object[0],
            ["page_info"] = new Dictionary<string, object> { ["count"] = 0, ["pages"] = 0, ["page"] = 1, ["items"] = 0 }
        });
    }
}
