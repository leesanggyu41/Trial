using UnityEngine;
using Fusion;

public class Stimulant : NetworkBehaviour, ReactionObject
{
    public bool NeedsTargeting => false;
    public TargetType DesiredTarget => TargetType.None;

    public void OnEvent(bool myself, NetworkId targetId)
    {
        RPC_UseStimulant(Object.InputAuthority, targetId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseStimulant(PlayerRef player, NetworkId targetId)
    {
        if (!Runner.IsServer) return;

        PlayerGameData targetData = null;
        if (Runner.TryFindObject(targetId, out NetworkObject targetObj))
            targetData = targetObj.GetComponent<PlayerGameData>();

        // 3. 최종 처리
        if (targetData != null)
        {
            targetData.IsAwakening = true;
            Debug.Log($"[성공] {player.PlayerId}님 각성 완료.");
            Runner.Despawn(Object); // 성공 시에만 아이템 삭제
        }
    }
}

