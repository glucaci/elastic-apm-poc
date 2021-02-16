using System;
using System.Collections.Generic;
using Elastic.Apm;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;
using IError = HotChocolate.IError;

namespace Demo.Tracing
{
    internal class HotChocolateDiagnosticListener : DiagnosticEventListener
    {
        public override IActivityScope ExecuteRequest(IRequestContext context)
        {
            var transaction = Agent.Tracer.CurrentTransaction;

            transaction.Name = context.Request.OperationName ?? context.Request.QueryId;

            return new EmptyActivityScope();
        }

        public override void RequestError(IRequestContext context, Exception exception)
        {
        }

        public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
        {
        }

        public override void ResolverError(IMiddlewareContext context, IError error)
        {
        }

        public class EmptyActivityScope : IActivityScope
        {
            public void Dispose()
            {
            }
        }
    }
}