using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Umamimolecule.DurableFunctionsStateMachine.Tests
{
    public class StateMachineTests
    {
        enum State
        {
            Stopped = 0,
            WarmingUp,
            Ready,
            Dispensing
        }

        enum Trigger
        {
            SwitchOn,
            OperatingTemperatureReached,
            StartDispensing,
            StopDispensing,
            SwitchOff
        }

        [Fact]
        public async Task HappyPath()
        {
            List<string> logs = new List<string>();
            var machine = this.CreateInstance(logs);

            await machine.FireAsync(Trigger.SwitchOn);
            machine.State.ShouldBe(State.WarmingUp);

            await machine.FireAsync(Trigger.OperatingTemperatureReached);
            machine.State.ShouldBe(State.Ready);

            await machine.FireAsync(Trigger.StartDispensing);
            machine.State.ShouldBe(State.Dispensing);

            await machine.FireAsync(Trigger.StopDispensing);
            machine.State.ShouldBe(State.Ready);

            await machine.FireAsync(Trigger.SwitchOff);
            machine.State.ShouldBe(State.Stopped);

            logs[0].ShouldBe("OnExit called for Stopped");
            logs[1].ShouldBe("OnEntry called for WarmingUp");
            logs[2].ShouldBe("OnExit called for WarmingUp");
            logs[3].ShouldBe("OnEntry called for Ready");
            logs[4].ShouldBe("OnExit called for Ready");
            logs[5].ShouldBe("OnEntry called for Dispensing");
            logs[6].ShouldBe("OnExit called for Dispensing");
            logs[7].ShouldBe("OnEntry called for Ready");
            logs[8].ShouldBe("OnExit called for Ready");
            logs[9].ShouldBe("OnEntry called for Stopped");
        }

        [Fact]
        public async Task NoEntryExit()
        {
            var machine = new StateMachine<State, Trigger>();

            machine.Configure(State.Stopped)
                   .Permit(Trigger.SwitchOn, State.Ready);

            machine.Configure(State.Ready)
                   .Permit(Trigger.SwitchOff, State.Stopped);

            await machine.FireAsync(Trigger.SwitchOn);
            machine.State.ShouldBe(State.Ready);

            await machine.FireAsync(Trigger.SwitchOff);
            machine.State.ShouldBe(State.Stopped);
        }

        [Fact]
        public void InitialStateConstructor()
        {
            var machine = new StateMachine<State, Trigger>(State.Ready);
            machine.State.ShouldBe(State.Ready);
        }

        [Fact]
        public async Task InvalidTransitionShouldThrowUnhandledTriggerException()
        {
            List<string> logs = new List<string>();
            var machine = this.CreateInstance(logs);

            var exception = await ShouldThrowAsyncExtensions.ShouldThrowAsync<UnhandledTriggerException>(() => machine.FireAsync(Trigger.StartDispensing));

            exception.State.ShouldBe(State.Stopped);
            exception.Trigger.ShouldBe(Trigger.StartDispensing);
        }

        [Fact]
        public async Task InvalidTransitionShouldCallUnhandledTriggerCallback()
        {
            List<string> logs = new List<string>();
            var machine = this.CreateInstance(logs);

            State receivedState = default;
            Trigger receivedTrigger = default;
            machine.OnUnhandledTrigger = (State s, Trigger t) =>
            {
                receivedState = s;
                receivedTrigger = t;    
            };

            await machine.FireAsync(Trigger.StartDispensing);

            receivedState.ShouldBe(State.Stopped);
            receivedTrigger.ShouldBe(Trigger.StartDispensing);
        }

        private StateMachine<State, Trigger> CreateInstance(List<string> logs)
        {
            var machine = new StateMachine<State, Trigger>();

            machine.Configure(State.Stopped)
                   .OnEntry(() => { logs.Add($"OnEntry called for {State.Stopped}"); return Task.CompletedTask; })
                   .OnExit(() => { logs.Add($"OnExit called for {State.Stopped}"); return Task.CompletedTask; })
                   .Permit(Trigger.SwitchOn, State.WarmingUp);

            machine.Configure(State.WarmingUp)
                   .OnEntry(() => { logs.Add($"OnEntry called for {State.WarmingUp}"); return Task.CompletedTask; })
                   .OnExit(() => { logs.Add($"OnExit called for {State.WarmingUp}"); return Task.CompletedTask; })
                   .Permit(Trigger.SwitchOff, State.Stopped)
                   .Permit(Trigger.OperatingTemperatureReached, State.Ready);

            machine.Configure(State.Ready)
                   .OnEntry(() => { logs.Add($"OnEntry called for {State.Ready}"); return Task.CompletedTask; })
                   .OnExit(() => { logs.Add($"OnExit called for {State.Ready}"); return Task.CompletedTask; })
                   .Permit(Trigger.SwitchOff, State.Stopped)
                   .Permit(Trigger.StartDispensing, State.Dispensing);

            machine.Configure(State.Dispensing)
                   .OnEntry(() => { logs.Add($"OnEntry called for {State.Dispensing}"); return Task.CompletedTask; })
                   .OnExit(() => { logs.Add($"OnExit called for {State.Dispensing}"); return Task.CompletedTask; })
                   .Permit(Trigger.SwitchOff, State.Stopped)
                   .Permit(Trigger.StopDispensing, State.Ready);

            machine.SetInitialState(State.Stopped);

            return machine;
        }
    }
}
