using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro;

public class ServerConnectionManager : MonoBehaviour
{
    private NetworkRunner _runner;

    [Header("UI Reference")]
    [SerializeField] // 대소문자 수정
    private GameObject errorWindow; // errormasege 대신 직관적인 이름으로 변경
    
    public TextMeshProUGUI errorText; // ErroeText 오타 수정
    public TextMeshProUGUI statusText; 
    public GameObject retryButton;

    async void Start()
    {
        // 시작 시 에러 창은 닫아둡니다.
        if (errorWindow != null) errorWindow.SetActive(false);
        
        await ConnectToServer();
    }

    public async Task ConnectToServer()
    {
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
            // 2. Fusion 서버 접속 시도 (MainScene 인덱스가 1번이라고 가정)
            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = "LobbyScene",
                Scene = SceneRef.FromIndex(1), 
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            // 3. 결과 판정
            if (result.Ok)
            {
                UpdateStatus("접속 성공! 입장합니다.");
            }
            else
            {
                string errorMessage = GetFriendlyErrorMessage(result.ShutdownReason);
                ShowError(errorMessage);
            }
        }
        catch (System.Exception e)
        {
            ShowError($"치명적 오류: {e.Message}");
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

    private void UpdateStatus(string msg) => statusText.text = msg;

    private void ShowError(string msg)
    {
        if (errorWindow != null) errorWindow.SetActive(true); // 에러 창 띄우기
        if (errorText != null) errorText.text = msg;         // 에러 내용 쓰기
        
        UpdateStatus("접속 실패.");
        if (retryButton != null) retryButton.SetActive(true); 
        Debug.LogError(msg);
    }
}