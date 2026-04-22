using UnityEngine;
using Fusion;

public class StunGun : NetworkBehaviour, ReactionObject
{
    public bool NeedsTargeting => true;

    public TargetType DesiredTarget => TargetType.Player;

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        RPC_UseStunGun(targetId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseStunGun(NetworkId targetId)
    {
        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            var playerData = targetObj.GetComponent<PlayerGameData>();
            if (playerData == null) return;

            // 1. 플레이어의 행동을 1턴 동안 스턴 상태로 만듭니다.
            playerData.IsStunned = true; // IsStunned은 PlayerGameData에 추가된 변수라고 가정합니다.

            Runner.Despawn(Object); // 아이템 사용 후 삭제

        }
    }
}
