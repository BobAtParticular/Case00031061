namespace Shipping
{
    using System.Threading.Tasks;
    using Sales.Events;
    using NServiceBus;
    using NServiceBus.Logging;

    public class OrderRecievedHandler : IHandleMessages<OrderReceived>
    {
        static ILog Log = LogManager.GetLogger<OrderRecievedHandler>();

        public Task Handle(OrderReceived message, IMessageHandlerContext context)
        {
            Log.Info($"OrderReceived recieved for order {message.OrderId}");

            return Task.CompletedTask;
        }
    }
}
