using System;
using System.Diagnostics.Tracing;
using Thor.Core.Abstractions;

namespace Demo.Tracing
{
    [EventSourceDefinition(Name = "Shop-Events")]
    public interface IShopEventSources
    {
        [Event(1,
            Level = EventLevel.Informational,
            Message = "Get inventory {upc}",
            Version = 1)]
        void GetInventory(int upc);

        [Event(2,
            Level = EventLevel.Warning,
            Message = "Inventory running low. Remaining stock {stock}",
            Version = 1)]
        void LowInventory(int stock);

        [Event(3,
            Level = EventLevel.Error,
            Message = "No product found with id {upc}",
            Version = 1)]
        void NoProduct(int upc);

        [Event(4,
            Level = EventLevel.Critical,
            Message = "Failed retrieving inventory",
            Version = 1)]
        void GetInventoryFailed(Exception ex);
    }
}