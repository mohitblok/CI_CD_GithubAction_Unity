//#define DEBUG_LOCAL
using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/// <summary>
/// LobbyManager is a singleton that manages the lobby connection.
/// </summary>
public class LobbyManager : MonoSingleton<LobbyManager>, INetworkRunnerCallbacks
{
    private NetworkRunner runner;

    private Action<List<SessionInfo>> OnSessionListUpdatedCallback;
    
    public void AddOnSessionListUpdatedCallback(Action<List<SessionInfo>> callback)
    {
        OnSessionListUpdatedCallback -= callback;
        OnSessionListUpdatedCallback += callback;
    }
    
    public void RemoveOnSessionListUpdatedCallback(Action<List<SessionInfo>> callback)
    {
        OnSessionListUpdatedCallback -= callback;
    }

    private void Connect()
    {
        if (runner == null)
        {
            GameObject go = new GameObject("Session");
            go.transform.SetParent(transform);

            runner = go.AddComponent<NetworkRunner>();
            runner.AddCallbacks(this);
        }
    }

    public void EnterLobby(string lobbyId)
    {
        Connect();
        runner.JoinSessionLobby(SessionLobby.Custom, lobbyId);
    }

    public void LeaveLobby()
    {
        if (runner != null)
        {
            runner.Shutdown();
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnPlayerJoined");
#endif
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnPlayerLeft");
#endif
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnInput");
#endif
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
#if DEBUG_LOCAL
        Debug.Log("OnInputMissing");
#endif
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnShutdown");
#endif

        if (this.runner != null && this.runner.gameObject && runner == this.runner)
            Destroy(this.runner.gameObject);

        this.runner = null;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnConnectedToServer");
#endif
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnDisconnectedFromServer");
#endif
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnConnectRequest");
#endif
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnConnectFailed");
#endif
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnUserSimulationMessage");
#endif
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnSessionListUpdated");

        foreach (var sessionInfo in sessionList)
        {
            Debug.Log($"{sessionInfo.Name} {sessionInfo.Region} {sessionInfo.MaxPlayers} {sessionInfo.PlayerCount}");
        }
#endif
        
        OnSessionListUpdatedCallback?.Invoke(sessionList);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnCustomAuthenticationResponse");
#endif
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnHostMigration");
#endif
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnReliableDataReceived");
#endif
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnSceneLoadDone");
#endif
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
#if DEBUG_LOCAL
        Debug.Log($"OnSceneLoadStart");
#endif
    }
}