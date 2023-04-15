using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UnityUtils.ParallelCode {
    
    public class ParallelCodeExecutor : MonoBehaviour
    {
        private Thread _thread;
        private readonly object _threadLocker = new();
   
        private Action _onSuccess;
        private Action _onFail;
        private Action _onTimeout;
        private bool _callbackCalled;
        
        private float _maxTime;
        private bool _destroyOnSuccess;
        private bool _destroyOnFail;
        private bool _destroyOnTimeout;
        private bool _useLateUpdate;
        private bool _logExceptions;

        private bool _hasSucceeded = false;
        private bool _hasFailed = false;
        private bool _hasTimedOut = false;
        public bool HasFinished => _hasSucceeded || _hasFailed || _hasTimedOut;
        
        private float _startTime;
        public float ElapsedTime => Time.time - _startTime;
        public float RemainingTime => _maxTime <= 0 ? float.PositiveInfinity : _maxTime - ElapsedTime;

        /// <summary>
        /// Executes the given action in a separate thread and calls the appropriate given callbacks when it finishes.
        /// 
        /// Method parameters may be extended in the future, therefore it is recommended to use named parameters.
        /// </summary>
        /// <param name="action"> The action to execute in a separate thread. </param>
        /// <param name="onSuccess"> The callback to call when the action finishes successfully and in the given time. </param>
        /// <param name="onFail"> The callback to call when the action fails. </param>
        /// <param name="onTimeout"> The callback to call when the action times out. </param>
        /// <param name="maxTime"> The maximum time the action can take.
        /// If it takes longer, it will be aborted and the onTimeout callback will be called.
        /// A value smaller than or equal to 0 means no timeout. </param>
        /// <param name="destroyOnSuccess"> If true, the game object will be destroyed when the action finishes successfully. </param>
        /// <param name="destroyOnFail"> If true, the game object will be destroyed when the action fails. </param>
        /// <param name="destroyOnTimeout"> If true, the game object will be destroyed when the action times out. </param>
        /// <param name="parent"> The parent of the new game object. </param>
        /// <param name="dontDestroyOnSceneChange"> If true, the game object will not be destroyed when the scene changes, using DontDestroyOnLoad. </param>
        /// <param name="useLateUpdate"> If true, the callbacks will be called in LateUpdate, otherwise in Update. </param>
        /// <param name="startImmediately"> If true, the action will be executed immediately, otherwise it will be executed when StartThread is called. </param>
        /// <param name="logExceptions"> If true, exceptions will be logged to the console using Debug.LogException. </param>
        /// <returns> The ParallelCodeExecutor component of the new game object. </returns>
        public static ParallelCodeExecutor ExecuteParallel(
                Action action, Action onSuccess = null, Action onFail = null, Action onTimeout = null, float maxTime = -1,
                bool destroyOnSuccess = true, bool destroyOnFail = true, bool destroyOnTimeout = true,
                Transform parent = null, bool dontDestroyOnSceneChange = false,
                bool useLateUpdate = true, bool startImmediately = true, bool logExceptions = true) {
            var go = new GameObject("ParallelCodeExecutor", typeof(ParallelCodeExecutor)) {
                transform = {
                    parent = parent
                }
            };
            if (dontDestroyOnSceneChange)
                DontDestroyOnLoad(go);
            var executor = go.GetComponent<ParallelCodeExecutor>();
            executor._onSuccess = onSuccess;
            executor._onFail = onFail;
            executor._onTimeout = onTimeout;
            executor._maxTime = maxTime;
            executor._destroyOnSuccess = destroyOnSuccess;
            executor._destroyOnFail = destroyOnFail;
            executor._destroyOnTimeout = destroyOnTimeout;
            executor._useLateUpdate = useLateUpdate;
            executor._logExceptions = logExceptions;
            executor.Execute(action, startImmediately);
            return executor;
        }

        /// <summary>
        /// Starts the thread if it has not been started before.
        ///
        /// Uses ThreadState.Unstarted to check if the thread has been started.
        /// </summary>
        /// <returns> True if the thread has been started, false otherwise. </returns>
        public bool StartThread() {
            if (_thread.ThreadState != ThreadState.Unstarted) return false;
            _thread.Start();
            return true;
        }

        private void Execute(Action action, bool startImmediately) {
            _thread = new Thread(() => {
                try {
                    action();
                    OnThreadSuccess();
                }
                catch (ThreadAbortException e) {
                    if (_logExceptions)
                        Debug.LogException(e);
                    // OnThreadTimeout() has already been called, that's why we're here
                }
                catch (Exception e) {
                    if (_logExceptions)
                        Debug.LogException(e);
                    OnThreadFail();
                }
                
            });
            _startTime = Time.time;
            if (startImmediately) {
                StartThread();
            }
        }

        private void OnThreadSuccess() {
            lock (_threadLocker) {
                _hasSucceeded = true;
            }
        }
        
        private void OnThreadFail() {
            lock (_threadLocker) {
                _hasFailed = true;
            }
        }
        
        private void OnThreadTimeout() {
            lock (_threadLocker) {
                _hasTimedOut = true;
            }
            _thread.Abort();
        }
        
        private void UpdateMethod() {
            if (_callbackCalled)
                return;
            if (HasFinished) {
                if (_hasSucceeded) {
                    _onSuccess?.Invoke();
                    if (_destroyOnSuccess)
                        Destroy(gameObject);
                } else if (_hasFailed) {
                    _onFail?.Invoke();
                    if (_destroyOnFail)
                        Destroy(gameObject);
                } else if (_hasTimedOut) {
                    _onTimeout?.Invoke();
                    if (_destroyOnTimeout)
                        Destroy(gameObject);
                }
                else {
                    Debug.LogError("This should never happen");
                }
                _callbackCalled = true;
            } else if (_maxTime > 0 && ElapsedTime > _maxTime) {
                OnThreadTimeout();
            }
        }

        private void Update() {
            if (!_useLateUpdate)
                UpdateMethod();
        }

        private void LateUpdate() {
            if (_useLateUpdate)
                UpdateMethod();
        }
    }
    
}
