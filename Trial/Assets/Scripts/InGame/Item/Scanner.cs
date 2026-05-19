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
        OnUse();
        Debug.Log("[Scanner] OnEvent 호출됨");
        RPC_UseScanner(isSelfTarget, targetId);
    }
    public void RPC_UseScanner(bool isSelfTarget, NetworkId targetId)
    {
        Debug.Log("[Scanner] RPC_UseScanner 서버에서 실행됨");
        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            SyringeItem syringe = targetObj.GetComponent<SyringeItem>();
            if (syringe == null) return;

            syringe.IsScanned = true;

            SyringeType type = syringe.MyType;
            int result = type == SyringeType.Toxin ? 0 : 1;

            BBBScript bbbScript = GetComponent<BBBScript>();
            bbbScript.type = result;

            Debug.Log("[Scanner] RPC_PlayAnimation 실행됨");
            RPC_PlayAnimation();
        }

        

    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayAnimation()
    {
        Debug.Log("[Scanner] RPC_PlayAnimation 실행됨");
        animator.SetTrigger("Use");
    }

    public void robot()
    {
        GrabAndDespawn();
    }
}
