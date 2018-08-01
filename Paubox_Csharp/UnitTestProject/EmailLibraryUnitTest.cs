using EmailLib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace UnitTestProject
{
    [TestFixture]
    public class EmailLibraryUnitTest
    {
        public class TestData
        {
            static string CSVFileName = @"E:\Paubox REST API project\SendMessage_TestData.csv"; 
            public static List<Message> GetTestDataForSuccess()
            {
                Message objTestData = null;
                List<Message> objListTestData = new List<Message>();
                using (var inputStream = new FileStream(CSVFileName, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(inputStream))
                    {
                        string inputLine;
                        inputLine = streamReader.ReadLine();// For Reading Columns
                        while ((inputLine = streamReader.ReadLine()) != null)
                        {
                            var data = inputLine.Split(',');
                            if (data[13] != "SUCCESS")
                                continue;

                            objTestData = new Message();
                            Content content = new Content();
                            Header header = new Header();
                            Attachment attachment = new Attachment();
                            List<Attachment> listAttachments = new List<Attachment>();

                            objTestData.Recipients = new string[] { data[1] };
                            objTestData.Bcc = new string[] { data[2] };

                            header.Subject = data[3];
                            header.From = data[4];
                            header.ReplyTo = data[5];

                            if (!string.IsNullOrWhiteSpace(data[6]))
                                objTestData.AllowNonTLS = bool.Parse(data[6]);

                            if (string.IsNullOrWhiteSpace(data[7]))
                                content.PlainText = null;
                            else
                                content.PlainText = data[7];

                            if (string.IsNullOrWhiteSpace(data[8]))
                                content.HtmlText = null;
                            else
                                content.HtmlText = data[8];


                            if (Convert.ToInt32(data[9]) > 0)
                            {
                                attachment.FileName = data[10];
                                attachment.ContentType = data[11];
                                attachment.Content = data[12];

                                listAttachments.Add(attachment);
                            }

                            objTestData.Header = header;
                            objTestData.Content = content;
                            objTestData.Attachments = listAttachments;                            
                            objListTestData.Add(objTestData);
                        }
                    }

                }
                return objListTestData;
            }

            public static List<Message> GetTestDataForFailure()
            {
                Message objTestData = null;
                List<Message> objListTestData = new List<Message>();
                using (var inputStream = new FileStream(CSVFileName, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(inputStream))
                    {
                        string inputLine;
                        inputLine = streamReader.ReadLine();// For Reading Columns
                        while ((inputLine = streamReader.ReadLine()) != null)
                        {
                            var data = inputLine.Split(',');
                            if (data[13] != "ERROR")
                                continue;

                            objTestData = new Message();
                            Content content = new Content();
                            Header header = new Header();
                            Attachment attachment = new Attachment();
                            List<Attachment> listAttachments = new List<Attachment>();

                            objTestData.Recipients = new string[] { data[1] };
                            objTestData.Bcc = new string[] { data[2] };

                            header.Subject = data[3];
                            header.From = data[4];
                            header.ReplyTo = data[5];

                            if (!string.IsNullOrWhiteSpace(data[6]))
                                objTestData.AllowNonTLS = bool.Parse(data[6]);

                            if (string.IsNullOrWhiteSpace(data[7]))
                                content.PlainText = null;
                            else
                                content.PlainText = data[7];

                            if (string.IsNullOrWhiteSpace(data[8]))
                                content.HtmlText = null;
                            else
                                content.HtmlText = data[8];


                            if (Convert.ToInt32(data[9]) > 0)
                            {
                                attachment.FileName = data[10];
                                attachment.ContentType = data[11];
                                attachment.Content = data[12];

                                listAttachments.Add(attachment);
                            }

                            objTestData.Header = header;
                            objTestData.Content = content;
                            objTestData.Attachments = listAttachments;
                            objListTestData.Add(objTestData);
                        }
                    }
                }
                return objListTestData;
            }
        }

        #region Unit Tests for Get Email Disposition Method

        [Test, TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("151515215")]
        public void TestGetEmailDisposition_ReturnError(string sourceTrackingId)
        {

            var actualResponse = EmailLibrary.GetEmailDisposition(sourceTrackingId);
            if (actualResponse == null || actualResponse.Errors == null || actualResponse.Errors.Count <= 0)
            {
                Assert.Fail();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(actualResponse.Errors[0].Title))
                {
                    Assert.Fail();
                }
                else
                {
                    Assert.Pass();
                }
            }
        }

        [Test, TestCase("1aed91d1-f7ce-4c3d-8df2-85ecd225a7fc")]
        [TestCase("ce1e2143-474d-43ba-b829-17a26b8005e5")]
        public void TestGetEmailDisposition_ReturnSuccess(string sourceTrackingId)
        {

            var actualResponse = EmailLibrary.GetEmailDisposition(sourceTrackingId);
            if (actualResponse == null || actualResponse.Data == null || actualResponse.Data.Message == null || string.IsNullOrWhiteSpace(actualResponse.Data.Message.Id))
            {
                Assert.Fail();
            }
            else if (actualResponse.Errors != null && actualResponse.Errors.Count > 0)
            {
                Assert.Fail();
            }
            else if (actualResponse.Data.Message.Message_Deliveries != null && actualResponse.Data.Message.Message_Deliveries.Count > 0)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        #endregion Unit Tests for Get Email Disposition Method

        #region Unit Tests for Send Message Method

        [Test, TestCaseSource(typeof(TestData), "GetTestDataForSuccess")]
        public void TestSendMessage_ReturnSuccess(Message testMsg)
        {           
            var actualResponse = EmailLibrary.SendMessage(testMsg);
            if (actualResponse == null || actualResponse.Data == null || actualResponse.SourceTrackingId == null)
            {
                Assert.Fail();
            }
            else if (actualResponse.Errors != null && actualResponse.Errors.Count > 0)
            {
                Assert.Pass();
            }


        }

        [Test, TestCaseSource(typeof(TestData), "GetTestDataForFailure")]
        public void TestSendMessage_ReturnError(Message testMsg)
        {
            var actualResponse = EmailLibrary.SendMessage(testMsg);
            if (actualResponse != null)
            {
                if (actualResponse.Errors != null && actualResponse.Errors.Count > 0)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail();
                }
            }
            else 
            {
                Assert.Fail();
            }
        }

        #endregion Unit Tests for Send Message Method
    }


}
