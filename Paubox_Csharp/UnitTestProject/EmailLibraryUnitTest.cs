using Paubox;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace UnitTestProject
{
    [TestFixture]
    public class EmailLibraryUnitTest
    {
        public class TestData
        {
            static string csvFileName;

            static TestData()
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                csvFileName = configuration["CSVTestDataFilePath"];
            }

            /// <summary>
            /// Get Test data of Messages for Successful Scenarios
            /// </summary>
            /// <returns>List of Messages</returns>
            public static List<Message> GetTestDataForSuccess()
            {
                Message objTestData = null;
                List<Message> objListTestData = new List<Message>();
                using (var inputStream = new FileStream(csvFileName, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(inputStream))
                    {
                        string inputLine;
                        inputLine = streamReader.ReadLine();// For Reading Columns
                        while ((inputLine = streamReader.ReadLine()) != null)
                        {
                            var data = inputLine.Split(',');
                            if (data[15] != "SUCCESS") // If Expected output is not Success , then skip the test data
                                continue;

                            objTestData = new Message();
                            Content content = new Content();
                            Header header = new Header();
                            Attachment attachment = new Attachment();
                            List<Attachment> listAttachments = new List<Attachment>();

                            objTestData.Recipients = new string[] { data[1] };
                            objTestData.Bcc = new string[] { data[2] };
                            objTestData.Cc = new string[] { data[14] };

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

                            objTestData.ForceSecureNotification = data[13];

                            objTestData.Header = header;
                            objTestData.Content = content;
                            objTestData.Attachments = listAttachments;
                            objListTestData.Add(objTestData);
                        }
                    }

                }
                return objListTestData;
            }

            /// <summary>
            /// Get Test data of Messages for Failure Scenarios
            /// </summary>
            /// <returns>List of Messages</returns>
            public static List<Message> GetTestDataForFailure()
            {
                Message objTestData = null;
                List<Message> objListTestData = new List<Message>();
                using (var inputStream = new FileStream(csvFileName, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(inputStream))
                    {
                        string inputLine;
                        inputLine = streamReader.ReadLine();// For Reading Columns
                        while ((inputLine = streamReader.ReadLine()) != null)
                        {
                            var data = inputLine.Split(',');
                            if (data[15] != "ERROR") // If Expected output is not Error , then skip the test data
                                continue;

                            objTestData = new Message();
                            Content content = new Content();
                            Header header = new Header();
                            Attachment attachment = new Attachment();
                            List<Attachment> listAttachments = new List<Attachment>();

                            objTestData.Recipients = new string[] { data[1] };
                            objTestData.Bcc = new string[] { data[2] };
                            objTestData.Cc = new string[] { data[14] };

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

                            objTestData.ForceSecureNotification = data[13];

                            objTestData.Header = header;
                            objTestData.Content = content;
                            objTestData.Attachments = listAttachments;
                            objListTestData.Add(objTestData);
                        }
                    }
                }
                return objListTestData;
            }

            ///// <summary>
            ///// Get Test data of Messages for Exception Scenarios
            ///// </summary>
            ///// <returns>List of Messages</returns>
            //public static List<Message> GetTestDataForException()
            //{
            //    Message objTestData = null;
            //    List<Message> objListTestData = new List<Message>();
            //    using (var inputStream = new FileStream(csvFileName, FileMode.Open, FileAccess.Read))
            //    {
            //        using (var streamReader = new StreamReader(inputStream))
            //        {
            //            string inputLine;
            //            inputLine = streamReader.ReadLine();// For Reading Columns
            //            while ((inputLine = streamReader.ReadLine()) != null)
            //            {
            //                var data = inputLine.Split(',');
            //                if (data[13] != "EXCEPTION") // If Expected output is not Error , then skip the test data
            //                    continue;

            //                objTestData = new Message();
            //                Content content = new Content();
            //                Header header = new Header();
            //                Attachment attachment = new Attachment();
            //                List<Attachment> listAttachments = new List<Attachment>();

            //                objTestData.Recipients = new string[] { data[1] };
            //                objTestData.Bcc = new string[] { data[2] };

            //                if (string.IsNullOrWhiteSpace(data[3]) && string.IsNullOrWhiteSpace(data[4]) && string.IsNullOrWhiteSpace(data[5]))
            //                {
            //                    header = null;
            //                }
            //                else
            //                {
            //                    header.Subject = data[3];
            //                    header.From = data[4];
            //                    header.ReplyTo = data[5];
            //                }

            //                if (!string.IsNullOrWhiteSpace(data[6]))
            //                    objTestData.AllowNonTLS = bool.Parse(data[6]);

            //                if (string.IsNullOrWhiteSpace(data[7]))
            //                    content.PlainText = null;
            //                else
            //                    content.PlainText = data[7];

            //                if (string.IsNullOrWhiteSpace(data[8]))
            //                    content.HtmlText = null;
            //                else
            //                    content.HtmlText = data[8];

            //                if(content.HtmlText == null && content.PlainText == null)
            //                {
            //                    content = null;
            //                }


            //                if (Convert.ToInt32(data[9]) > 0)
            //                {
            //                    attachment.FileName = data[10];
            //                    attachment.ContentType = data[11];
            //                    attachment.Content = data[12];

            //                    listAttachments.Add(attachment);
            //                }

            //                objTestData.Header = header;
            //                objTestData.Content = content;
            //                objTestData.Attachments = listAttachments;
            //                objListTestData.Add(objTestData);
            //            }
            //        }
            //    }
            //    return objListTestData;
            //}
        }

        #region Unit Tests for Get Email Disposition Method

        /// <summary>
        /// Test for Get Email Disposition method to check if all test cases return successful response
        /// </summary>
        /// <param name="sourceTrackingId"></param>
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

        /// <summary>
        /// Test for Get Email Disposition method to check if all test cases return failure response
        /// </summary>
        /// <param name="sourceTrackingId"></param>
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

        #endregion Unit Tests for Get Email Disposition Method

        #region Unit Tests for Send Message Method

        /// <summary>
        /// Test for Send Message method to check if all test cases return successful response
        /// </summary>
        /// <param name="testMsg"></param>
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

        /// <summary>
        /// Test for Send Message method to check if all test cases return failure response
        /// </summary>
        /// <param name="testMsg"></param>
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
