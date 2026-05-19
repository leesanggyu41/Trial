using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using System.Collections;
using Fusion;

public class RobotArmFixer : NetworkBehaviour
{
    public Transform ikTarget; // Cube (Chain IK Target)
    public float moveSpeed = 1f;
    public Transform homePosition;
    public float grabAnimDuration = 0.5f; // 집는 애니메이션 지속 시간

    private Transform _grabbedItem;



    public Transform Bak;

    [Networked, OnChangedRender(nameof(OnIKTargetChanged))]
    public Vector3 NetworkedIKPosition { get; set; }



    private void LateUpdate()
    {
        if (_grabbedItem != null)
            _grabbedItem.position = ikTarget.position;
    }

    void OnIKTargetChanged()
    {
        ikTarget.position = NetworkedIKPosition;
    }
    public void MoveToItem(Transform itemTransform)
    {
        RPC_MoveToItem(itemTransform.position);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_MoveToItem(Vector3 targetPosition)
    {
        ikTarget.DOKill();
        ikTarget.DOMove(targetPosition, moveSpeed)
            .SetEase(Ease.InOutSine);
    }
    public void GrabAndReturn(Transform itemTransform, NetworkId itemId, System.Action onComplete)
    {
        StopAllCoroutines();
        StartCoroutine(GrabSequence(itemTransform.position, itemId, onComplete));
        RPC_GrabAndReturn(itemTransform.position, itemId); // 다른 클라이언트에도 실행
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_GrabAndReturn(Vector3 itemPosition, NetworkId itemId)
    {
        StopAllCoroutines();
        StartCoroutine(GrabSequence(itemPosition, itemId, null));
    }

    private IEnumerator GrabSequence(Vector3 itemPosition, NetworkId itemId, System.Action onComplete)
    {
        ikTarget.DOKill();


        yield return ikTarget.DOMove(itemPosition, moveSpeed)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        if (Runner.TryFindObject(itemId, out var itemObj))
            _grabbedItem = itemObj.transform;

        GetComponent<Animator>().SetTrigger("Grab");
        yield return new WaitForSeconds(grabAnimDuration);

        

        yield return ikTarget.DOMove(homePosition.position, moveSpeed)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

            if (_grabbedItem != null)
        {
            _grabbedItem.SetParent(null);
            _grabbedItem = null;
        }
        GetComponent<Animator>().SetTrigger("UnGrab");
        onComplete?.Invoke();

        if (Runner.IsServer)
        GameTurnManager.Instance.NowTurn = GameTurn.Player;
    }
}