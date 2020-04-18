using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Umamimolecule.DurableFunctionsStateMachine;

namespace Umamimolecule.DurableFunctionExample
{
    public class Orchestrator
    {
        public const string FunctionName = "Orchestrator";

        private StateMachine<State, Trigger> machine;

        public Orchestrator()
        {

        }

        [FunctionName(FunctionName)]
        public async Task Run(
            [OrchestrationTrigger]IDurableOrchestrationContext context,
            ILogger log)
        {
            var replayAwareLogger = new ReplayAwareLogger(context, log);
            this.machine = this.CreateInstance(context, replayAwareLogger);
            await machine.FireAsync(Trigger.Initialized);
        }

        private StateMachine<State, Trigger> CreateInstance(IDurableOrchestrationContext context, ILogger log)
        {
            var machine = new StateMachine<State, Trigger>();

            machine.Configure(State.Initializing)
                   .OnEntry(() => { log.LogInformation($"OnEntry called for {State.Initializing}"); return Task.CompletedTask; })
                   .OnExit(() => { log.LogInformation($"OnExit called for {State.Initializing}"); return Task.CompletedTask; })
                   .Permit(Trigger.Initialized, State.Green);

            machine.Configure(State.Green)
                   .OnEntry(() => { log.LogInformation($"OnEntry called for {State.Green}"); return GreenActivity(context, machine); })
                   .OnExit(() => { log.LogInformation($"OnExit called for {State.Green}"); return Task.CompletedTask; })
                   .Permit(Trigger.Stopping, State.Orange);

            machine.Configure(State.Orange)
                   .OnEntry(() => { log.LogInformation($"OnEntry called for {State.Orange}"); return OrangeActivity(context, machine); })
                   .OnExit(() => { log.LogInformation($"OnExit called for {State.Orange}"); return Task.CompletedTask; })
                   .Permit(Trigger.Stopped, State.Red);

            machine.Configure(State.Red)
                   .OnEntry(() => { log.LogInformation($"OnEntry called for {State.Red}"); return RedActivity(context, machine); })
                   .OnExit(() => { log.LogInformation($"OnExit called for {State.Red}"); return Task.CompletedTask; })
                   .Permit(Trigger.Resume, State.Green);

            machine.SetInitialState(State.Initializing);

            return machine;
        }

        private async Task GreenActivity(IDurableOrchestrationContext context, StateMachine<State, Trigger> machine)
        {
            context.SetCustomStatus(State.Green.ToString());
            await context.CallActivityAsync(Activity.FunctionName, State.Green.ToString());
            await machine.FireAsync(Trigger.Stopping);
        }

        private async Task OrangeActivity(IDurableOrchestrationContext context, StateMachine<State, Trigger> machine)
        {
            context.SetCustomStatus(State.Orange.ToString());
            await context.CallActivityAsync(Activity.FunctionName, State.Orange.ToString());
            await machine.FireAsync(Trigger.Stopped);
        }

        private async Task RedActivity(IDurableOrchestrationContext context, StateMachine<State, Trigger> machine)
        {
            context.SetCustomStatus(State.Red.ToString());
            await context.CallActivityAsync(Activity.FunctionName, State.Red.ToString());
        }

        enum State
        {
            Initializing,
            Green,
            Orange,
            Red,
        }

        enum Trigger
        {
            Initialized,
            Stopping,
            Stopped,
            Resume
        }
    }
}
