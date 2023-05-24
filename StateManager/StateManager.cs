using System;
using System.Linq;
using UnityEngine;

namespace UnityUtils.StateManager
{
    public abstract class StateManager : MonoBehaviour
    {
        [SerializeField] protected State initialState;

        /// <summary>
        /// The current state.
        /// Setting this property will call the OnStateStop method of the old state and the OnStateStart method of the new state.
        /// It will also invoke the BeforeStateChangeEvent and AfterStateChangeEvent events.
        /// </summary>
        public State CurrentState { get; private set; }
        
        /// <summary>
        /// The ID of the current state.
        /// </summary>
        public long CurrentStateId => CurrentState?.StateId ?? -2;

        /// <summary>
        /// Change the current state to a new state.
        /// </summary>
        /// <param name="newState"> The state to change to. </param>
        public void ChangeState(State newState)
        {
            BeforeStateChangeEvent?.Invoke(this, new StateChangeEventArgs(CurrentState, newState));
            if (CurrentState != null) CurrentState.OnStateExit();
            CurrentState = newState;
            if (CurrentState != null) CurrentState.OnStateEnter();
            AfterStateChangeEvent?.Invoke(this, new StateChangeEventArgs(CurrentState, newState));
        }

        /// <summary>
        /// Get a state by its id.
        /// Searches all children of this state manager for a state with the given id.
        /// </summary>
        /// <param name="stateId"> The id of the state to get </param>
        /// <param name="requestOnly"> Whether to only request the state change, without actually changing the state </param>
        /// <exception cref="Exception"> Throws an exception if no state with the given id was found </exception>
        public void ChangeStateByID(long stateId, bool requestOnly = false)
        {
            if (!TryGetStateById(stateId, out var state))
                throw new Exception($"No state with id {stateId} found");
            if (requestOnly)
            {
                RequestStateChange(state);
            }
            else
            {
                ChangeState(state);
            }
        }
        
        /// <summary>
        /// Change the current state to the next state by their ID.
        /// Specifically, this will change the state to the state with the ID (CurrentStateId + 1).
        /// </summary>
        /// <param name="requestOnly"> Whether to only request the state change, without actually changing the state </param>
        /// <exception cref="Exception"> Throws an exception if no state with an id of CurrentStateId + 1 was found </exception>
        public void ChangeToNextState(bool requestOnly = false)
        {
            ChangeStateByID(CurrentStateId + 1, requestOnly);
        }

        /// <summary>
        /// Get a state by its id.
        /// </summary>
        /// <param name="stateId"> The id of the state to get </param>
        /// <param name="state"> The state with the given id, or null if no state with the given id was found </param>
        /// <returns> Whether a state with the given id was found </returns>
        /// <exception cref="Exception"> Throws an exception if multiple states with the given id were found </exception>
        public bool TryGetStateById(long stateId, out State state)
        {
            var statesWithID = GetComponentsInChildren<State>().Where(s => s.StateId == stateId).ToArray();
            switch (statesWithID.Length)
            {
                case 1:
                    state = statesWithID[0];
                    return true;
                case 0:
                    state = null;
                    return false;
                default:
                    throw new Exception("Multiple states with the same id found");
            }
            
        }


        /// <summary>
        /// Request a state change without actually changing the state.
        /// see <see cref="RequestedStateChangeEvent"/>
        /// </summary>
        /// <param name="newState"> The state to change to </param>
        public void RequestStateChange(State newState)
        {
            RequestedStateChangeEvent?.Invoke(this, new StateChangeEventArgs(CurrentState, newState));
        }
        
        /// <summary>
        /// Event that gets invoked when a state change is requested, without the state actually changing
        /// Useful for network logic, where the state change needs to be approved by the server,
        /// and then the server will change the state and send the new state to all clients
        /// </summary>
        public event EventHandler<StateChangeEventArgs> RequestedStateChangeEvent;

        /// <summary>
        /// Event that gets invoked before the current state gets changed
        /// </summary>
        public event EventHandler<StateChangeEventArgs> BeforeStateChangeEvent;
        
        /// <summary>
        /// Event that gets invoked after the current state got changed
        /// </summary>
        public event EventHandler<StateChangeEventArgs> AfterStateChangeEvent;
        
        protected virtual void Awake()
        {
            Debug.Log("StateManager Awake");
            if (initialState == null && !TryGetStateById(0, out initialState))
                throw new Exception("No initial state set and no state with id 0 found");
            CurrentState = initialState;
        }

        protected virtual void Start()
        {
            Debug.Log($"StateManager Start - CurrentState: {CurrentState}");
            if (CurrentState != null) CurrentState.OnStateEnter();
        }

        private void Update()
        {
            BeforeStateUpdate();
            if (CurrentState != null) CurrentState.OnStateUpdate();
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