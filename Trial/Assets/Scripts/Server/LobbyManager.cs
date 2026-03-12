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
    [Header("페이지 설정")]
    public TMP_Text page;
    public int itemsPerPage = 8;
    private int currentPage = 0;
    private List<SessionInfo> allSessions = new List<SessionInfo>();

    private NetworkRunner _runner;
    

    private void Awake() => Instance = this;

private void Start()
{
    _runner = FindFirstObjectByType<NetworkRunner>();

    if (_runner != null)
    {
        // 항상 새로 등록 (씬 재로드 후 새 인스턴스니까)
        _runner.RemoveCallbacks(this);
        _runner.AddCallbacks(this);
        Debug.Log("LobbyManager: Runner 등록 완료");
    }
    else
    {
        Debug.Log("LobbyManager: Runner 없음 - 로비 재접속 시도");
        ServerConnectionManager.Instance.ConnectToServer();
    }
}
    // [참가자용] 로비에 접속하기 위한 함수
    public async void JoinLobby()
    {
        // 1. 기존 러너가 있다면 파괴 (새로 시작)
        if (_runner != null) Destroy(_runner.gameObject);

        // 2. 새로운 러너 생성
        GameObject go = new GameObject("LobbyRunner");
        _runner = go.AddComponent<NetworkRunner>();
        _runner.AddCallbacks(this);

        // 3. 로비 입장
        var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer);
        
        if (result.Ok) Debug.Log("로비 접속 성공!");
        else Debug.LogError($"로비 접속 실패: {result.ShutdownReason}");
    }

    // [핵심] 방 목록이 바뀔 때마다 실행됨 (인원수 변경 포함)
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
        if (sessionList.Count == 0 && allSessions.Count > 0) return;
        Debug.Log($"방 목록 갱신됨: {sessionList.Count}개");

        // 1. 보이는 방만 필터링해서 저장
        allSessions = sessionList.Where(s => s.IsVisible).ToList();
        
        // 2. 현재 페이지에 맞게 UI 갱신
        RefreshPage();
    }

    public void RefreshPage()
    {
        if (contentTransform == null) return;
        // 기존 아이템 싹 지우기
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
    {
        Destroy(contentTransform.GetChild(i).gameObject);
    }
    if (allSessions.Count == 0)
    {
        // 방 없을 때 처리 (빈 텍스트 표시 등)
        Debug.Log("현재 방이 없습니다.");
        return;
    }

        // 현재 페이지에 보여줄 시작 인덱스와 끝 인덱스 계산
        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, allSessions.Count);

        // 8개만 생성
        for (int i = startIndex; i < endIndex; i++)
        {
            CreateRoomItem(allSessions[i]);
        }
    }

    private void CreateRoomItem(SessionInfo session)
    {
        GameObject go = Instantiate(roomItemPrefab, contentTransform);
        
        // UI 정렬 및 크기 고정 (중요)
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchoredPosition3D = Vector3.zero;

        RoomItem item = go.GetComponent<RoomItem>();
        if (item != null) item.Setup(session);
    }
    public async void OnClickRefresh()
{
    if (_runner == null || !_runner.IsRunning)
    {
        RefreshPage();
    }
    _runner.RemoveCallbacks(this);
    _runner.AddCallbacks(this);

    // 로비 재접속으로 세션 목록 강제 갱신
    await _runner.JoinSessionLobby(SessionLobby.ClientServer);
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

    private void Update()
    {
        page.text = (currentPage+1).ToString() + "/" + allSessions.Count.ToString();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
       
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
       
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
       
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
       
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
       
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
}