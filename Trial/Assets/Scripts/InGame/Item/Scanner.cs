using UnityEngine;
using DG.Tweening;
using Fusion;

public class Scanner : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Syringe;

    public Animator animator;

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {

        RPC_UseScanner(isSelfTarget, targetId);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseScanner(bool isSelfTarget, NetworkId targetId)
    {
        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            SyringeItem syringe = targetObj.GetComponent<SyringeItem>();
            if (syringe == null) return;

            SyringeType type = syringe.MyType;
            int result = type == SyringeType.Toxin ? 0 : 1;

            BBBScript bbbScript = GetComponent<BBBScript>();
            bbbScript.type = result;

            animator.SetTrigger("Use");
        }

        

    }

    public void robot()
    {
        GrabAndDespawn();
    }
}
