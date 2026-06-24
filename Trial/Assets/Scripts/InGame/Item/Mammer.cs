using UnityEngine;
using Fusion;
using DG.Tweening;
using System.Collections;

public class Mammer : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Syringe;

    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float yOffset = 0.1f;

    private NetworkId _targetSyringeId;

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        _targetSyringeId = targetId;
        if (!Object.HasStateAuthority) return;
        if (TurnManager == null) return;
        if (TurnManager.NowTurn == GameTurn.Syringe) return;

        TurnManager.RPC_SetTurn(GameTurn.Animation);

        RPC_UseMammer(targetId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseMammer(NetworkId targetId)
    {
        if (!Runner.IsServer) return;

        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            var syringeScript = targetObj.GetComponent<SyringeItem>();
            if (syringeScript == null) return;

            Vector3 targetPos = targetObj.transform.position + Vector3.up * yOffset;

            // 모든 클라이언트에 연출 실행
            RPC_PlayHammerSequence(targetPos, targetId);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayHammerSequence(Vector3 targetPos, NetworkId syringeId)
    {
        StartCoroutine(HammerSequence(targetPos, syringeId));
    }

    private IEnumerator HammerSequence(Vector3 targetPos, NetworkId syringeId)
    {
        // NetworkTransform 비활성화 (있으면)
        var netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null) netTransform.enabled = false;

        yield return transform.DOMove(targetPos, moveSpeed)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        if (animator != null)
            animator.SetTrigger("Hit");

        yield return new WaitForSeconds(0.8f);

        // NetworkTransform 다시 활성화
        if (netTransform != null) netTransform.enabled = true;

        if (Runner.IsServer)
        {
            if (Runner.TryFindObject(syringeId, out var syringeObj))
            {
                var syringeScript = syringeObj.GetComponent<SyringeItem>();
                if (syringeScript != null)
                    SyringeTurn.ins.OnSyringeUsed(syringeId, syringeScript.MyType);
                Runner.Despawn(syringeObj);
            }
            GrabAndDespawn();
        }
    }

    // 애니메이션 이벤트에서 호출 (선택사항)
    public void OnHitComplete()
    {
        if (Runner.IsServer)
        {
            if (Runner.TryFindObject(_targetSyringeId, out var syringeObj))
            {
                var syringeScript = syringeObj.GetComponent<SyringeItem>();
                if (syringeScript != null)
                    SyringeTurn.ins.OnSyringeUsed(_targetSyringeId, syringeScript.MyType);
                Runner.Despawn(syringeObj);
            }
            GrabAndDespawn();
        }
    }
}