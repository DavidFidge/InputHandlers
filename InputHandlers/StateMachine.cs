using System;
using System.Collections.Generic;
using System.Linq;

using InputHandlers.State;

namespace InputHandlers.StateMachine
{
    public class StateMachine<T>
    {
        private readonly T _owner;

        public StateMachine(T owner)
        {
            _owner = owner;
            CurrentState = null;
            PreviousState = null;
            GlobalState = null;
        }

        public State<T> CurrentState { get; private set; }
        public State<T> PreviousState { get; private set; }
        public State<T> GlobalState { get; private set; }

        public void SetCurrentState(State<T> currentState)
        {
            CurrentState = currentState;
        }

        public void SetPreviousState(State<T> previousState)
        {
            PreviousState = previousState;
        }

        public void SetGlobalState(State<T> globalState)
        {
            GlobalState = globalState;
        }

        public void Update()
        {
            if (GlobalState != null)
                GlobalState.Execute(_owner);

            if (CurrentState != null)
                CurrentState.Execute(_owner);
        }

        public void ChangeState(State<T> newState)
        {
            PreviousState = CurrentState;
            CurrentState.Exit(_owner);
            CurrentState = newState;
            CurrentState.Enter(_owner);
        }

        public void RevertToPreviousState()
        {
            ChangeState(PreviousState);
        }

        public bool IsInState(State<T> state)
        {
            if (state.GetType() == CurrentState.GetType())
                return true;
            return false;
        }

        public string GetCurrentStateTypeName()
        {
            return CurrentState.GetType().Name;
        }
    }
}