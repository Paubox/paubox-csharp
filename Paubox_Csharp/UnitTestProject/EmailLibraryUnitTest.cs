using EmailLib;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace UnitTestProject
{
    [TestFixture]
    public class EmailLibraryUnitTest
    {
        public class GetTestDt
        {
            string CSVFileName = @"E:\Dropbox\GENESYSBUZZ\Paubox REST API project\SendMessage_TestData.xlsx";
            public int input { get; set; }
            public int output { get; set; }
            //public List<Message> GetTestData()
            //{
            //    Message objTestData = new Message();
            //    List<Message> objListTestData = new List<Message>();
            //    using (var inputStream = new FileStream(CSVFileName, FileMode.Open, FileAccess.Read))
            //        {
            //            using (var streamReader = new StreamReader(inputStream))
            //            {
            //                string inputLine;
            //                while ((inputLine = streamReader.ReadLine()) != null)
            //                {
            //                    var data = inputLine.Split(',');
            //                    List<int> param = new List<int>();
            //                    for (int i = 0; i < data.Length; i++)
            //                    {
            //                        param.Add(Convert.ToInt32(data[i]));
            //                    }

            //                    yield return new[] { param.ToArray() };
            //                }
            //            }
            //        }
                
            //}
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
    }
}
