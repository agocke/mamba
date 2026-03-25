namespace Mamba;

/// <summary>
/// Handle to a running HTTP server.
/// </summary>
public sealed class ServerHandle
{
    private readonly Task _serverTask;
    private readonly TaskCompletionSource _readyTcs;
    private readonly CancellationTokenSource _cts;

    internal ServerHandle(Task serverTask, TaskCompletionSource readyTcs, CancellationTokenSource cts)
    {
        _serverTask = serverTask;
        _readyTcs = readyTcs;
        _cts = cts;
    }

    /// <summary>
    /// Completes when the server is listening and ready to accept connections.
    /// </summary>
    public Task ReadyTask => _readyTcs.Task;

    /// <summary>
    /// Completes when the server has stopped.
    /// </summary>
    public Task ServerTask => _serverTask;

    /// <summary>
    /// Waits until the server is ready to accept connections.
    /// </summary>
    public Task WaitForReadyAsync() => _readyTcs.Task;

    /// <summary>
    /// Stops the server gracefully.
    /// </summary>
    public void Stop() => _cts.Cancel();

    /// <summary>
    /// Stops the server and waits for it to finish.
    /// </summary>
    public async Task StopAsync()
    {
        _cts.Cancel();
        try
        {
            await _serverTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }
}
