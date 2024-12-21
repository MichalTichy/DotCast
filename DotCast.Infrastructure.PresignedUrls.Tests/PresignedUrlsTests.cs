using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace DotCast.Infrastructure.PresignedUrls.Tests
{
    public class PresignedUrlsTests
    {
        private readonly PresignedUrlManager presignedManager;
        private const string BaseUrl = "dotcast.com";

        public PresignedUrlsTests()
        {
            var optionsMock = new Mock<IOptions<PresignedUrlOptions>>();
            optionsMock.Setup(x => x.Value).Returns(() => new PresignedUrlOptions
            {
                SecretKey = "SECRET"
            });
            presignedManager = new PresignedUrlManager(optionsMock.Object);
        }

        [Fact]
        public void PresignedUrlSurvivesRoundtrip()
        {
            //Arrange
            var fileId = "BookId/FileId";

            //Act
            var url = presignedManager.GenerateUrl(BaseUrl, TimeSpan.FromSeconds(1));
            var result = presignedManager.ValidateUrl(url).result;

            //Assert
            result.Should().BeTrue();
            fileId.Should().Be(fileId);
        }

        [Fact]
        public void PresignedUrlInvalidIfTampered()
        {
            // Arrange
            var fileId = "FileId";
            var sourceUrl = $"{BaseUrl}/{fileId}";
            var url = presignedManager.GenerateUrl(sourceUrl, TimeSpan.FromSeconds(1));
            url = url.Replace(fileId, "TEMPERED");
            // Act
            var result = presignedManager.ValidateUrl(url).result;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PresignedUrlExpiresAfterOneSeconds()
        {
            // Arrange
            var fileId = "AnotherFileId";
            var sourceUrl = $"{BaseUrl}/{fileId}";
            var url = presignedManager.GenerateUrl(sourceUrl, TimeSpan.FromSeconds(1));

            // Act
            await Task.Delay(1100);
            var result = presignedManager.ValidateUrl(url).result;

            // Assert
            result.Should().BeFalse();
        }
    }
}