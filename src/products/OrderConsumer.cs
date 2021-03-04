using System;
using System.Threading.Tasks;
using Demo.Messaging;
using MassTransit;

namespace Demo.Products
{
    public class OrderConsumer : IConsumer<Order>
    {
        public async Task Consume(ConsumeContext<Order> context)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }
    }
}