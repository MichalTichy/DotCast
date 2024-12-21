using DotCast.Infrastructure.Messaging.Base;
using JasperFx.Core;
using Shared.Infrastructure.CurrentUserProvider;
using System.Reflection.PortableExecutable;
using DotCast.Infrastructure.AppUser;
using Wolverine;
using Wolverine.Runtime;

namespace DotCast.Infrastructure.Messaging.Wolverine
{
    public class WolverineMessagePublisher(IMessageBus messageBus, ICurrentUserProvider<UserInfo> userProvider) : IMessagePublisher
    {
        public async Task<TResult> RequestAsync<TMessage, TResult>(TMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                var opts = await GetDeliveryOptionsAsync();

                return await messageBus.InvokeForTenantAsync<TResult>(opts.TenantId!, message!, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task ExecuteAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                var opts = await GetDeliveryOptionsAsync();
                await messageBus.InvokeForTenantAsync(opts.TenantId!, message!, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async ValueTask PublishToSingleAsync<TMessage>(TMessage message)
        {
            try
            {
                var opts = await GetDeliveryOptionsAsync();

                await messageBus.SendAsync(message, opts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async ValueTask PublishAsync<TMessage>(TMessage message)
        {
            try
            {
                var opts = await GetDeliveryOptionsAsync();

                await messageBus.PublishAsync(message, opts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<DeliveryOptions> GetDeliveryOptionsAsync()
        {
            var user = await userProvider.GetCurrentUserAsync();
            var options = new DeliveryOptions
            {
                TenantId = user?.Id ?? string.Empty
            };


            return options;
        }
    }
}
