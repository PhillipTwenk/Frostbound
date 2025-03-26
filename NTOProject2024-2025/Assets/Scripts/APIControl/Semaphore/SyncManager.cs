using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace APIControl.Semaphore
{
    public class SyncManager : MonoBehaviour
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 10);
        private static readonly Queue<Func<Task>> _taskQueue = new Queue<Func<Task>>();
        private static readonly object _lock = new object();
        private static bool _isProcessing = false;

        public static Task Enqueue(Func<Task> taskFunc)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            lock (_lock)
            {
                _taskQueue.Enqueue(async () =>
                {
                    try
                    {
                        await taskFunc();
                        taskCompletionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                });

                if (!_isProcessing)
                {
                    _isProcessing = true;
                    _ = ProcessQueue();
                }
            }

            return taskCompletionSource.Task;
        }

        private static async Task ProcessQueue()
        {
            while (true)
            {
                Func<Task> taskFunc;

                lock (_lock)
                {
                    if (_taskQueue.Count == 0)
                    {
                        _isProcessing = false;
                        return;
                    }

                    taskFunc = _taskQueue.Dequeue();
                }

                await _semaphore.WaitAsync();
                try
                {
                    await taskFunc();
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
    }
}