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

    private void OnClickKick()
    {
        if (_isKickMode && WaitingRoomManager.Instance != null)
            WaitingRoomManager.Instance.KickPlayer(_playerRef);
    }
}