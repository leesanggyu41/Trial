using UnityEngine;
using TMPro;
using Fusion;

public class PasswordPopupManager : MonoBehaviour
{
    public static PasswordPopupManager Instance;

    [Header("UI 연결")]
    public GameObject panel;            // 비밀번호 팝업 전체 부모 오브젝트
    public TMP_InputField inputField;   // 사용자가 비번을 치는 곳
    public TextMeshProUGUI errorText;   // "틀렸습니다" 표시용

    private SessionInfo _targetSession;
    private string _correctPassword;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false); // 처음엔 닫아둠
    }

    // RoomItem에서 호출하는 함수
    public void OpenPasswordPanel(SessionInfo session, string correctPw)
    {
        _targetSession = session;
        _correctPassword = correctPw;
        
        inputField.text = "";
        errorText.text = "";
        panel.SetActive(true);
    }

    // [확인] 버튼에 연결할 함수
    public void OnClickConfirm()
    {
        if (inputField.text == _correctPassword)
        {
            Debug.Log("비밀번호 일치! 입장을 시작합니다.");
            panel.SetActive(false);
            
            // RoomItem에 만들어둔 입장 정적 함수 호출
            ServerConnectionManager.Instance.JoinSession(_targetSession);
        }
        else
        {
            errorText.text = "비밀번호가 틀렸습니다.";
            inputField.text = "";
        }
    }

    // [취소] 버튼에 연결
    public void OnClickCancel()
    {
        panel.SetActive(false);
    }
}