namespace Sales
{
    using System;
    using System.Threading.Tasks;

    class SalesService : EndpointService
    {
        private const string serviceName = "Sales";

        static async Task Main()
        {
            Console.Title = serviceName;

            using (var service = new SalesService())
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

        protected override string EndpointName { get; } = serviceName;
    }
}