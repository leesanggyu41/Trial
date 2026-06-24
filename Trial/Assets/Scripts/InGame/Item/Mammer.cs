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
        // 1. 망치가 주사기 위치로 이동
        yield return transform.DOMove(targetPos, moveSpeed)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        // 2. 망치 애니메이션 실행
        if (animator != null)
            animator.SetTrigger("Hit");

        // 3. 애니메이션 중간에 주사기 삭제 (서버만)
        // 애니메이션 이벤트로 OnHitComplete() 호출하면 더 정확함
        yield return new WaitForSeconds(0.3f); // 애니메이션 길이에 맞게 조절

        if (Runner.IsServer)
        {
            if (Runner.TryFindObject(syringeId, out var syringeObj))
            {
                var syringeScript = syringeObj.GetComponent<SyringeItem>();
                if (syringeScript != null)
                    SyringeTurn.ins.OnSyringeUsed(syringeId, syringeScript.MyType);
                Runner.Despawn(syringeObj);
            }
        }

        // 4. 로봇팔이 망치 가져가기
        if (Runner.IsServer)
            GrabAndDespawn();
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