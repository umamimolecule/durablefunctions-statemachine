using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Umamimolecule.DurableFunctionsStateMachine
{
    public class StateConfiguration<TState, TTrigger> : IStateConfiguration<TState, TTrigger>
    {
        public Dictionary<TTrigger, TState> Transitions { get; } = new Dictionary<TTrigger, TState>();

        public Func<Task> EntryTask { get; private set; }

        public Func<Task> ExitTask { get; private set; }

        public IStateConfiguration<TState, TTrigger> OnEntry(Func<Task> task)
        {
            this.EntryTask = task;
            return this;
        }

        public IStateConfiguration<TState, TTrigger> OnExit(Func<Task> task)
        {
            this.ExitTask = task;
            return this;
        }

        public IStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState nextState)
        {
            this.Transitions.Add(trigger, nextState);
            return this;
        }
    }
}
