using NUnit.Framework;
using Paubox;

[TestFixture]
public class PauboxApiExceptionTest
{
    [Test]
    public void ExposesStatusVerbEndpointAndBody()
    {
        var ex = new PauboxApiException(403, "GET", "api/forms?customer_id=20147", "{\"error\":\"forbidden\"}");

        Assert.AreEqual(403, ex.StatusCode);
        Assert.AreEqual("GET", ex.Verb);
        Assert.AreEqual("api/forms?customer_id=20147", ex.Endpoint);
        Assert.AreEqual("{\"error\":\"forbidden\"}", ex.Body);
    }

    [Test]
    public void MessageIsVerbEndpointAndStatusOnly()
    {
        // Body content — potentially submitter-supplied — must NOT land in Message,
        // which structured loggers and error reporters (Sentry/App Insights/etc.)
        // capture by default. Keeping the body on a dedicated property makes it opt-in.
        string body = "{\"submitter_email\":\"patient@example.com\",\"error\":\"forbidden\"}";
        var ex = new PauboxApiException(403, "GET", "api/forms/stats", body);

        Assert.AreEqual("GET api/forms/stats -> 403", ex.Message);
        StringAssert.DoesNotContain("patient@example.com", ex.Message);
        StringAssert.DoesNotContain("error", ex.Message);
    }
}
