namespace DotCast.Infrastructure.Messaging.Base
{
    public interface IMessagePublisher
    {
        Task<TResult> RequestAsync<TMessage, TResult>(TMessage message, CancellationToken cancellationToken = default);
        Task ExecuteAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
        ValueTask PublishToSingleAsync<TMessage>(TMessage message);
        ValueTask PublishAsync<TMessage>(TMessage message);
    }
}
