using System;

namespace Umamimolecule.DurableFunctionsStateMachine
{
    [Serializable]
    public class UnhandledTriggerException : Exception
    {
        public UnhandledTriggerException(object state, object trigger)
            : base(string.Format(Resources.ExceptionMessages.UnhandledTrigger, trigger, state))
        {
            this.State = state;
            this.Trigger = trigger;
        }

        public object State { get; private set; }

        public object Trigger { get; private set; }
    }
}
