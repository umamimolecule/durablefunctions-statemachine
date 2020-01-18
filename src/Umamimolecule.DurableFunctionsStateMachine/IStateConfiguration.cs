using System;
using System.Threading.Tasks;

namespace Umamimolecule.DurableFunctionsStateMachine
{
    public interface IStateConfiguration<TState, TTrigger>
    {
        IStateConfiguration<TState, TTrigger> OnEntry(Func<Task> task);

        IStateConfiguration<TState, TTrigger> OnExit(Func<Task> task);

        IStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState nextState);
    }
}
