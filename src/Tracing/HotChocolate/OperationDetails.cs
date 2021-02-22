using System.Collections.Immutable;

namespace Demo.Tracing
{
    internal class OperationDetails
    {
        public static readonly OperationDetails Empty = new OperationDetails(
            "unnamed",
            "unknown",
            new[] { "unknown" }.ToImmutableHashSet());

        public OperationDetails(
            string? name,
            string? rootSelection,
            IImmutableSet<string> selections)
        {
            Name = name;
            RootSelection = rootSelection;
            Selections = selections;
        }

        public string? Name { get; }
        public string? RootSelection { get; }
        public IImmutableSet<string> Selections { get; }
    }
}