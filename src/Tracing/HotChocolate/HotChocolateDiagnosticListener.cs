using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Serilog;
using Serilog.Events;
using Agent = Elastic.Apm.Agent;
using IError = HotChocolate.IError;

namespace Demo.Tracing
{
    internal class HotChocolateDiagnosticListener : DiagnosticEventListener
    {
        public override IActivityScope ExecuteRequest(IRequestContext context)
        {
            // If there is no transaction, we are creating a empty transaction
            // which will be filled with meaningful name and type when request finishes.
            var transaction = Agent.Tracer.CurrentTransaction
                ?? Agent.Tracer.StartTransaction(string.Empty, string.Empty);

            return new RequestActivityScope(context, transaction);
        }

        public override IActivityScope ResolveFieldValue(IMiddlewareContext context)
        {
            //return EmptyScope;

            // If Dev

            if (context.Path.Depth == 0 &&
                context.Document.Definitions.Count == 1 &&
                context.Document.Definitions[0] is OperationDefinitionNode { Name: {Value: "exec_batch" } })
            {
                var span = Agent.Tracer.CurrentSpan;
                if (span != null)
                {
                    var fieldSpan = span.StartSpan(context.Field.Name!.Value, "graphql", "batch");
                    return new FieldActivityScope(fieldSpan);
                }
                else
                {
                    var transaction = Agent.Tracer.CurrentTransaction;
                    var fieldSpan = transaction.StartSpan(context.Field.Name!.Value, "graphql", "batch");
                    return new FieldActivityScope(fieldSpan);
                }
            }

            return EmptyScope;
        }

        public override void RequestError(IRequestContext context, Exception exception)
        {
            Report.Exception(exception);
        }

        public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
        {
            Report.Error(errors);
        }
    }
}