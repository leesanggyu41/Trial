// PlayerData는 각 플레이어의 닉네임과 대기실 UI와의 연결을 관리하는 클래스입니다.
// 플레이어가 대기실에 입장할 때 닉네임을 설정하고, 대기실 UI에서 해당 플레이어의 정보를 표시할 수 있도록 합니다.
// 또한, 호스트가 플레이어를 강퇴할 때 해당 플레이어에게 RPC를 호출하여 게임에서 퇴장하도록 합니다.
using UnityEngine;
using Fusion;

public class PlayerData : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnNicknameChanged))]
    public NetworkString<_32> Nickname { get; set; }

    public PlayerListItem LinkedItem { get; set; }



public override void Spawned()
{
    if (HasInputAuthority)
    {       
         string nickname = NicknameManager.Instance != null
            ? NicknameManager.Instance.GetNickname()
            : $"Player_{Random.Range(1000, 9999)}";
                    //직접 설정 제거, RPC만 사용
        Rpc_SetNickname(nickname);
    }

    if (WaitingRoomManager.Instance != null)
        WaitingRoomManager.Instance.RefreshPlayerList();
}
[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
public void Rpc_SetNickname(string nickname)
{
    Nickname = nickname; // StateAuthority(서버)가 설정
}
    // 플레이어가 게임에서 퇴장할 때 해당 플레이어의 네트워크 객체를 제거하여 게임에서 사라지도록 합니다.
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
    Debug.Log($"[OnNicknameChanged] Object: {gameObject.name}, Nickname: '{Nickname}', IsEmpty: {string.IsNullOrEmpty(Nickname.ToString())}");
    if (string.IsNullOrEmpty(Nickname.ToString())) return;
    Debug.Log($"[OnNicknameChanged] Object: {gameObject.name}, Nickname: {Nickname}, InputAuthority: {Object.InputAuthority}");
    
    var controller = GetComponent<PlayerControll>();
    Debug.Log($"[OnNicknameChanged] Controller: {controller}, NameText: {controller?.NameText}");

    if (WaitingRoomManager.Instance != null)
        WaitingRoomManager.Instance.RefreshPlayerList();
}
}