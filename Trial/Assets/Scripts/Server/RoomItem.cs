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
        Debug.Log($"{_session.Name} 방 입장을 시도합니다.");
        // 여기에 이후 입장 로직(비밀번호 체크 등)을 연결합니다.
    }
}