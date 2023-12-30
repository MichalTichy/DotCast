using DotCast.SharedKernel.Models;

namespace DotCast.SharedKernel.Messages
{
    public interface IMessageHandler<TMessage>
    {
        Task Handle(TMessage message);
    }

    public interface IMultiMessageHandler<TMessage1, TMessage2>
    {
        Task Handle(TMessage1 message);
        Task Handle(TMessage2 message);
    }

    public interface IAsyncCascadingMessageHandler<TMessage>
    {
        IAsyncEnumerable<object> Handle(TMessage message);
    }

    public interface ICascadingMessageHandler<TMessage>
    {
        IEnumerable<object> Handle(TMessage message);
    }


    public interface IMessageHandler<TMessage, TResponse>
    {
        Task<TResponse> Handle(TMessage message);
    }
}