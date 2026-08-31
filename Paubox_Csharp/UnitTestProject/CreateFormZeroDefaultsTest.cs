using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Moq;
using Paubox;

/// <summary>
/// Locks in the fix for the "CreateFormRequest silently sends zero-defaults for
/// primitives" gate finding. Before: <c>Active</c>, <c>Version</c>, <c>SubmissionCount</c>
/// were non-nullable value types with no NullValueHandling, so every create
/// serialized as <c>"active":false, "version":0, "submission_count":0</c> whether
/// the caller intended those values or not. After: all four are nullable and the
/// class carries <c>[JsonObject(ItemNullValueHandling.Ignore)]</c>.
/// </summary>
[TestFixture]
public class CreateFormZeroDefaultsTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-key");
    }

    [Test]
    public void OmitsUnsetPrimitivesFromRequestBody()
    {
        string capturedBody = null;
        _mockApiHelper.Setup(x => x.CallToAPI(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => capturedBody = body)
            .Returns("{\"id\":\"550e8400-e29b-41d4-a716-446655440000\"}");

        _formsLibrary.CreateForm(new CreateFormRequest
        {
            Title = "QA form",
            FormJson = new { fields = new object[0] },
            CustomerId = 20147
        });

        Assert.IsNotNull(capturedBody);
        JObject body = JObject.Parse(capturedBody);

        // Set fields present
        Assert.AreEqual("QA form", (string)body["title"]);
        Assert.AreEqual(20147, (int)body["customer_id"]);

        // Unset primitives omitted — no leaked zeros/false
        Assert.IsFalse(body.ContainsKey("active"), "active should be omitted when unset");
        Assert.IsFalse(body.ContainsKey("version"), "version should be omitted when unset");
        Assert.IsFalse(body.ContainsKey("submission_count"), "submission_count should be omitted when unset");
        Assert.IsFalse(body.ContainsKey("signable"), "signable should be omitted when unset");
    }

    [Test]
    public void SetsCustomerIdRequiredMissingThrowsArgumentException()
    {
        Assert.Throws<System.ArgumentException>(
            () => _formsLibrary.CreateForm(new CreateFormRequest
            {
                Title = "QA form",
                FormJson = new { fields = new object[0] }
                // CustomerId deliberately unset
            }));

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never());
    }
}
