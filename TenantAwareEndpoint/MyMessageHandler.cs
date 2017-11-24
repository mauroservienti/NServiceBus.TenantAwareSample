using NServiceBus;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TenantAwareEndpoint
{
    class MyMessageHandler : IHandleMessages<MyMessage>
    {
        MyTenantProvider tenantProvider;

        public MyMessageHandler(MyTenantProvider tenantProvider)
        {
            this.tenantProvider = tenantProvider;
        }

        public Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            Console.WriteLine($"tenant-id: {context.MessageHeaders["tenant-id"]} - {tenantProvider.TenantId}");

            Debug.Assert(context.MessageHeaders["tenant-id"] == tenantProvider.TenantId, "Something is off, TenantProvider should have same ID as current message context");

            return Task.CompletedTask;
        }
    }
}
