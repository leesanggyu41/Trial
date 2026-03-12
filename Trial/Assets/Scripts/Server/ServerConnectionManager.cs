using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro;

public class ServerConnectionManager : MonoBehaviour
{
    public static ServerConnectionManager Instance;


    private NetworkRunner _runner;

    [Header("UI Reference")]
    [SerializeField] // 대소문자 수정
    private GameObject errorWindow; // errormasege 대신 직관적인 이름으로 변경
    
    public TextMeshProUGUI errorText; // ErroeText 오타 수정
    public TextMeshProUGUI statusText; 
    public GameObject retryButton;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 2. 이게 유일하다면? 내가 대장!
        Instance = this;

        // 3. [핵심] 씬이 바뀌어도 나랑 내 몸에 붙은 NetworkRunner는 삭제하지 마라!
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        // 시작 시 에러 창은 닫아둡니다.
        if (errorWindow != null) errorWindow.SetActive(false);
        
        await ConnectToServer();
        
    }

    public async Task ConnectToServer()
    {
        if (_runner == null)
        {
            // 자기 gameObject 말고 새 오브젝트에 붙이기
            GameObject runnerObj = new GameObject("LobbyRunner");
            DontDestroyOnLoad(runnerObj);
            _runner = runnerObj.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }
        // 1. 초기 UI 세팅
        if (retryButton != null) retryButton.SetActive(false);
        if (errorWindow != null) errorWindow.SetActive(false);
        UpdateStatus("Loading...");

        // 인터넷 연결 여부 확인
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowError("네트워크 연결이 없습니다.");
            return;
        }

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        try
{
    UpdateStatus("서버 접속 및 로비 진입 시도 중...");

    // 1. 방을 만드는 게 아니라 '로비'에 접속합니다.
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
    public void OnRetryButtonClick()
    {
    
    _ = ConnectToServer(); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    private string GetFriendlyErrorMessage(ShutdownReason reason)
    {
        switch (reason)
        {
            case ShutdownReason.CustomAuthenticationFailed: return "인증 실패: AppID를 확인하세요.";
            case ShutdownReason.InvalidAuthentication: return "잘못된 인증입니다.";
            case ShutdownReason.ConnectionTimeout: return "서버 응답 시간 초과.";
            case ShutdownReason.PhotonCloudTimeout: return "클라우드 연결 지연.";
            case ShutdownReason.DisconnectedByPluginLogic: return "플러그인에 의해 차단됨.";
            default: return $"연결 오류 (Code: {reason})";
        }
    }

    private void UpdateStatus(string msg)
    {
        if (statusText != null) // null 체크
        statusText.text = msg;
    }


    private void ShowError(string msg)
    {
        if (errorWindow != null) errorWindow.SetActive(true); // 에러 창 띄우기
        if (errorText != null) errorText.text = msg;         // 에러 내용 쓰기
        
        UpdateStatus("접속 실패.");
        if (retryButton != null) retryButton.SetActive(true); 
        Debug.LogError(msg);
    }


    public async void JoinSession(SessionInfo session)
{
    if (NicknameManager.Instance != null)
        NicknameManager.Instance.SaveNickname();
    Debug.Log($"{session.Name} 방으로 입장을 시작합니다.");

    // 1. 현재 로비에 접속된 러너(나 자신 혹은 변수)를 종료
    if (_runner != null)
    {
        await _runner.Shutdown();
        // Shutdown 후 기존 오브젝트를 파괴하거나 재사용할 수 있지만, 
        // 깨끗한 상태를 위해 새로 만드는 방식을 추천합니다.
        Destroy(_runner.gameObject);
        _runner = null;
    }

    // 2. 새로운 게임용 러너 생성
    GameObject go = new GameObject("GameRunner");
    _runner = go.AddComponent<NetworkRunner>();
    DontDestroyOnLoad(go); // 게임 씬에서도 살아있어야 함

    // 3. 게임 모드로 시작 (Client)
    var result = await _runner.StartGame(new StartGameArgs()
    {
        GameMode = GameMode.Client,
        SessionName = session.Name,
        Scene = SceneRef.FromIndex(2), // 실제 게임 씬 인덱스
        SceneManager = go.AddComponent<NetworkSceneManagerDefault>()
    });

    if (result.Ok)
    {
        Debug.Log("방 입장 성공!");
    }
    else
    {
        Debug.LogError($"방 입장 실패: {result.ShutdownReason}");
        // 실패 시 다시 로비로 돌아오는 처리가 필요할 수 있습니다.
    }
}
public async void LeaveRoom()
{
    if (_runner != null && _runner.IsRunning)
    {
        await _runner.Shutdown();

        if (_runner != null)
        {
            
            _runner = null;
        }
        _runner = null;
    }

    
    // Shutdown 완료 후 로비로 이동
    SceneManager.LoadScene(1);

}
public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
{
    // 추방당하거나 연결 끊기면 로비로 이동
    if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic ||
        shutdownReason == ShutdownReason.Ok)
    {
        if (ServerConnectionManager.Instance != null)
            ServerConnectionManager.Instance.LeaveRoom();
    }
}
}