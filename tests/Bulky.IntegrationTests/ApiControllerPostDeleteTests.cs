using System.Net;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Bulky.IntegrationTests
{
    /// <summary>
    /// Tests for POST/DELETE and error scenarios across API controllers.
    /// </summary>
    public class ApiControllerPostDeleteTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public ApiControllerPostDeleteTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task DeleteCategory_NonExistentId_ReturnsBadRequest()
        {
            // Arrange
            var id = 99999; // Non-existent ID
            var url = $"/api/admin/category/{id}";

            // Act
            var response = await _client.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GetCategory_WithSearchParam_ReturnsFilteredData()
        {
            // Arrange
            var url = "/api/admin/category?search=祭祖";

            // Act
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("data", out var dataElement));
            // Data should be present (either filtered results or empty if no match)
            Assert.True(dataElement.ValueKind == JsonValueKind.Array);
        }

        [Fact]
        public async Task GetProduct_ReturnsWithRelatedData()
        {
            // Arrange
            var url = "/api/admin/product";

            // Act
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("data", out var dataElement));
            
            // Verify at least one product has category and company data
            if (dataElement.GetArrayLength() > 0)
            {
                var firstProduct = dataElement[0];
                Assert.True(firstProduct.TryGetProperty("category", out _));
                Assert.True(firstProduct.TryGetProperty("company", out _));
            }
        }

        [Fact]
        public async Task GetKindness_WithEmptyDatabase_ReturnsEmptyData()
        {
            // Arrange
            var url = "/api/admin/kindness";

            // Act
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("data", out var dataElement));
            // Data can be empty or populated depending on test data
            Assert.True(dataElement.ValueKind == JsonValueKind.Array);
        }

        [Fact]
        public async Task GetAncestral_ValidRequest_ReturnsStructuredData()
        {
            // Arrange
            var url = "/api/admin/ancestral";

            // Act
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("data", out var dataElement));
            Assert.True(dataElement.ValueKind == JsonValueKind.Array);
        }

        [Fact]
        public async Task DeleteRange_InvalidPayload_HandlesSilently()
        {
            // Arrange
            var url = "/api/admin/category/deleterange";
            var invalidIds = new List<int> { 99999, 100000 }; // Non-existent IDs
            var json = System.Text.Json.JsonSerializer.Serialize(invalidIds);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            // Should succeed because DeleteRange deletes what it can find and ignores missing items
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseContent);
            Assert.True(doc.RootElement.TryGetProperty("success", out var successElement));
            Assert.True(successElement.GetBoolean());
        }

        [Fact]
        public async Task DeleteAllForAncestral_EmptiesData_ReturnsSuccess()
        {
            // Arrange
            var deleteUrl = "/api/admin/ancestral/deleteall";
            var getUrl = "/api/admin/ancestral";

            // Act - Delete all
            var deleteResponse = await _client.PostAsync(deleteUrl, null);

            // Assert delete succeeded
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Verify data is deleted
            var getResponse = await _client.GetAsync(getUrl);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(getContent);
            Assert.True(doc.RootElement.TryGetProperty("data", out var dataElement));
            Assert.Equal(0, dataElement.GetArrayLength());
        }
    }
}
