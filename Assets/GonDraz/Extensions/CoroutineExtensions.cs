using System;
using System.Collections;
using GonDraz.Events;
using UnityEngine;

namespace GonDraz.Extensions
{
    public static class CoroutineExtensions
    {
        /// <summary>
        ///     Chờ thời gian thực (không bị ảnh hưởng bởi Time.timeScale).
        /// </summary>
        /// <param name="time">Thời gian chờ tính bằng giây.</param>
        /// <returns>Trả về IEnumerator để sử dụng trong coroutine.</returns>
        public static IEnumerator WaitForSecondsRealtime(float time)
        {
            var start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < time)
                yield return null;
        }

        /// <summary>
        ///     Chờ đến khi điều kiện đúng hoặc hết thời gian timeout.
        /// </summary>
        /// <param name="predicate">Điều kiện để kiểm tra.</param>
        /// <param name="timeout">Thời gian tối đa để chờ, tính bằng giây.</param>
        /// <returns>Trả về IEnumerator để sử dụng trong coroutine.</returns>
        public static IEnumerator WaitUntilOrTimeout(Func<bool> predicate, float timeout)
        {
            var start = Time.time;
            while (!predicate() && Time.time - start < timeout)
                yield return null;
        }

        /// <summary>
        ///     Lặp lại một coroutine nhiều lần liên tiếp.
        /// </summary>
        /// <param name="coroutine">Coroutine cần lặp lại.</param>
        /// <param name="times">Số lần lặp lại.</param>
        /// <returns>Trả về IEnumerator để sử dụng trong coroutine.</returns>
        public static IEnumerator RepeatCoroutine(Func<IEnumerator> coroutine, int times)
        {
            for (var i = 0; i < times; i++)
                yield return coroutine();
        }

        /// <summary>
        ///     Runs an action after a delay in seconds (using coroutine).
        ///     (Thực thi một hành động sau một khoảng delay bằng coroutine)
        /// </summary>
        public static IEnumerator InvokeAfter(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        /// <summary>
        ///     Runs an action every interval seconds, for a number of times (using coroutine).
        ///     (Thực thi một hành động lặp lại nhiều lần với khoảng thời gian giữa các lần)
        /// </summary>
        public static IEnumerator InvokeRepeating(float interval, int repeatCount, Action action)
        {
            for (var i = 0; i < repeatCount; i++)
            {
                action?.Invoke();
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        ///     Waits until a predicate is true, then runs an action.
        ///     (Chờ đến khi điều kiện đúng rồi thực thi một hành động)
        /// </summary>
        public static IEnumerator WaitUntilThen(Action action, Func<bool> predicate)
        {
            yield return new WaitUntil(predicate);
            action?.Invoke();
        }

        /// <summary>
        ///     Waits while a predicate is true, then runs an action.
        ///     (Chờ đến khi điều kiện sai rồi thực thi một hành động)
        /// </summary>
        public static IEnumerator WaitWhileThen(Action action, Func<bool> predicate)
        {
            yield return new WaitWhile(predicate);
            action?.Invoke();
        }

        /// <summary>
        ///     Runs an action after a delay in seconds (using coroutine).
        ///     (Thực thi một hành động sau một khoảng delay bằng coroutine)
        /// </summary>
        public static IEnumerator InvokeAfter(float delay, GEvent action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        /// <summary>
        ///     Runs an action every interval seconds, for a number of times (using coroutine).
        ///     (Thực thi một hành động lặp lại nhiều lần với khoảng thời gian giữa các lần)
        /// </summary>
        public static IEnumerator InvokeRepeating(float interval, int repeatCount, GEvent action)
        {
            for (var i = 0; i < repeatCount; i++)
            {
                action?.Invoke();
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        ///     Waits until a predicate is true, then runs an action.
        ///     (Chờ đến khi điều kiện đúng rồi thực thi một hành động)
        /// </summary>
        public static IEnumerator WaitUntilThen(GEvent action, Func<bool> predicate)
        {
            yield return new WaitUntil(predicate);
            action?.Invoke();
        }

        /// <summary>
        ///     Waits while a predicate is true, then runs an action.
        ///     (Chờ đến khi điều kiện sai rồi thực thi một hành động)
        /// </summary>
        public static IEnumerator WaitWhileThen(GEvent action, Func<bool> predicate)
        {
            yield return new WaitWhile(predicate);
            action?.Invoke();
        }

        /// <summary>
        ///     Starts a coroutine and automatically stops it after a timeout.
        ///     (Chạy một coroutine và tự động dừng sau một khoảng timeout)
        /// </summary>
        public static IEnumerator WithTimeout(this IEnumerator coroutine, float timeout)
        {
            var start = Time.time;
            while (coroutine.MoveNext())
            {
                if (Time.time - start > timeout)
                    yield break;
                yield return coroutine.Current;
            }
        }
    }
}