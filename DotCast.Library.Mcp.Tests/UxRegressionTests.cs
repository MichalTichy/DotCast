using DotCast.App.Shared;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Library.Handlers;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing;
using DotCast.Storage.Processing.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotCast.Library.Mcp.Tests;

public class UxRegressionTests
{
    [Theory]
    [InlineData(698, "11h 38m")]
    [InlineData(59, "0h 59m")]
    [InlineData(1500, "25h 0m")]
    public void Duration_does_not_round_hours_up(int minutes, string expected) => Assert.Equal(expected, Ux.Duration(TimeSpan.FromMinutes(minutes)));

    [Fact]
    public void Unknown_rating_has_a_distinct_label() => Assert.Equal("Unrated", Ux.Rating(0));

    [Fact]
    public async Task Failed_file_metadata_save_does_not_publish_a_changed_library_record()
    {
        var book = MetadataPatchTests.Book("test-save", "own");
        var repository = new Mock<IRepository<AudioBook>>();
        repository.Setup(r => r.GetByIdAsync(book.Id, default, null)).ReturnsAsync(book);
        var storage = new Mock<IStorage>();
        storage.Setup(s => s.GetStorageEntry(book.Id)).Returns(new StorageEntryWithFiles(book.Id, [], null));
        storage.Setup(s => s.UpdateMetadataAsync(book.AudioBookInfo, default)).ThrowsAsync(new IOException("Unavailable files"));
        var messenger = new Mock<IMessagePublisher>();
        var handler = new SaveAudioBookRequestHandler(repository.Object, storage.Object, messenger.Object);
        await Assert.ThrowsAsync<IOException>(() => handler.Handle(new(book)));
        repository.Verify(r => r.UpdateAsync(It.IsAny<AudioBook>(), It.IsAny<CancellationToken>(), It.IsAny<string?>()), Times.Never);
        messenger.Verify(m => m.PublishAsync(It.IsAny<AudioBookReadyForProcessing>()), Times.Never);
    }

    [Fact]
    public async Task Save_cannot_edit_an_inaccessible_book()
    {
        var repository = new Mock<IRepository<AudioBook>>();
        var storage = new Mock<IStorage>();
        var handler = new SaveAudioBookRequestHandler(repository.Object, storage.Object, Mock.Of<IMessagePublisher>());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new(MetadataPatchTests.Book("foreign", "foreign"))));
        storage.Verify(s => s.UpdateMetadataAsync(It.IsAny<AudioBookInfo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Failed_processing_step_reports_failure_without_overwriting_metadata()
    {
        var storage = new Mock<IStorage>();
        storage.Setup(s => s.GetStorageEntry("test-process")).Returns(new StorageEntryWithFiles("test-process", [], null));
        var step = new Mock<IProcessingStep>();
        step.Setup(s => s.Process(It.IsAny<string>(), It.IsAny<Dictionary<string, ModificationType>>())).ThrowsAsync(new IOException("Test failure"));
        var messenger = new Mock<IMessagePublisher>();
        var pipeline = new ProcessingPipeline([step.Object], messenger.Object, storage.Object, NullLogger<ProcessingPipeline>.Instance);
        await pipeline.Handle(new("test-process", []));
        storage.Verify(s => s.ExtractMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        messenger.Verify(m => m.ExecuteAsync(It.IsAny<AudioBookStorageMetadataUpdated>(), It.IsAny<CancellationToken>()), Times.Never);
        messenger.Verify(m => m.PublishAsync(It.Is<ProcessingJobChanged>(job => job.AudioBookId == "test-process" && job.Succeeded == false)), Times.Once);
        messenger.Verify(m => m.PublishAsync(It.Is<ProcessingJobChanged>(job => job.Succeeded == true)), Times.Never);
    }
}
