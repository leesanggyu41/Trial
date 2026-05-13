using UnityEngine;
using DG.Tweening;
using Fusion;

public class Scanner : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Syringe;

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        
        RPC_UseScanner(isSelfTarget, targetId);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseScanner(bool isSelfTarget, NetworkId targetId)
    {
        
    }
}
