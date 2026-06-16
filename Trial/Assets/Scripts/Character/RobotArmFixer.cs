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

    private NetworkObject _grabbedNetworkObject;



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
        // 자기 자신은 직접 실행, 다른 클라이언트에만 RPC
        StartCoroutine(GrabSequence(itemTransform.position, itemId, onComplete));
        RPC_GrabAndReturn(itemTransform.position, itemId);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_GrabAndReturn(Vector3 itemPosition, NetworkId itemId)
    {
        StopAllCoroutines();
        // RPC 호출한 본인은 제외
        if (Object.HasInputAuthority) return;
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

        _grabbedNetworkObject = itemObj;

        GetComponent<Animator>().SetTrigger("Grab");
        yield return new WaitForSeconds(grabAnimDuration);



        yield return ikTarget.DOMove(homePosition.position, moveSpeed)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        if (_grabbedNetworkObject != null && Runner.IsServer)
        {
            Runner.Despawn(_grabbedNetworkObject);
        }

        if (_grabbedItem != null)
        {
            _grabbedItem.SetParent(null);
            _grabbedItem = null;
        }
        GetComponent<Animator>().SetTrigger("UnGrab");
        onComplete?.Invoke();

        if (Runner.IsServer)
            GameTurnManager.Instance.RPC_SetTurn(GameTurn.Player);

    }
}