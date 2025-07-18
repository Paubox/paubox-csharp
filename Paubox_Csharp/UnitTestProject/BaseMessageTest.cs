using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Paubox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


[TestFixture]
public class BaseMessageTest
{
    [Test]
    public void TestSecureNotificationValueIsSetCorrectlyWhenRepresentedAsJson()
    {
        Assert.AreEqual(null, CreateValidBaseMessage(null).ToJson()["forceSecureNotification"]);
        Assert.AreEqual(null, CreateValidBaseMessage("").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(null, CreateValidBaseMessage(" ").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(null, CreateValidBaseMessage("Unknown").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(null, CreateValidBaseMessage("  Unknown  ").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(null, CreateValidBaseMessage("  UNKNOWN  ").ToJson()["forceSecureNotification"]);

        Assert.AreEqual(true, (bool)CreateValidBaseMessage("true").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(true, (bool)CreateValidBaseMessage("TRUE").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(true, (bool)CreateValidBaseMessage("True").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(true, (bool)CreateValidBaseMessage("  True  ").ToJson()["forceSecureNotification"]);

        Assert.AreEqual(false, (bool)CreateValidBaseMessage("false").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(false, (bool)CreateValidBaseMessage("FALSE").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(false, (bool)CreateValidBaseMessage("False").ToJson()["forceSecureNotification"]);
        Assert.AreEqual(false, (bool)CreateValidBaseMessage("  False ").ToJson()["forceSecureNotification"]);
    }

    // ------------------------------------------------------------
    // Helper methods
    //
    private BaseMessage CreateValidBaseMessage(string? forceSecureNotificationValue = null)
    {
        BaseMessage baseMessage = new BaseMessage();

        Header header = new Header() {
            Subject = "Test Email",
            From = "you@yourdomain.com",
            ReplyTo = "reply-to@yourdomain.com",
        };
        baseMessage.Header = header;

        baseMessage.ForceSecureNotification = forceSecureNotificationValue;

        return baseMessage;
    }
}
