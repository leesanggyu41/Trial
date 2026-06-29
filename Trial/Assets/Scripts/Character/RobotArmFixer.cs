using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using System.Collections;
using Fusion;

public class RobotArmFixer : NetworkBehaviour
{
    public Transform ikTarget;
    public float moveSpeed = 1f;
    public Transform homePosition;
    public float grabAnimDuration = 0.5f;

    private Transform _grabbedItem;
    private NetworkObject _grabbedNetworkObject;
    private Coroutine _currentSequence; // 단일 코루틴 추적

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

    public void GrabAndReturn(Transform itemTransform, NetworkId itemId, System.Action onComplete)
    {
        // 서버만 전체 흐름을 제어
        if (!Runner.IsServer) return;

        if (_currentSequence != null)
            StopCoroutine(_currentSequence);

        // 클라이언트에게 시각적 동작만 지시
        RPC_StartGrabVisual(itemTransform.position);

        // 서버는 콜백 포함 전체 시퀀스 실행
        _currentSequence = StartCoroutine(GrabSequence(itemTransform.position, itemId, onComplete));
    }

    // 클라이언트용: 시각 효과만 (Despawn/콜백 없음)
    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    private void RPC_StartGrabVisual(Vector3 itemPosition)
    {
        if (_currentSequence != null)
            StopCoroutine(_currentSequence);

        _currentSequence = StartCoroutine(GrabVisualOnly(itemPosition));
    }

    private IEnumerator GrabVisualOnly(Vector3 itemPosition)
    {
        ikTarget.DOKill();

        yield return ikTarget.DOMove(itemPosition, moveSpeed)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        GetComponent<Animator>().SetTrigger("Grab");
        yield return new WaitForSeconds(grabAnimDuration);

        yield return ikTarget.DOMove(homePosition.position, moveSpeed)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        GetComponent<Animator>().SetTrigger("UnGrab");
    }

    // 서버 전용: 실제 로직 + 콜백
    private IEnumerator GrabSequence(Vector3 itemPosition, NetworkId itemId, System.Action onComplete)
{
    Debug.Log($"[GrabSequence] 시작 - itemId: {itemId}");
    
    ikTarget.DOKill();

    yield return ikTarget.DOMove(itemPosition, moveSpeed)
        .SetEase(Ease.InOutSine)
        .WaitForCompletion();

    Debug.Log($"[GrabSequence] 아이템 위치 도착 - TryFindObject 시도");

    if (Runner.TryFindObject(itemId, out var itemObj))
    {
        Debug.Log($"[GrabSequence] 아이템 찾음: {itemObj.name}");
        _grabbedItem = itemObj.transform;
        _grabbedNetworkObject = itemObj;
    }
    else
    {
        Debug.LogError($"[GrabSequence] 아이템 못 찾음! itemId: {itemId} ← 여기가 문제면 타이밍 이슈");
    }

    GetComponent<Animator>().SetTrigger("Grab");
    yield return new WaitForSeconds(grabAnimDuration);

    Debug.Log($"[GrabSequence] 홈으로 복귀 시작");

    yield return ikTarget.DOMove(homePosition.position, moveSpeed)
        .SetEase(Ease.InOutSine)
        .WaitForCompletion();

    Debug.Log($"[GrabSequence] 홈 도착 - Despawn 시도: {_grabbedNetworkObject != null}");

    if (_grabbedNetworkObject != null)
    {
        Runner.Despawn(_grabbedNetworkObject);
        _grabbedNetworkObject = null;
    }

    _grabbedItem = null;
    GetComponent<Animator>().SetTrigger("UnGrab");
    onComplete?.Invoke();
    GameTurnManager.Instance?.RPC_SetTurn(GameTurn.Player);
    _currentSequence = null;
    
    Debug.Log($"[GrabSequence] 완료");
}
}