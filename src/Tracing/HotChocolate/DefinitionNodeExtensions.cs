using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HotChocolate.Language;

namespace Demo.Tracing
{
    public static class DefinitionNodeExtensions
    {
        public static IReadOnlyList<FieldNode> GetFieldNodes(
            this IReadOnlyList<IDefinitionNode> definitions)
        {
            return definitions.OfType<OperationDefinitionNode>()
                .SelectMany(x => x.SelectionSet.Selections)
                .OfType<FieldNode>()
                .ToImmutableList();
        }

        public static int FirstSelectionsCount(
            this IReadOnlyList<IDefinitionNode> definitions)
        {
            return definitions.OfType<OperationDefinitionNode>()
                .FirstOrDefault()?.SelectionSet.Selections.Count ?? 0;
        }
    }
}