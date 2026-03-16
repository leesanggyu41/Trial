// PlayerListItem는 대기실에서 각 플레이어의 정보를 표시하는 UI 요소입니다.
// 플레이어의 닉네임을 표시하고, 호스트가 추방 모드일 때 해당 플레이어를 강퇴할 수 있도록 버튼을 활성화합니다.
// 또한, 자신인 플레이어는 강퇴 버튼이 비활성화되어 추방되지 않도록 처리합니다.
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class PlayerListItem : MonoBehaviour
{
    public TMP_Text nicknameText;
    public Button itemButton;

    private PlayerRef _playerRef;
    private bool _isKickMode = false;
    private bool _isSelf = false;

    public void Setup(PlayerData playerData, PlayerRef playerRef, bool isSelf)
    {
        _playerRef = playerRef;
        _isSelf = isSelf;
        nicknameText.text = playerData.Nickname.ToString();
        itemButton.interactable = false;

        itemButton.onClick.AddListener(OnClickKick);

    }

    // 추방 모드 토글
    public void SetKickMode(bool isKickMode)
    {
        itemButton.interactable = isKickMode && !_isSelf;
        _isKickMode = isKickMode;
        itemButton.interactable = isKickMode;

    }
    // 강퇴 버튼 클릭 시 호출되는 메서드입니다. 추방 모드일 때만 작동하며, 호스트가 해당 플레이어를 강퇴하도록 WaitingRoomManager에 요청합니다.
    private void OnClickKick()
    {
        if (_isKickMode && WaitingRoomManager.Instance != null)
            WaitingRoomManager.Instance.KickPlayer(_playerRef);
    }
}