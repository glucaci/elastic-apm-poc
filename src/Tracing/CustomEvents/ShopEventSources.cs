using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Elastic.Apm;
using Elastic.Apm.Api;
using Serilog.Events;

namespace Demo.Tracing
{
    public sealed class ShopEventSources : IShopEventSources
    {
        private ShopEventSources() { }

        public static IShopEventSources Log { get; } = new ShopEventSources();

        private static string EventName = "Shop-Events";

        [NonEvent]
        public void GetInventory(int upc)
        {
            GetInventoryImpl(upc);
        }

        [Event(1, Level = EventLevel.Informational, Message = "Get inventory {2}", Version = 1)]
        private void GetInventoryImpl(int upc)
        {
            AppLog.Write(EventName, LogEventLevel.Information, "Get inventory {Upc}", upc);
        }
        
        [NonEvent]
        public void LowInventory(int stock)
        {
            LowInventoryImpl(stock);
        }

        [Event(2, Level = EventLevel.Warning, Message = "Inventory running low. Remaining stock {2}", Version = 1)]
        private void LowInventoryImpl(int stock)
        {
            AppLog.Write(EventName, LogEventLevel.Warning, "Inventory running low. Remaining stock {Stock}", stock);
        }

        [NonEvent]
        public void NoProduct(int upc)
        {
            NoProductImpl(upc);
        }

        [Event(3, Level = EventLevel.Error, Message = "No product found with id {2}", Version = 1)]
        private void NoProductImpl(int upc)
        {
            var executionSegment = Agent.Tracer.GetExecutionSegment();
            executionSegment.CaptureError(
                $"No product found with id {upc}", 
                EventName, 
                new StackFrame[0], 
                executionSegment.ParentId,
                new Dictionary<string, Label>
                {
                    {"EventSourcesName", new Label(EventName)},
                    {"EventSourcesType", new Label("Error")}
                });

            AppLog.Write(EventName, LogEventLevel.Error, "No product found with id {Upc}", upc);
        }

        [NonEvent]
        public void GetInventoryFailed(Exception ex)
        {
            GetInventoryFailedImpl(ex);
        }

        [Event(4, Level = EventLevel.Critical, Message = "Failed retrieving inventory", Version = 1)]
        private void GetInventoryFailedImpl(Exception ex)
        {
            var executionSegment = Agent.Tracer.GetExecutionSegment();
            var frames = new EnhancedStackTrace(ex).GetFrames();
            executionSegment.CaptureError(
                "Failed retrieving inventory",
                EventName,
                frames, 
                executionSegment.ParentId, 
                new Dictionary<string, Label>
                {
                    {"EventSourcesName", new Label(EventName)},
                    {"EventSourcesType", new Label("Critical")}
                });

            AppLog.Write(EventName, LogEventLevel.Fatal, "Failed retrieving inventory");
        }
    }
}
