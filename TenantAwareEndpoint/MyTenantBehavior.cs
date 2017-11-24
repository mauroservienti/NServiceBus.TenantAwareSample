using NServiceBus.Pipeline;
using System;
using System.Threading.Tasks;

namespace TenantAwareEndpoint
{
    class MyTenantBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            // custom logic before calling the next step in the pipeline.

            var tenantProvider = context.Builder.Build<MyTenantProvider>();
            tenantProvider.TenantId = context.MessageHeaders["tenant-id"];

            await next().ConfigureAwait(false);

            // custom logic after all inner steps in the pipeline completed.
        }
    }
}
