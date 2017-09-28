namespace Sales
{
    using System.Threading.Tasks;
    using Events;
    using Messages;
    using NServiceBus;
    using NServiceBus.Logging;

    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        static ILog Log = LogManager.GetLogger<SubmitOrderHandler>();

        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            Log.Info($"SubmitOrder recieved for order {message.OrderId}");

            await Task.WhenAll(
                context.Reply(new ClientOrderAccepted{ OrderId = message.OrderId }),
                context.Publish<OrderReceived>(evt => evt.OrderId = message.OrderId))
                .ConfigureAwait(false);
        }
    }
}
