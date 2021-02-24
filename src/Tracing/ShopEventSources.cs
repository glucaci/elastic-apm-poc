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
            Serilog.Log.Write(LogEventLevel.Information, "[{EventName}] Get inventory {upc}", EventName, upc);
        }

        [NonEvent]
        public void LowInventory(int stock)
        {
            LowInventoryImpl(stock);
        }

        [Event(2, Level = EventLevel.Warning, Message = "Inventory running low. Remaining stock {2}", Version = 1)]
        private void LowInventoryImpl(int stock)
        {
            Serilog.Log.Write(LogEventLevel.Warning, "[{EventName}] Inventory running low. Remaining stock {stock}", EventName, stock);
        }

        [NonEvent]
        public void NoProduct(int upc)
        {
            NoProductImpl(upc);
        }

        [Event(3, Level = EventLevel.Error, Message = "No product found with id {2}", Version = 1)]
        private void NoProductImpl(int upc)
        {
            var executionSegment = Agent.Tracer.GetCurrentExecutionSegment();
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

            Serilog.Log.Write(LogEventLevel.Error, "[{EventName}] No product found with id {upc}", EventName, upc);
        }

        [NonEvent]
        public void GetInventoryFailed(Exception ex)
        {
            GetInventoryFailedImpl(ex);
        }

        [Event(4, Level = EventLevel.Critical, Message = "Failed retrieving inventory", Version = 1)]
        private void GetInventoryFailedImpl(Exception ex)
        {
            var executionSegment = Agent.Tracer.GetCurrentExecutionSegment();
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

            Serilog.Log.Write(LogEventLevel.Fatal, "[{EventName}] Failed retrieving inventory", EventName);
        }
    }
}
