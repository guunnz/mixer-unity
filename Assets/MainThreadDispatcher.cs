#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance = null!;
    public static MainThreadDispatcher Instance => _instance;

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // You use a thread-safe collection first-in first-out so you can pass on callbacks between the threads
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
    private readonly ConcurrentQueue<TaskCompletionSource<bool>> actionCompletionSources = new ConcurrentQueue<TaskCompletionSource<bool>>();

    public delegate Task Action();
    // This now can be called from any thread/task etc
    // => dispatched action will be executed in the next Unity Update call
    public void DoInMainThread(Action action)
    {
        mainThreadActions.Enqueue(action);
    }

    public Task DoInMainThreadAsync(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();
        actionCompletionSources.Enqueue(tcs);
        mainThreadActions.Enqueue(async () =>
        {
            await action();
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    // In the Unity main thread Update call routine you work off whatever has been enqueued since the last frame
    private void Update()
    {
        while (mainThreadActions.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }
}