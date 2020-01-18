using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Umamimolecule.DurableFunctionsStateMachine
{
    public class StateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>
    {
        private TState state;
        private readonly Dictionary<TState, StateConfiguration<TState, TTrigger>> configuration = new Dictionary<TState, StateConfiguration<TState, TTrigger>>();

        public StateMachine()
        {
        }

        public StateMachine(TState initialState)
        {
            this.state = initialState;
        }

        public TState State => this.state;

        public Action<TState, TTrigger> OnUnhandledTrigger { get; set; }

        public IStateConfiguration<TState, TTrigger> Configure(TState state)
        {
            StateConfiguration<TState, TTrigger> stateConfiguration = new StateConfiguration<TState, TTrigger>();
            this.configuration.Add(state, stateConfiguration);
            return stateConfiguration;
        }

        public async Task FireAsync(TTrigger trigger)
        {
            if (!this.configuration[this.state].Transitions.TryGetValue(trigger, out TState nextState))
            {
                if (this.OnUnhandledTrigger != null)
                {
                    this.OnUnhandledTrigger(this.state, trigger);
                    return;
                }

                throw new UnhandledTriggerException(state, trigger);
            }

            if (this.configuration[this.state].ExitTask != null)
            {
                await this.configuration[this.state].ExitTask();
            }

            this.state = nextState;

            if (this.configuration[nextState].EntryTask != null)
            {
                await this.configuration[nextState].EntryTask();
            }
        }

        public void SetInitialState(TState state)
        {
            this.state = state;
        }
    }
}
