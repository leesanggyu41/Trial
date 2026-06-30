using UnityEngine;
using DG.Tweening;
using Fusion;

public class Scanner : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Syringe;

    public Animator animator;

    public void OnEvent(bool isSelfTarget, NetworkId targetId, PlayerRef usingPlayer = default)
{
    OnUse();
    RPC_UseScanner(isSelfTarget, targetId, usingPlayer);
}

[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
public void RPC_UseScanner(bool isSelfTarget, NetworkId targetId, PlayerRef usingPlayer)
{
    if (!Runner.TryFindObject(targetId, out var targetObj)) return;

    SyringeItem syringe = targetObj.GetComponent<SyringeItem>();
    if (syringe == null) return;

    syringe.ScannedByPlayer = usingPlayer;
    Debug.Log($"[Scanner] ScannedByPlayer 설정됨: {syringe.ScannedByPlayer}");

    SyringeType type = syringe.MyType;
    int result = type == SyringeType.Toxin ? 1 : 0;

    RPC_PlayAnimation(result, usingPlayer);
}

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
public void RPC_PlayAnimation(int result, PlayerRef usingPlayer)
{
    animator.SetTrigger("Use");

    // 사용한 플레이어에게만 BBBScript 적용
    if (Runner.LocalPlayer != usingPlayer) return;

    BBBScript bbbScript = GetComponent<BBBScript>();
    if (bbbScript != null) bbbScript.type = result;
}

    public void robot()
    {
        GrabAndDespawn();
    }
}
