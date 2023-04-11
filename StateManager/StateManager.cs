using System;
using UnityEngine;

namespace UnityUtils.StateManager
{
    public abstract class StateManager : MonoBehaviour
    {
        [SerializeField] protected State initialState;

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
                if (_currentState != null) _currentState.OnStateExit();
                _currentState = value;
                if (_currentState != null) _currentState.OnStateEnter();
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
        
        protected void Awake()
        {
            Debug.Log("StateManager Awake");
            _currentState = initialState;
        }

        private void Start()
        {
            if (_currentState != null) _currentState.OnStateEnter();
        }

        private void Update()
        {
            BeforeStateUpdate();
            if (_currentState != null) _currentState.OnStateUpdate();
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
}