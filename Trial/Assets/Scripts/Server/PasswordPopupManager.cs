// PasswordPopupManager는 비밀번호가 설정된 방에 입장하려고 할 때 나타나는 팝업을 관리하는 클래스입니다.
// RoomItem에서 입장 버튼을 클릭하면 OpenPasswordPanel()이 호출되어 해당 방의 정보를 받아와 팝업을 띄우고, 사용자가 비밀번호를 입력하여 확인 버튼을 누르면 OnClickConfirm()이 호출되어 비밀번호를 검증한 후 입장 여부를 결정합니다.
// 또한, 취소 버튼을 누르면 OnClickCancel()이 호출되어 팝업이 닫히도록 처리합니다.
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
    public GameObject waitpanal;        // 입장 대기 패널

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

    public void ClosePasswordPanel()
    {
        inputField.text = "";
        errorText.text = "";
        panel.SetActive(false);
    }

    // [확인] 버튼에 연결할 함수
    public void OnClickConfirm()
    {
        if (inputField.text == _correctPassword)
        {
            Debug.Log("비밀번호 일치! 입장을 시작합니다.");
            panel.SetActive(false);
            waitpanal.SetActive(true);
            // RoomItem에 만들어둔 입장 정적 함수 호출
            ServerConnectionManager.Instance.JoinSession(_targetSession);
        }
        else
        {
            errorText.text = "비밀번호가 틀렸습니다.";
            inputField.text = "";
            Invoke(nameof(ClearError), 2f); // 2초 후에 오류 메시지 제거
        }
    }

    private void ClearError()
    {
        errorText.text = "";
    }

    // [취소] 버튼에 연결
    public void OnClickCancel()
    {
        panel.SetActive(false);
    }
}