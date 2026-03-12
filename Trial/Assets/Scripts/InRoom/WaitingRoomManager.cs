using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;

public class WaitingRoomManager : MonoBehaviour
{
    public static WaitingRoomManager Instance;

    [Header("UI 연결")]
    public Button startButton;
    public Button leaveButton;
    public Button kickButton;
    public Transform playerListContent;
    public GameObject playerListItemPrefab;
    public TMP_Text roomNameText;

    private bool _isKickMode = false;
    private NetworkRunner _runner;
    private List<PlayerListItem> _playerItems = new List<PlayerListItem>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _runner = FindFirstObjectByType<NetworkRunner>();

        if (_runner == null)
        {
            Debug.LogError("WaitingRoomManager: Runner를 찾을 수 없음!");
            return;
        }

        // 방 이름 표시
        if (_runner.SessionInfo.IsValid)
            roomNameText.text = $"실험명 : {_runner.SessionInfo.Name}";

        // 방장 여부에 따라 UI 설정
        bool isHost = _runner.IsServer;
        kickButton.gameObject.SetActive(isHost);
        startButton.gameObject.SetActive(isHost);

        // 버튼 이벤트 연결
        kickButton.onClick.AddListener(OnClickKick);
        startButton.onClick.AddListener(OnClickStart);

        RefreshPlayerList();
    }

    private void Update()
    {
        if (_runner == null) return;

        // 2인 이상일 때만 시작 버튼 활성화
        if (_runner.IsServer)
            startButton.interactable = _runner.SessionInfo.PlayerCount >= 2;
    }

    public void RefreshPlayerList()
    {
        if (_runner == null || playerListContent == null) return;

        // 기존 아이템 삭제
        for (int i = playerListContent.childCount - 1; i >= 0; i--)
            Destroy(playerListContent.GetChild(i).gameObject);
        _playerItems.Clear();

        // 현재 씬의 모든 PlayerObject 찾아서 목록 생성
        var playerObjects = FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
        foreach (var playerData in playerObjects)
        {
            GameObject go = Instantiate(playerListItemPrefab, playerListContent);
            PlayerListItem item = go.GetComponent<PlayerListItem>();

            // 자기 자신은 추방 불가
            bool isSelf = playerData.HasStateAuthority;
            if (isSelf)
                item.Setup(playerData, PlayerRef.None);
            else
                item.Setup(playerData, playerData.Runner.LocalPlayer);

            _playerItems.Add(item);
        }
    }

    //  추방 모드 토글
    private void OnClickKick()
    {
        _isKickMode = !_isKickMode;

        foreach (var item in _playerItems)
            item.SetKickMode(_isKickMode);

        // 버튼 색으로 추방 모드 표시
        kickButton.image.color = _isKickMode ? Color.red : Color.white;
    }

    // 플레이어 추방
    public void KickPlayer(PlayerRef playerRef)
    {
        if (_runner == null || !_runner.IsServer || playerRef == PlayerRef.None) return;

        Debug.Log($"플레이어 추방: {playerRef.PlayerId}");
        _runner.Disconnect(playerRef);

        // 추방 후 킥 모드 해제
        _isKickMode = false;
        kickButton.image.color = Color.white;
        foreach (var item in _playerItems)
            item.SetKickMode(false);
    }

    //  게임 시작 (방장만)
    private void OnClickStart()
    {
        if (_runner != null && _runner.IsServer)
        {
            Debug.Log("게임 시작!");
            // 나중에 게임 씬으로 이동 로직 추가
        }
    }
}