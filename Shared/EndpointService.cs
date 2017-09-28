using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;

[DesignerCategory("Code")]
public abstract class EndpointService :
    ServiceBase
{
    IEndpointInstance endpointInstance;

    protected abstract string EndpointName { get; }

    protected virtual void CustomizeTransport(TransportExtensions<SqlServerTransport> transport)
    {
    }

    protected override void OnStart(string[] args)
    {
        AsyncOnStart().GetAwaiter().GetResult();
    }

    protected virtual async Task AsyncOnStart()
    {
        var endpointConfiguration = new EndpointConfiguration(EndpointName);

        endpointConfiguration.UsePersistence<InMemoryPersistence>();

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>()
            .ConnectionString(SqlHelper.BuildCatalogConnectionString(endpointConfiguration.GetSettings().EndpointName()));

        CustomizeTransport(transport);

        endpointConfiguration.SendFailedMessagesTo(SqlHelper.BuildAddressWithCatalog("error", "ServiceControl"));

        endpointConfiguration.Recoverability()
            .Immediate(i => i.NumberOfRetries(0))
            .Delayed(d => d.NumberOfRetries(0));

        endpointConfiguration.EnableInstallers();

        await endpointConfiguration.EnsureDatabaseExists().ConfigureAwait(false); //See SqlHelper in shared

        endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
    }

    protected override void OnStop()
    {
        AsyncOnStop().GetAwaiter().GetResult();
    }

    protected Task AsyncOnStop()
    {
        if (endpointInstance == null)
        {
            return Task.CompletedTask;
        }
        return endpointInstance.Stop();
    }
}