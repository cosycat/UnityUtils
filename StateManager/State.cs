using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityUtils.StateManager
{
    [Serializable]
    public abstract class State : MonoBehaviour
    {
        /// <summary>
        /// The state manager that manages this state
        /// </summary>
        public StateManager SM { get; private set; }
        
        /// <summary>
        /// Whether this state is the current state of its state manager
        /// </summary>
        public bool IsCurrentState => SM.CurrentState == this;

        /// <summary>
        /// Tries to get the state component in the next sibling of this state.
        /// </summary>
        /// <param name="nextState"> The state component in the next sibling, or null if there is no next sibling or the next sibling does not have a state component </param>
        /// <returns> Whether the next sibling has a state component </returns>
        public bool TryGetNextState(out State nextState)
        {
            if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
            {
                nextState = null;
                return false;
            }

            var nextSibling = transform.parent.transform.GetChild(transform.GetSiblingIndex() + 1);
            nextState = nextSibling.TryGetComponent(out State state) ? state : null;
            return state != null;
        }
        
        /// <summary>
        /// Sets the next state from <see cref="TryGetNextState"/> as the current state of the state manager.
        /// </summary>
        /// <exception cref="Exception"> Throws an exception if there is no next state </exception>
        public void SetNextState()
        {
            if (!TrySetNextState())
                throw new Exception("There is no next state");
        }

        /// <summary>
        /// Tries to set the next state from <see cref="TryGetNextState"/> as the current state of the state manager.
        /// </summary>
        /// <returns> True if there is a next state, which was set as the current state. False if there is no next state </returns>
        public bool TrySetNextState()
        {
            if (!TryGetNextState(out var nextState))
                return false;

            SM.CurrentState = nextState;
            return true;
        }

        /// <summary>
        /// Gets called whenever this state starts
        /// </summary>
        protected internal abstract void OnStateEnter();
        
        /// <summary>
        /// Gets called whenever this state stops, before it is replaced by another state
        /// </summary>
        protected internal abstract void OnStateExit();
        
        /// <summary>
        /// Gets called every frame while this state is active
        /// </summary>
        protected internal abstract void OnStateUpdate();
        
        protected internal virtual void OnStateAwake() { }
        protected internal virtual void OnStateStart() { }

        private void Update()
        {
            // To make sure the OnStateUpdateMethod is used instead of the Unity Update method.
        }

        /// <summary>
        /// Use this method to create a new state for a given state manager.
        /// </summary>
        /// <param name="stateManager"> The state manager that should manage the new state </param>
        /// <param name="state"> The newly created state </param>
        /// <typeparam name="T"> The type of the state to create </typeparam>
        public static void CreateState<T>([NotNull] StateManager stateManager, out T state) where T : State
        {
            var go = new GameObject();
            go.transform.SetParent(stateManager.transform);
            state = go.AddComponent<T>();
            // state.SM = stateManager; // is already set in the Awake method
            state.name = typeof(T).Name;
        }

        private void Awake()
        {
            var t = transform;
            while (SM == null && t.parent != null)
            {
                t = t.parent;
                if (t.TryGetComponent<StateManager>(out var sm))
                {
                    SM = sm;
                }
            }
            OnStateAwake();
        }
        
        private void Start()
        {
            OnStateStart();
        }
    }
}