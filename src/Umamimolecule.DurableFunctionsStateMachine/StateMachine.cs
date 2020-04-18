using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Umamimolecule.DurableFunctionsStateMachine
{
    public class StateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>
    {
        private readonly Dictionary<TState, StateConfiguration<TState, TTrigger>> configuration = new Dictionary<TState, StateConfiguration<TState, TTrigger>>();

        public StateMachine()
        {
        }

        public StateMachine(TState initialState)
        {
            this.SetInitialState(initialState);
        }

        public TState State { get; private set; }

        public Action<TState, TTrigger> OnUnhandledTrigger { get; set; }

        public IStateConfiguration<TState, TTrigger> Configure(TState state)
        {
            StateConfiguration<TState, TTrigger> stateConfiguration = new StateConfiguration<TState, TTrigger>();
            this.configuration.Add(state, stateConfiguration);
            return stateConfiguration;
        }

        public async Task FireAsync(TTrigger trigger)
        {
            if (!this.configuration[this.State].Transitions.TryGetValue(trigger, out TState nextState))
            {
                if (this.OnUnhandledTrigger != null)
                {
                    this.OnUnhandledTrigger(this.State, trigger);
                    return;
                }

                throw new UnhandledTriggerException(State, trigger);
            }

            if (this.configuration[this.State].ExitTask != null)
            {
                await this.configuration[this.State].ExitTask();
            }

            this.State = nextState;

            if (this.configuration[nextState].EntryTask != null)
            {
                await this.configuration[nextState].EntryTask();
            }
        }

        public void SetInitialState(TState state)
        {
            this.State = state;
        }
    }
}
