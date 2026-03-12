using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

public class ServerConnectionManager : MonoBehaviour
{
    public static ServerConnectionManager Instance;

    private NetworkRunner _runner;

    [Header("UI Reference")]
    [SerializeField] private GameObject errorWindow;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI statusText;
    public GameObject retryButton;

    [Header("Spawn 설정")]
    public NetworkPrefabRef playerPrefab;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        if (errorWindow != null) errorWindow.SetActive(false);
        await ConnectToServer();
    }

    // Runner 생성 공통 함수
    private NetworkRunner CreateRunner(string name = "Runner")
    {
        if (_runner != null)
        {
            Destroy(_runner.gameObject);
            _runner = null;
        }

        GameObject runnerObj = new GameObject(name);
        DontDestroyOnLoad(runnerObj);
        _runner = runnerObj.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        return _runner;
    }

    public NetworkRunner GetRunner() => _runner;

    // 로비 접속
    public async Task ConnectToServer()
    {
        if (retryButton != null) retryButton.SetActive(false);
        if (errorWindow != null) errorWindow.SetActive(false);
        UpdateStatus("Loading...");

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowError("네트워크 연결이 없습니다.");
            return;
        }

        CreateRunner("LobbyRunner");

        try
        {
            UpdateStatus("로비 접속 중...");
            var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer);

            if (result.Ok)
            {
                UpdateStatus("로비 접속 성공!");
                SceneManager.LoadScene(1);
            }
            else
            {
                ShowError($"로비 접속 실패: {result.ShutdownReason}");
            }
        }
        catch (System.Exception e)
        {
            ShowError($"오류 발생: {e.Message}");
        }
    }

    // 방 생성
    public async Task CreateRoom(string roomName, Dictionary<string, SessionProperty> customProps)
    {
        if (NicknameManager.Instance != null)
            NicknameManager.Instance.SaveNickname();

        if (_runner != null && _runner.IsRunning)
            await _runner.Shutdown();

        CreateRunner("GameRunner");

        SpawnManager spawnManager = _runner.gameObject.AddComponent<SpawnManager>();
        spawnManager.playerPrefab = playerPrefab;

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            PlayerCount = 4,
            SessionProperties = customProps,
            Scene = SceneRef.FromIndex(2),
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
            Debug.Log("방 생성 성공");
        else
        {
            Debug.LogError($"방 생성 실패: {result.ShutdownReason}");
            Destroy(_runner.gameObject);
            _runner = null;
        }
    }

    // 방 참가
    public async void JoinSession(SessionInfo session)
    {
        if (NicknameManager.Instance != null)
            NicknameManager.Instance.SaveNickname();

        string sessionName = session.Name;

        if (_runner != null && _runner.IsRunning)
        {
            await _runner.Shutdown();
            Destroy(_runner.gameObject);
            _runner = null;
            await Task.Delay(500);
        }

        CreateRunner("GameRunner");

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(2),
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
            Debug.Log("방 입장 성공!");
        else
        {
            Debug.LogError($"방 입장 실패: {result.ShutdownReason}");
            Destroy(_runner.gameObject);
            _runner = null;
        }
    }

    // 방 나가기
    public async void LeaveRoom()
    {
        if (_runner != null && _runner.IsRunning)
            await _runner.Shutdown();

        if (_runner != null)
        {
            Destroy(_runner.gameObject);
            _runner = null;
        }

        SceneManager.LoadScene(1);
    }

    public void OnRetryButtonClick() => _ = ConnectToServer();
    public void QuitGame() => Application.Quit();

    private void UpdateStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private void ShowError(string msg)
    {
        if (errorWindow != null) errorWindow.SetActive(true);
        if (errorText != null) errorText.text = msg;
        UpdateStatus("접속 실패.");
        if (retryButton != null) retryButton.SetActive(true);
        Debug.LogError(msg);
    }
}