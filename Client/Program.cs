using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using Sales.Messages;

public class Program
{
    static async Task Main()
    {
        const string endpointName = "Client";

        Console.Title = endpointName;

        var endpointConfiguration = new EndpointConfiguration(endpointName);

        endpointConfiguration.UsePersistence<InMemoryPersistence>();

        endpointConfiguration.UseTransport<SqlServerTransport>()
            .ConnectionString(SqlHelper.BuildCatalogConnectionString(endpointConfiguration.GetSettings().EndpointName()));

        endpointConfiguration.SendFailedMessagesTo(SqlHelper.BuildAddressWithCatalog("error", "ServiceControl"));

        endpointConfiguration.Recoverability()
            .Immediate(i => i.NumberOfRetries(0))
            .Delayed(d => d.NumberOfRetries(0));

        endpointConfiguration.EnableInstallers();

        await endpointConfiguration.EnsureDatabaseExists(); //See SqlHelper in shared

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Client started. Press <enter> to send a message");
        Console.WriteLine("Press any other key to exit");
        while (true)
        {
            if (Console.ReadKey().Key != ConsoleKey.Enter)
            {
                break;
            }
            var order = new SubmitOrder
            {
                OrderId = Guid.NewGuid()
            };

            await endpointInstance.Send(SqlHelper.BuildAddressWithCatalog("Sales", "Sales"), order).ConfigureAwait(false);

            Console.WriteLine($"SubmitOrder message sent with ID {order.OrderId}");
        }

        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}