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
        }
    }
}