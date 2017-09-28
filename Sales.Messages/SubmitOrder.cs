namespace Sales.Messages
{
    using System;
    using NServiceBus;

    public class SubmitOrder : IMessage
    {
        public Guid OrderId { get; set; }
    }
}
