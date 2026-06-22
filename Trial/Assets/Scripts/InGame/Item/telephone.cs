using UnityEngine;
using Fusion;

public class telephone : ItemBase, ReactionObject
{
    public bool NeedsTargeting => false;

    public TargetType DesiredTarget => TargetType.None;


    public void OnEvent(bool myself, NetworkId targetId)
    {
        BaseOnEvent(() =>  RPC_UseTelephone(Object.InputAuthority, targetId));
       
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseTelephone(PlayerRef player, NetworkId targetId)
    {
        if (!Runner.IsServer) return;

        SyringeTurn.ins.AddSyringes(3); // 예시로 2개의 주사기 생성

        //Runner.Despawn(Object); // 성공 시에만 아이템 삭제
    }
}
