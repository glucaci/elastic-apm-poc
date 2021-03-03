using System;
using System.Collections.Generic;
using System.Linq;
using static Demo.Tracing.ShopEventSources;

namespace Demo.Inventory
{
    public class InventoryInfoRepository
    {
        private readonly Dictionary<int, InventoryInfo> _infos;

        public InventoryInfoRepository()
        {
            _infos = new InventoryInfo[]
            {
                new InventoryInfo(1, true),
                new InventoryInfo(2, false),
                new InventoryInfo(3, true)
            }.ToDictionary(t => t.Upc);
        }

        public InventoryInfo GetInventoryInfo(int upc)
        {
            // Simulate info
            Log.GetInventory(upc);

            if (upc > 3)
            {
                // Simulate error
                Log.NoProduct(upc);
                throw new InvalidOperationException($"Invalid product id {upc}");
            }

            return _infos[upc];
        }
    }
}