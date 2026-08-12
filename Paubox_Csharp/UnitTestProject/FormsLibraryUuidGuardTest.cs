using System;
using NUnit.Framework;
using Moq;
using Paubox;

/// <summary>
/// Exercises the UUID guard on every method that interpolates a caller-supplied id
/// into a URL path segment. `CopyForm` is deliberately not swept — its `formId` goes
/// in the JSON body, not the URL, so the URL-injection class doesn't apply there.
/// </summary>
[TestFixture]
public class FormsLibraryUuidGuardTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-key");
    }

    private static readonly string[] HostileIds =
    {
        "../../etc/passwd",
        "550e8400-e29b-41d4-a716-446655440000?admin=1",
        "550e8400-e29b-41d4-a716-446655440000#frag",
        "550e8400/archive",
        "not-a-uuid-at-all",
        "",
        "   ",
    };

    [Test]
    public void GetFormRejectsHostileFormId()
    {
        foreach (var id in HostileIds)
            Assert.Throws<ArgumentException>(() => _formsLibrary.GetForm(id), "GetForm accepted: " + id);
        VerifyNoHttpCall();
    }

    [Test]
    public void GetFormByIdRejectsHostileFormId()
    {
        foreach (var id in HostileIds)
            Assert.Throws<ArgumentException>(() => _formsLibrary.GetFormById(id));
        VerifyNoHttpCall();
    }

    [Test]
    public void UpdateFormRejectsHostileFormId()
    {
        foreach (var id in HostileIds)
            Assert.Throws<ArgumentException>(
                () => _formsLibrary.UpdateForm(id, new UpdateFormRequest { Title = "x" }));
        VerifyNoHttpCall();
    }

    [Test]
    public void ArchiveFormRejectsHostileFormId()
    {
        foreach (var id in HostileIds)
            Assert.Throws<ArgumentException>(() => _formsLibrary.ArchiveForm(id));
        VerifyNoHttpCall();
    }

    [Test]
    public void UnarchiveFormRejectsHostileFormId()
    {
        foreach (var id in HostileIds)
            Assert.Throws<ArgumentException>(() => _formsLibrary.UnarchiveForm(id));
        VerifyNoHttpCall();
    }

    [Test]
    public void ListFormSubmissionsRejectsHostileFormId()
    {
        foreach (var id in HostileIds)
            Assert.Throws<ArgumentException>(() => _formsLibrary.ListFormSubmissions(id));
        VerifyNoHttpCall();
    }

    [Test]
    public void ExportSubmissionsCsvRejectsHostileFormId()
    {
        foreach (var id in HostileIds)
            Assert.Throws<ArgumentException>(() => _formsLibrary.ExportSubmissionsCsv(id));
        VerifyNoHttpCall();
    }

    [Test]
    public void ExportSubmissionCsvRejectsHostileIds()
    {
        string validFormId = "550e8400-e29b-41d4-a716-446655440000";
        string validSubmissionId = "11111111-2222-3333-4444-555555555555";

        foreach (var id in HostileIds)
        {
            Assert.Throws<ArgumentException>(() => _formsLibrary.ExportSubmissionCsv(id, validSubmissionId));
            Assert.Throws<ArgumentException>(() => _formsLibrary.ExportSubmissionCsv(validFormId, id));
        }
        VerifyNoHttpCall();
    }

    [Test]
    public void ExportSubmissionPdfRejectsHostileIds()
    {
        string validFormId = "550e8400-e29b-41d4-a716-446655440000";
        string validSubmissionId = "11111111-2222-3333-4444-555555555555";

        foreach (var id in HostileIds)
        {
            Assert.Throws<ArgumentException>(() => _formsLibrary.ExportSubmissionPdf(id, validSubmissionId));
            Assert.Throws<ArgumentException>(() => _formsLibrary.ExportSubmissionPdf(validFormId, id));
        }
        VerifyNoHttpCallBytes();
    }

    private void VerifyNoHttpCall()
    {
        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never());
    }

    private void VerifyNoHttpCallBytes()
    {
        _mockApiHelper.Verify(x => x.CallToAPIBytes(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }
}
