// LobbyManager는 대기실에서 방 목록을 관리하는 클래스입니다.
// 방 목록을 서버에서 받아와서 UI에 표시하고, 
//페이지네이션 기능을 제공하여 여러 페이지에 걸쳐 방을 탐색할 수 있도록 합니다.\
// 또한, 방 목록이 업데이트될 때마다 UI를 새로고침하여 최신 상태를 유지합니다.
using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Fusion.Sockets;
using System;
using System.Linq;
using TMPro;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static LobbyManager Instance;

    [Header("UI 설정")]
    public GameObject roomItemPrefab;
    public Transform contentTransform;
    public TMP_Text page;
    public int itemsPerPage = 8;

    private int currentPage = 0;
    private List<SessionInfo> allSessions = new List<SessionInfo>();
    private NetworkRunner _runner;

    private void Awake() => Instance = this;

    private void Start()
    {
        // ServerConnectionManager에서 Runner 가져오기
        _runner = ServerConnectionManager.Instance.GetRunner();

        if (_runner != null)
        {
            _runner.RemoveCallbacks(this);
            _runner.AddCallbacks(this);
        }
        else
        {
            _ = ServerConnectionManager.Instance.ConnectToServer();
        }
    }

    private void Update()
    {
        // 전체 페이지 수 = 방 개수 / 페이지당 개수 (예전에는 방 개수를 그대로 표시했음)
        int totalPages = Mathf.Max(1, Mathf.CeilToInt(allSessions.Count / (float)itemsPerPage));
        if (page != null) page.text = $"{currentPage + 1}/{totalPages}";
    }
    // 방 목록이 업데이트될 때마다 호출되는 콜백 메서드입니다. 받은 방 목록에서 유효한 방만 필터링하여 저장하고, UI를 새로고침합니다.
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // (예전에는 빈 목록을 무시해서, 마지막 방이 사라져도 이미 없는 방이 계속 표시됐음)
        allSessions = sessionList.Where(s => s.IsValid && s.IsVisible).ToList();

        // 방이 줄어 현재 페이지가 비면 마지막 페이지로
        int maxPage = Mathf.Max(0, (allSessions.Count - 1) / itemsPerPage);
        if (currentPage > maxPage) currentPage = maxPage;

        RefreshPage();
    }

    public void RefreshPage()
    {
        if (contentTransform == null) return;

        for (int i = contentTransform.childCount - 1; i >= 0; i--)
            Destroy(contentTransform.GetChild(i).gameObject);

        if (allSessions.Count == 0) return;

        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, allSessions.Count);

        for (int i = startIndex; i < endIndex; i++)
            CreateRoomItem(allSessions[i]);
    }

    private void CreateRoomItem(SessionInfo session)
    {
        GameObject go = Instantiate(roomItemPrefab, contentTransform);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchoredPosition3D = Vector3.zero;

        RoomItem item = go.GetComponent<RoomItem>();
        if (item != null) item.Setup(session);
    }
    
    public async void OnClickRefresh()
    {
        if (_runner == null  || !_runner.IsCloudReady) return;
        Debug.Log("방리스트 새로고침");
        _runner.RemoveCallbacks(this);
        _runner.AddCallbacks(this);
        _ = await _runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public void OnClickNextPage()
    {
        if ((currentPage + 1) * itemsPerPage < allSessions.Count)
        {
            currentPage++;
            RefreshPage();
        }
    }

    public void OnClickPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    // 빈 콜백
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnSceneLoadDone(NetworkRunner runner) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}
}