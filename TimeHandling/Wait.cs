using System;
using System.Collections;
using UnityEngine;

namespace UnityUtils.TimeHandling {
    
    public class Wait
    {
        /// <summary>
        /// Waits until the condition is fulfilled or the timeout is reached.
        /// </summary>
        /// <param name="condition"> The condition to wait for. </param>
        /// <param name="timeout"> The timeout in seconds. </param>
        /// <returns> An IEnumerator that can be used in a coroutine. </returns>
        /// <exception cref="TimeoutException"> Throws a TimeoutException if the timeout is reached. </exception>
        
        public static IEnumerator Until(Func<bool> condition, float timeout = 30f)
        {
            var timePassed = 0f;
            while (!condition() && timePassed < timeout) {
                yield return new WaitForEndOfFrame();
                timePassed += Time.deltaTime;
            }
            if (timePassed >= timeout) {
                throw new TimeoutException("Condition was not fulfilled for " + timeout + " seconds.");
            }
        }
    }
    
}
