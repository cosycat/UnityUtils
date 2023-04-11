using System;
using UnityEngine;

namespace UnityUtils.StateManager
{
    public abstract class StateManager : MonoBehaviour
    {
        private State _currentState;

        /// <summary>
        /// The current state.
        /// Setting this property will call the OnStateStop method of the old state and the OnStateStart method of the new state.
        /// It will also invoke the BeforeStateChangeEvent and AfterStateChangeEvent events.
        /// </summary>
        public State CurrentState
        {
            get => _currentState;
            set
            {
                BeforeStateChangeEvent?.Invoke(this, new StateChangeEventArgs(_currentState, value));
                _currentState.OnStateStop();
                _currentState = value;
                _currentState.OnStateStart();
                AfterStateChangeEvent?.Invoke(this, new StateChangeEventArgs(_currentState, value));
            }
        }

        /// <summary>
        /// Event that gets invoked before the current state gets changed
        /// </summary>
        public event EventHandler<StateChangeEventArgs> BeforeStateChangeEvent;
        
        /// <summary>
        /// Event that gets invoked after the current state got changed
        /// </summary>
        public event EventHandler<StateChangeEventArgs> AfterStateChangeEvent;

        private void Update()
        {
            BeforeStateUpdate();
            _currentState.OnStateUpdate();
            AfterStateUpdate();
        }
        
        /// <summary>
        /// Gets called before the current state gets updated
        /// </summary>
        protected virtual void BeforeStateUpdate() {}
        
        /// <summary>
        /// Gets called after the current state got updated
        /// </summary>
        protected virtual void AfterStateUpdate() {}
        
        
    }

    /// <summary>
    /// Event args for state change events
    /// </summary>
    public class StateChangeEventArgs : EventArgs
    {
        /// <summary>
        /// The state that was/is active before the change
        /// </summary>
        public State OldState { get; }
        
        /// <summary>
        /// The state that will be/is active after the change
        /// </summary>
        public State NewState { get; }

        public StateChangeEventArgs(State oldState, State newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    public abstract class State
    {
        
        /// <summary>
        /// Gets called whenever this state starts
        /// </summary>
        public abstract void OnStateStart();
        
        /// <summary>
        /// Gets called whenever this state stops, before it is replaced by another state
        /// </summary>
        public abstract void OnStateStop();
        
        /// <summary>
        /// Gets called every frame while this state is the current state
        /// </summary>
        public abstract void OnStateUpdate();
    
    }
}