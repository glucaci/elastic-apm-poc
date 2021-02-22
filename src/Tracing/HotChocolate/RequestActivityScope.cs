using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Elastic.Apm.Api;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Language;

namespace Demo.Tracing
{
    internal class RequestActivityScope : IActivityScope
    {
        private readonly IRequestContext _context;
        private readonly ITransaction _transaction;

        public RequestActivityScope(IRequestContext context, ITransaction transaction)
        {
            _context = context;
            _transaction = transaction;
        }

        public void Dispose()
        {
            var operationDetails = GetOperationDetails();
            _transaction.Name = $"[{operationDetails.Name}] {operationDetails.RootSelection}";
            _transaction.Type = "graphql";

            for (var i = 0; i < operationDetails.Selections.Count; i++)
            {
                _transaction.SetLabel($"selection_{i}", operationDetails.Selections.ElementAt(i));
            }

            bool hasErrors = _context.HasException() || _context.Result.HasErrors();
            _transaction.Result = hasErrors ? "Failed" : "Success";
            _transaction.Outcome = hasErrors ? Outcome.Failure : Outcome.Success;

            if (_context.Result.HasErrors())
            {
                Report.Error(_context.Result!.Errors);
            }

            if (_context.Exception != null)
            {
                Report.Exception(_context.Exception);
            }
        }

        private OperationDetails GetOperationDetails()
        {
            var definitions = _context.Document?.Definitions;
            if (definitions?.Count > 0)
            {
                string? name = GetOperationName(definitions);

                var selections = definitions.GetFieldNodes()
                    .Select(x => string.IsNullOrEmpty(x.Name.Value) ? "unknown" : x.Name.Value)
                    .DefaultIfEmpty("unknown")
                    .ToImmutableHashSet();

                var rootSelection = definitions.Count > 1
                    ? "multiple"
                    : name == "exec_batch"
                        ? selections.Count > 1
                            ? $"multiple ({definitions.FirstSelectionsCount()})"
                            : $"{selections.FirstOrDefault()} ({definitions.FirstSelectionsCount()})"
                        : selections.FirstOrDefault();

                return new OperationDetails(name, rootSelection, selections);
            }

            return OperationDetails.Empty;
        }

        private string? GetOperationName(IReadOnlyList<IDefinitionNode> definition)
        {
            string? name = _context.Request.OperationName;

            if (string.IsNullOrEmpty(name) &&
                definition.Count == 1 && 
                definition[0] is OperationDefinitionNode node)
            {
                name = node.Name?.Value;
            }

            if (string.IsNullOrEmpty(name) || name == "fetch")
            {
                name = _context.Request.QueryId;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "unnamed";
            }

            return name;
        }
    }
}