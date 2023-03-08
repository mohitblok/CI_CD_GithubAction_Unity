//#define DEBUG_MODE

using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;

public class RunnerCallbacks : MonoSingleton<RunnerCallbacks>, INetworkRunnerCallbacks
{
    public event Action<NetworkRunner, PlayerRef> onPlayerJoinCallback;
    public event Action onPlayerLeftCallback;
    public event Action<NetworkRunner, NetworkInput> onInputCallback;
    public event Action onInputMissingCallback;
    public event Action<NetworkRunner, ShutdownReason> onShutdownCallback;
    public event Action onConnectedToServerCallback;
    public event Action onDisconnectedFromServerCallback;
    public event Action onConnectRequestCallback;
    public event Action onConnectFailedCallback;
    public event Action onUserSimulationMessageCallback;
    public event Action onSessionListUpdatedCallback;
    public event Action onCustomAuthenticationResponseCallback;
    public event Action onHostMigrationCallback;
    public event Action onReliableDataReceivedCallback;
    public event Action onSceneLoadDoneCallback;
    public event Action onSceneLoadStartCallback;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
#if DEBUG_MODE
        Debug.Log($"OnPlayerJoined");
#endif

        onPlayerJoinCallback?.Invoke(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
#if DEBUG_MODE
        Debug.Log($"OnPlayerLeft");
#endif
        onPlayerLeftCallback?.Invoke();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
#if DEBUG_MODE
        Debug.Log($"OnInput");
#endif
        onInputCallback?.Invoke(runner, input);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
#if DEBUG_MODE
        Debug.Log($"OnInputMissing");
#endif
        onInputMissingCallback?.Invoke();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
#if DEBUG_MODE
        Debug.Log($"OnShutdown {shutdownReason}");
#endif
        onShutdownCallback?.Invoke(runner, shutdownReason);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
#if DEBUG_MODE
        Debug.Log($"OnConnectedToServer");
#endif
        onConnectedToServerCallback?.Invoke();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
#if DEBUG_MODE
        Debug.Log($"OnDisconnectedFromServer");
#endif
        onDisconnectedFromServerCallback?.Invoke();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
#if DEBUG_MODE
        Debug.Log($"OnConnectRequest");
#endif
        onConnectRequestCallback?.Invoke();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
#if DEBUG_MODE
        Debug.Log($"OnConnectFailed {reason}");
#endif
        onConnectFailedCallback?.Invoke();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
#if DEBUG_MODE
        Debug.Log($"OnUserSimulationMessage");
#endif
        onUserSimulationMessageCallback?.Invoke();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
#if DEBUG_MODE
        Debug.Log($"OnSessionListUpdated");
#endif
        onSessionListUpdatedCallback?.Invoke();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
#if DEBUG_MODE
        Debug.Log($"OnCustomAuthenticationResponse");
#endif
        onCustomAuthenticationResponseCallback?.Invoke();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
#if DEBUG_MODE
        Debug.Log($"OnHostMigration");
#endif
        onHostMigrationCallback?.Invoke();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
#if DEBUG_MODE
        Debug.Log($"OnReliableDataReceived");
#endif
        onReliableDataReceivedCallback?.Invoke();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
#if DEBUG_MODE
        Debug.Log($"OnSceneLoadDone");
#endif
        onSceneLoadDoneCallback?.Invoke();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
#if DEBUG_MODE
        Debug.Log($"OnSceneLoadStart");
#endif
        onSceneLoadStartCallback?.Invoke();
    }
}