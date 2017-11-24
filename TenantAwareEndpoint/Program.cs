using Castle.Windsor;
using NServiceBus;
using NServiceBus.Features;
using System;
using System.Threading.Tasks;

namespace TenantAwareEndpoint
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            var configuration = new EndpointConfiguration("TenantAwareEndpoint");

            var container = new WindsorContainer();
            configuration.UseContainer<WindsorBuilder>(customizations =>
            {
                customizations.ExistingContainer(container);
            });

            configuration.RegisterComponents(components =>
            {
                components.ConfigureComponent<MyTenantProvider>(DependencyLifecycle.InstancePerUnitOfWork);
            });

            configuration.UseSerialization<JsonSerializer>();
            configuration.SendFailedMessagesTo("error");
            configuration.DisableFeature<TimeoutManager>(); //just for the sake of the sample
            configuration.UseTransport<RabbitMQTransport>()
                .ConnectionString("host=localhost");

            var pipeline = configuration.Pipeline;
            pipeline.Register(
                behavior: new MyTenantBehavior(),
                description: "Scope per tenant");

            var tcs = new TaskCompletionSource<object>();

            Console.CancelKeyPress += (sender, e) => { tcs.SetResult(null); };
            Console.WriteLine("Press Ctrl+C to exit...");

            var endpoint = await Endpoint.Start(configuration);

            while (!tcs.Task.IsCompleted)
            {
                var options = new SendOptions();
                options.RouteToThisEndpoint(); //equivalent to SendLocal
                options.SetHeader("tenant-id", Guid.NewGuid().ToString());

                await endpoint.Send(new MyMessage(), options);
            }

            await endpoint.Stop();
        }
    }
}
