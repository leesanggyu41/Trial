using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

public class NetworkCallbackHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    private void Start()
    {
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null)
            runner.AddCallbacks(this);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        Debug.Log($"Player {player} has left the game.");
        // 나간 플레이어의 NetworkObject 찾기
        if (GameSceneManager.Instance._spawnedPlayers.TryGetValue(player, out var playerObj))
        {
            // 나간 플레이어를 사망 처리
            PlayerGameData playerData = playerObj.GetComponent<PlayerGameData>();
            if (playerData != null && !playerData.IsDead)
            {
                playerData.IsDead = true;
            }

            // 플레이어 턴에서 제거
            PlayerControll pc = playerObj.GetComponent<PlayerControll>();
            if (pc != null)
            {
                GameTurnManager.Instance.Pt_T.DeletePlayer(pc);
            }

            // NetworkObject 제거
            runner.Despawn(playerObj);
            GameSceneManager.Instance._spawnedPlayers.Remove(player);
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        SceneManager.LoadScene("LobbyScene");
    }

    // 나머지 빈 구현
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}