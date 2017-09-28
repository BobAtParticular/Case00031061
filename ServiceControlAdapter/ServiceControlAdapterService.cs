namespace ServiceControlAdapter
{
    using System;
    using System.ComponentModel;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using NServiceBus;
    using ServiceControl.TransportAdapter;

    [DesignerCategory("Code")]
    class ServiceControlAdapterService :
        ServiceBase
    {
        const string serviceName = "ServiceControlAdapter";

        static async Task Main()
        {
            Console.Title = serviceName;

            using (var service = new ServiceControlAdapterService())
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
        protected override void OnStart(string[] args)
        {
            AsyncOnStart().GetAwaiter().GetResult();
        }

        async Task AsyncOnStart()
        {
            const string catalogName = "ServiceControl";

            await SqlHelper.EnsureDatabaseExists(catalogName);

            var transportAdapterConfig = new TransportAdapterConfig<SqlServerTransport, SqlServerTransport>(serviceName)
            {
                EndpointSideAuditQueue = "audit",
                EndpointSideErrorQueue = "error",
                ServiceControlSideAuditQueue = "audit_adapted",
                ServiceControlSideErrorQueue = "error_adapted"
            };

            transportAdapterConfig.CustomizeEndpointTransport(transport => transport.ConnectionString(SqlHelper.BuildCatalogConnectionString(catalogName)));

            transportAdapterConfig.CustomizeServiceControlTransport(transport => transport.ConnectionString(SqlHelper.BuildCatalogConnectionString(catalogName)));

            transportAdapter = TransportAdapter.Create(transportAdapterConfig);

            await transportAdapter.Start().ConfigureAwait(false);
        }

        protected override void OnStop()
        {
            AsyncOnStop().GetAwaiter().GetResult();
        }

        Task AsyncOnStop() => transportAdapter == null ? Task.CompletedTask : transportAdapter.Stop();

        ITransportAdapter transportAdapter;
    }
}