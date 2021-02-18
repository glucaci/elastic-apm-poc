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
            _transaction.Name = GetTransactionName();
            _transaction.Type = "GraphQL";

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

            //_transaction.End(); - only when not using AspNetCoreDiagnosticSubscriber
        }

        private string? GetTransactionName()
        {
            string? name;
            if (!string.IsNullOrEmpty(_context.Request.OperationName))
            {
                // This is the most used case because the react generated queries always
                // provide a operation name
                name = _context.Request.OperationName;
            }
            else if (_context.Document?.Definitions.Count == 1 &&
                     _context.Document.Definitions[0] is OperationDefinitionNode node &&
                     !string.IsNullOrEmpty(node.Name?.Value))
            {
                // This is the case when we don't have a operation name and we have only one
                // root node which we are trying to resolve.
                name = node.Name!.Value;

                if (node.SelectionSet.Selections.Count == 1 &&
                    node.SelectionSet.Selections[0] is FieldNode fieldNode)
                {
                    name += $".{fieldNode.Name!.Value}";
                }
            }
            else
            {
                // If no case above is reached we set the query id which is a hash of the query
                // without a meaningful name but at least will stay together in the same group.
                name = _context.Request.QueryId;
            }

            return name;
        }
    }
}