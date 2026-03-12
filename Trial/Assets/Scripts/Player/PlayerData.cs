using UnityEngine;
using Fusion;

public class PlayerData : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnNicknameChanged))]
    public NetworkString<_32> Nickname { get; set; }

    public override void Spawned()
    {
        // 자기 자신이면 닉네임 설정
        if (HasStateAuthority)
        {
            Nickname = NicknameManager.Instance != null
                ? NicknameManager.Instance.GetNickname()
                : $"Player_{Random.Range(1000, 9999)}";
        }
    }

    void OnNicknameChanged()
    {
        if (WaitingRoomManager.Instance != null)
            WaitingRoomManager.Instance.RefreshPlayerList();
    }
}