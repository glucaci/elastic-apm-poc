using System;
using HotChocolate;
using static Demo.Tracing.ShopEventSources;

namespace Demo.Inventory
{
    public class Query
    {
        public InventoryInfo GetInventoryInfo(
            int upc, 
            [Service] InventoryInfoRepository repository)
        {
            try
            {
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