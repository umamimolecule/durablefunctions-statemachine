using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace Umamimolecule.DurableFunctionExample
{
    public class ReplayAwareLogger : ILogger
    {
        private readonly IDurableOrchestrationContext context;
        private readonly ILogger logger;

        public ReplayAwareLogger(
            IDurableOrchestrationContext context,
            ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this.logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.context.IsReplaying)
            {
                this.logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}
