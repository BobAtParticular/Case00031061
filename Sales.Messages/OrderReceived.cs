using System;
using NServiceBus;

namespace Sales.Events
{
    public class OrderReceived :
        IEvent
    {
        public Guid OrderId { get; set; }
    }
}