using System;
using System.Collections.Generic;
using Elastic.Apm.Api;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Agent = Elastic.Apm.Agent;
using IError = HotChocolate.IError;

namespace Demo.Tracing
{
    internal class HotChocolateDiagnosticListener : DiagnosticEventListener
    {
        public override IActivityScope ExecuteRequest(IRequestContext context)
        {
            var transaction = Agent.Tracer.CurrentTransaction;
            return new RequestActivityScope(context, transaction);
        }

        public override IActivityScope ResolveFieldValue(IMiddlewareContext context)
        {
            //return EmptyScope;
            // If Dev
            if (context.Path.Depth == 0 &&
                context.Document.Definitions.Count == 1 &&
                context.Document.Definitions[0] is OperationDefinitionNode {Name: {Value: "exec_batch"}})
            {
                var executionSegment = Agent.Tracer.GetCurrentExecutionSegment();
                var span = executionSegment.StartSpan(
                    context.Field.Name!.Value, ApiConstants.TypeRequest, Constants.GraphQLType);
                return new FieldActivityScope(span);
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