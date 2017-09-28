namespace Shipping
{
    using System;
    using System.Threading.Tasks;
    using Sales.Events;
    using NServiceBus;
    using NServiceBus.Transport.SQLServer;

    class ShippingService : EndpointService
    {
        const string serviceName = "Shipping";

        static async Task Main()
        {
            Console.Title = serviceName;

            using (var service = new ShippingService())
            {
                if (ServiceHelper.IsService())
                {
                    Run(service);
                    return;
                }
                await service.AsyncOnStart();
                Console.WriteLine("Endpoint started. Press any key to exit");
                Console.ReadKey();
                await service.AsyncOnStop();
            }
        }

        protected override void CustomizeTransport(TransportExtensions<SqlServerTransport> transport)
        {
            #region Setup subscription to another database / service:

            transport.UseCatalogForEndpoint("Sales", SqlHelper.BuildCatalogName("Sales")); //Associate "Sales" service with the database it owns
            transport.Routing().RegisterPublisher(typeof(OrderReceived), "Sales"); //Do not use app.config to define where the publisher is

            #endregion
        }

        protected override string EndpointName { get; } = serviceName;
    }
}