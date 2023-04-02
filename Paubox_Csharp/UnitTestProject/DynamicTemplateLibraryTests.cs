using Paubox;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestFixture]
    public class DynamicTemplateLibraryTests
    {
        private readonly DynamicTemplateLibrary _library;
        private readonly string tempFilePath = ConfigurationManager.AppSettings["TemplateFilePath"];

        public DynamicTemplateLibraryTests()
        {
            _library = new DynamicTemplateLibrary();

            if (!File.Exists(tempFilePath))
            {
                File.WriteAllText(tempFilePath, "Test File");
            }
        }

        [Test]
        public async Task CreateDynamicTemplate_ValidTemplateRequest_ReturnsCreateDynamicTemplateResponse()
        {
            // Arrange
            var templateRequest = new DynamicTemplateRequest()
            {
                TemplateName = "test",
                TemplatePath = tempFilePath
            };

            // Act
            var result = await _library.CreateDynamicTemplate(templateRequest);

            // Assert
            Assert.IsAssignableFrom<CreateDynamicTemplateResponse>(result);
        }

        [Test]
        public async Task GetAllDynamicTemplate_ValidCall_ReturnsListOfDynamicTemplateAllResponse()
        {
            // Act
            var result = await _library.GetAllDynamicTemplate();

            // Assert
            Assert.IsAssignableFrom<List<DynamicTemplateAllResponse>>(result);
        }

        [Test]
        public async Task GetDynamicTemplate_ValidID_ReturnsDynamicTemplateResponse()
        {
            // Arrange
            var ID = 1;

            // Act
            var result = await _library.GetDynamicTemplate(ID);

            // Assert
            Assert.IsAssignableFrom<DynamicTemplateResponse>(result);
        }

        [Test]
        public async Task DeleteDynamicTemplate_ValidID_ReturnsDeleteDynamicTemplateResponse()
        {
            // Arrange
            var ID = 1;

            // Act
            var result = await _library.DeleteDynamicTemplate(ID);

            // Assert
            Assert.IsAssignableFrom<DeleteDynamicTemplateResponse>(result);
        }

        [Test]
        public async Task UpdateDynamicTemplate_ValidIDAndTemplateRequest_ReturnsCreateDynamicTemplateResponse()
        {
            // Arrange
            var ID = 1;
            var templateRequest = new DynamicTemplateRequest()
            {
                TemplateName = "Updated Test",
                TemplatePath = tempFilePath
            };

            // Act
            var result = await _library.UpdateDynamicTemplate(ID, templateRequest);

            // Assert
            Assert.IsAssignableFrom<CreateDynamicTemplateResponse>(result);
        }
    }
}
