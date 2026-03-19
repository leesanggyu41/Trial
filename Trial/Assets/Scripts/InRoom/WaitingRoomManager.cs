// WaitingRoomManager는 대기실에서 플레이어 목록을 관리하고, 게임 시작과 플레이어 퇴장 기능을 담당하는 클래스입니다.
// 호스트는 플레이어를 강퇴할 수 있으며, 게임을 시작할 수 있습니다
// 플레이어 목록은 실시간으로 갱신되며, 각 플레이어의 상태에 따라 UI가 업데이트됩니다. 
//또한, 네트워크 연결 상태를 모니터링하여 Runner가 끊기면 자동으로 로비로 돌아가도록 구현되어 있습니다.
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

    private void Awake() => Instance = this;

    private void Start()
    {
        // ServerConnectionManager에서 Runner 가져오기
        _runner = ServerConnectionManager.Instance.GetRunner();

        if (_runner == null)
        {
            Debug.LogError("WaitingRoomManager: Runner를 찾을 수 없음!");
            return;
        }

        if (_runner.SessionInfo.IsValid)
            roomNameText.text = $"실험명 : {_runner.SessionInfo.Name}";

        bool isHost = _runner.IsServer;
        kickButton.gameObject.SetActive(isHost);
        startButton.gameObject.SetActive(isHost);

        kickButton.onClick.AddListener(OnClickKick);
        leaveButton.onClick.AddListener(() => ServerConnectionManager.Instance.LeaveRoom());
        startButton.onClick.AddListener(OnClickStart);

        RefreshPlayerList();
    }

    private void Update()
    {
        if (_runner == null) return;

        if (_runner.IsServer)
            startButton.interactable = _runner.SessionInfo.PlayerCount >= 2;

        // Runner 끊기면 로비로
        if (!_runner.IsRunning)
        {
            _runner = null;
            ServerConnectionManager.Instance.LeaveRoom();
        }
    }

    public void RefreshPlayerList()
    {
        if (playerListContent == null || _runner == null) return;

        for (int i = playerListContent.childCount - 1; i >= 0; i--)
            Destroy(playerListContent.GetChild(i).gameObject);
        _playerItems.Clear();

        var playerDataList = FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
        foreach (var playerData in playerDataList)
        {
            if (playerData == null || playerData.gameObject == null) continue;
            if (playerData.Object == null) continue;

            GameObject go = Instantiate(playerListItemPrefab, playerListContent);
            PlayerListItem item = go.GetComponent<PlayerListItem>();

            PlayerRef playerRef = playerData.Object.InputAuthority;
            bool isSelf = playerRef == _runner.LocalPlayer;

            item.Setup(playerData, playerRef, isSelf);
            _playerItems.Add(item);
            playerData.LinkedItem = item;
        }
    }

    private void OnClickKick()
    {
        _isKickMode = !_isKickMode;
        foreach (var item in _playerItems)
            item.SetKickMode(_isKickMode);
        kickButton.image.color = _isKickMode ? Color.red : new Color(0.1098039f, 0.1098039f, 0.1098039f);
    }

    public void KickPlayer(PlayerRef playerRef)
    {
        if (_runner == null || !_runner.IsServer || playerRef == PlayerRef.None) return;
        if (playerRef == _runner.LocalPlayer) return;

        var playerDataList = FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
        foreach (var playerData in playerDataList)
        {
            if (playerData.Object == null) continue;
            if (playerData.Object.InputAuthority == playerRef)
            {
                playerData.Rpc_Kick();
                break;
            }
        }

        _isKickMode = false;
        kickButton.image.color = new Color(0.1098039f, 0.1098039f, 0.1098039f);
        foreach (var item in _playerItems)
            item.SetKickMode(false);
    }

    private void OnClickStart()
    {
        if (_runner != null && _runner.IsServer)
        {
            _runner.SessionInfo.UpdateCustomProperties
            (new Dictionary<string, SessionProperty>
                {
                    { "IsStarted", 1 }
                }
                    
            );
                _runner.SessionInfo.IsOpen = false;
        
            
            Debug.Log("게임 시작!");
            _runner.LoadScene(SceneRef.FromIndex(3));
        }
            
    }
}