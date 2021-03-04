using System;
using Demo.Messaging;
using HotChocolate;
using MassTransit;
using static Demo.Tracing.ShopEventSources;

namespace Demo.Inventory
{
    public class Query
    {
        public InventoryInfo GetInventoryInfo(
            int upc, 
            [Service] InventoryInfoRepository repository,
            [Service] IBus bus)
        {
            try
            {
                bus.Publish(new Order { Count = 5 });

                if (upc > 1)
                {
                    // Simulate warning
                    Log.LowInventory(3);
                }

                return repository.GetInventoryInfo(upc);
            }
            catch (Exception ex)
            {
                // Simulate critical
                Log.GetInventoryFailed(ex);
                throw;
            }
        }

        public double GetShippingEstimate(int price, int weight) =>
            price > 1000 ? 0 : weight * 0.5;
    }
}