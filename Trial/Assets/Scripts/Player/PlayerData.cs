using UnityEngine;
using Fusion;

public class PlayerData : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnNicknameChanged))]
    public NetworkString<_32> Nickname { get; set; }

    public PlayerListItem LinkedItem { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Nickname = NicknameManager.Instance != null
                ? NicknameManager.Instance.GetNickname()
                : $"Player_{Random.Range(1000, 9999)}";
        }

        if (WaitingRoomManager.Instance != null)
            WaitingRoomManager.Instance.RefreshPlayerList();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (LinkedItem != null)
            Destroy(LinkedItem.gameObject);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void Rpc_Kick()
    {
        Debug.Log("추방당했습니다!");
        if (ServerConnectionManager.Instance != null)
            ServerConnectionManager.Instance.LeaveRoom();
    }

    void OnNicknameChanged()
    {
        if (WaitingRoomManager.Instance != null)
            WaitingRoomManager.Instance.RefreshPlayerList();
    }
}