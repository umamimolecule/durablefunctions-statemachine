using System;
using System.Threading.Tasks;

namespace Umamimolecule.DurableFunctionsStateMachine
{
    public interface IStateMachine<TState, TTrigger>
    {
        TState State { get; }

        Action<TState, TTrigger> OnUnhandledTrigger { get; set; }

        IStateConfiguration<TState, TTrigger> Configure(TState state);

        void SetInitialState(TState state);

        Task FireAsync(TTrigger trigger);
    }
}
