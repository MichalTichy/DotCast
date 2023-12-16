using DotCast.SharedKernel.Models;

namespace DotCast.SharedKernel.Messages
{
    public abstract class MessageHandler<TMessage>
    {
        public abstract Task Handle(TMessage message);
    }

    public abstract class CascadingMessageHandler<TMessage>
    {
        public abstract IAsyncEnumerable<object> Handle(TMessage message);
    }

    public abstract class MessageHandler<TMessage, TResponse>
    {
        public abstract Task<TResponse> Handle(TMessage message);
    }
}