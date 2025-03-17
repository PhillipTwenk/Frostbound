using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Unitilities
{
    public static class Utility
    {
        /// <summary>
        /// Вызов метода через указанное количество времени
        /// </summary>
        /// <param name="mb"> Ссылка на MonoBehaviour скрипта</param>
        /// <param name="f"> Делегат типа Action в который загружается метод </param>
        /// <param name="delay"> Время, через которое нужно вызвать метод </param>
        public static void Invoke(MonoBehaviour mb, Action f, float delay)
        {
            mb.StartCoroutine(InvokeRoutine(f, delay));
        }

        private static IEnumerator InvokeRoutine(Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            f();
        }
    
    
    
    
        /// <summary>
        /// Асинхронная перегрузка методов Invoke и InvokeRoutine
        /// Ожидает выполнения внтруенних методов делегата Func
        /// </summary>
        /// <param name="f"></param>
        /// <param name="delay"></param>
        public static async Task Invoke(Func<Task> f, float delay)
        {
            await InvokeRoutine(f, (int)delay * 1000);
        }

        private static async Task InvokeRoutine(Func<Task> f, int delay)
        {
            await Task.Delay(delay);
            await f();
        }
    }
}