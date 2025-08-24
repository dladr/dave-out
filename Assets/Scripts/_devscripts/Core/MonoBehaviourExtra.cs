using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Extended MonoBehaviour with additional utility methods for coroutines and lerping.
    /// Provides convenient methods for delayed actions and smooth value transitions.
    /// </summary>
    public class MonoBehaviourExtra : MonoBehaviour
    {
        /// <summary>
        /// Invokes an action after a specified delay using a coroutine.
        /// </summary>
        /// <param name="action">The action to invoke</param>
        /// <param name="timer">Delay in seconds before invoking the action</param>
        public void Invoke(Action action, float timer = 0)
        {
            StartCoroutine(InvokeCo(action, timer));
        }

        private IEnumerator InvokeCo(Action a, float timer)
        {
            yield return new WaitForSeconds(timer);
            a();
        }

        /// <summary>
        /// Lerps between two values over time, calling the provided action with interpolated values.
        /// </summary>
        /// <param name="overTime">Action called with each interpolated value</param>
        /// <param name="startValue">Starting value for the lerp</param>
        /// <param name="endValue">Target value for the lerp</param>
        /// <param name="timeToFade">Duration of the lerp in seconds</param>
        /// <param name="onComplete">Optional callback when lerp completes</param>
        /// <returns>The coroutine for this lerp operation</returns>
        public IEnumerator LerpOverTime(Action<float> overTime, float startValue, float endValue, float timeToFade, Action onComplete = null)
        {
            return ActionOverTime(currentPercent =>
            {
                var currentValue = Mathf.Lerp(startValue, endValue, currentPercent);
                overTime(currentValue);
            }, timeToFade, onComplete);
        }

        /// <summary>
        /// Executes an action over time, providing the completion percentage (0-1) to the action.
        /// </summary>
        /// <param name="overTime">Action called with completion percentage (0-1)</param>
        /// <param name="timeToFade">Duration in seconds</param>
        /// <param name="onComplete">Optional callback when operation completes</param>
        /// <returns>The coroutine for this operation</returns>
        public IEnumerator ActionOverTime(Action<float> overTime, float timeToFade, Action onComplete = null)
        {
            var currentTime = 0f;
            while (currentTime < timeToFade)
            {
                var percentComplete = currentTime / timeToFade;

                overTime(percentComplete);

                currentTime += Time.deltaTime;
                yield return null;
            }
            overTime(1f);
            yield return null;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Starts a lerp coroutine and returns the Coroutine reference for management.
        /// </summary>
        /// <param name="overTime">Action called with each interpolated value</param>
        /// <param name="startValue">Starting value for the lerp</param>
        /// <param name="endValue">Target value for the lerp</param>
        /// <param name="timeToFade">Duration of the lerp in seconds</param>
        /// <param name="onComplete">Optional callback when lerp completes</param>
        /// <returns>The started coroutine</returns>
        public Coroutine LerpCoroutine(Action<float> overTime, float startValue, float endValue, float timeToFade,
            Action onComplete = null)
        {
            return StartCoroutine(LerpOverTime(overTime, startValue, endValue, timeToFade, onComplete));
        }

        /// <summary>
        /// Starts an action-over-time coroutine and returns the Coroutine reference for management.
        /// </summary>
        /// <param name="overTime">Action called with completion percentage (0-1)</param>
        /// <param name="timeToFade">Duration in seconds</param>
        /// <param name="onComplete">Optional callback when operation completes</param>
        /// <returns>The started coroutine</returns>
        public Coroutine StartCoroutineOverTime(Action<float> overTime, float timeToFade, Action onComplete = null)
        {
            return StartCoroutine(ActionOverTime(overTime, timeToFade, onComplete));
        }
    }
}


