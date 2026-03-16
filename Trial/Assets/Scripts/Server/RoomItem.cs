// RoomItem은 로비에서 각 방의 정보를 표시하는 UI 요소입니다.
// 방 제목, 현재 인원 수, 최대 인원 수를 표시하고, 비밀번호가 설정된 방에는 자물쇠 아이콘을 활성화하여 시각적으로 구분합니다.
// 또한, 입장 버튼을 클릭하면 해당 방의 비밀번호 설정 여부에 따라 PasswordPopupManager를 통해 비밀번호 입력을 요구하거나 바로 입장하도록 ServerConnectionManager에 요청하는 역할을 합니다.
using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI roomNameText;    // 방 제목
    public TextMeshProUGUI playerCountText; // 인원 (1/4)
    public GameObject lockIcon;            // 자물쇠 이미지 오브젝트

    private SessionInfo _session;

    // LobbyManager에서 데이터를 넣어주는 함수
    public void Setup(SessionInfo session)
    {
        _session = session;
        roomNameText.text = session.Name;
        playerCountText.text = $"{session.PlayerCount} / {session.MaxPlayers}";

        // 방 생성 시 넣었던 "IsPassword" 속성 확인 (1이면 비밀번호 방)
        if (session.Properties.TryGetValue("IsPassword", out var isPasswordProp))
        {
            bool isPassword = (int)isPasswordProp == 1;
            if (lockIcon != null) lockIcon.SetActive(isPassword);
        }
        else
        {
            if (lockIcon != null) lockIcon.SetActive(false);
        }
    }

    // 버튼 클릭 시 호출 (Unity Button 이벤트에 연결)
    public void OnClickJoin()
{
    // 비밀번호 정보 추출
    bool isPasswordProtected = false;
    string correctPassword = "";

    if (_session.Properties.TryGetValue("IsPassword", out var isPassProp))
        isPasswordProtected = (int)isPassProp == 1;

    if (isPasswordProtected && _session.Properties.TryGetValue("PwData", out var pwProp))
        correctPassword = (string)pwProp;

    // 분기 처리
    if (isPasswordProtected)
    {
        // 비밀번호 팝업을 띄움
        PasswordPopupManager.Instance.OpenPasswordPanel(_session, correctPassword);
    }
    else
    {
        // 바로 입장 (매니저 호출)
        ServerConnectionManager.Instance.JoinSession(_session);
    }
}
}