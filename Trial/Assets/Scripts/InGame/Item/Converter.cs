using UnityEngine;
using Fusion;

public class Converter : NetworkBehaviour, ReactionObject
{
    public bool NeedsTargeting => true;

    public TargetType DesiredTarget => TargetType.Syringe;

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        RPC_UseConverter(targetId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseConverter(NetworkId targetId)
    {
        if (!Runner.IsServer) return;

        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            // 주사기의 타입을 반대로 돌린다.
            var syringeScript = targetObj.GetComponent<SyringeItem>();
            if (syringeScript == null) return;

            syringeScript.MyType = syringeScript.MyType == SyringeType.Toxin ? SyringeType.NS : SyringeType.Toxin;

        }
        Runner.Despawn(Object); // 변환기도 제거
    }
}
