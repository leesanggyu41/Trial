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
    // 1. 씬에 이미 존재하는 Runner를 찾습니다 (ServerConnectionManager에서 만든 놈)
    _runner = FindFirstObjectByType<NetworkRunner>();

    if (_runner != null)
    {
        // 2. 혹시 모르니 기존 등록을 지우고 다시 등록 (중복 방지 및 확실한 연결)
        _runner.RemoveCallbacks(this); 
        _runner.AddCallbacks(this);
        
        Debug.Log("LobbyManager: 기존 Runner를 찾아 콜백을 성공적으로 등록했습니다!");

        // 3. 만약 이미 로비에 접속된 상태라면, 현재 목록을 즉시 요청하거나 
        // 서버가 보내주길 기다립니다.
        if (_runner.SessionInfo.IsValid)
        {
             Debug.Log("현재 Runner가 유효한 세션/로비 정보를 가지고 있습니다.");
        }
    }
    else
    {
        Debug.LogError("LobbyManager: 씬에서 NetworkRunner를 찾을 수 없습니다! 이전 씬에서 DontDestroyOnLoad가 되었는지 확인하세요.");
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
        Debug.Log($"방 목록 갱신됨: {sessionList.Count}개");

        // 1. 보이는 방만 필터링해서 저장
        allSessions = sessionList.Where(s => s.IsVisible).ToList();
        
        // 2. 현재 페이지에 맞게 UI 갱신
        RefreshPage();
    }

    public void RefreshPage()
    {
        // 기존 아이템 싹 지우기
        foreach (Transform child in contentTransform) Destroy(child.gameObject);

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