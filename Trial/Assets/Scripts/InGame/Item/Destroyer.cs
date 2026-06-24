using UnityEngine;
using Fusion;
using System.Collections;
using System.Linq;

public class Destroyer : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Player;

    [Header("연동 컴포넌트")]
    public AudioChange audioChange;
    [SerializeField] private Animator animator;

    private NetworkObject _targetItem;

    [Header("연출 설정")]
    [SerializeField] private Transform itemHolder; // 파쇄기 집게 사이 빈 공간 위치
    [SerializeField] private float delayBeforeDespawn = 0.5f; // Armature_Crach 애니메이션 중 아이템이 파괴되는 타이밍 (초)

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        BaseOnEvent(() => RPC_UseDestroyer(targetId));
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseDestroyer(NetworkId targetId)
    {
        if (!Runner.IsServer) return;

        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            PlayerRef targetPlayer = targetObj.InputAuthority;

            // 해당 플레이어의 아이템 목록 찾기 (자기 자신인 파쇄기는 제외)
            var items = FindObjectsByType<ItemBase>(FindObjectsSortMode.None)
                .Where(item => item.OwnerRef == targetPlayer && item != this)
                .ToList();

            if (items.Count > 0)
            {
                // 랜덤 아이템 선택 후 ID 추출
                ItemBase randomItem = items[Random.Range(0, items.Count)];
                NetworkId targetItemId = randomItem.Object.Id;

                // 모든 클라이언트에게 연출 시작 명령
                RPC_PlayDestroySequence(targetItemId);
            }
            else
            {
                Debug.Log($"[파쇄장치] {targetPlayer} 의 아이템이 없음!");
            }
        }
    }

    private void LateUpdate()
    {
        if (_targetItem != null && itemHolder != null)
            _targetItem.transform.position = itemHolder.position;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDestroySequence(NetworkId targetItemId)
    {
        StartCoroutine(DestroySequenceRoutine(targetItemId));
    }

    private IEnumerator DestroySequenceRoutine(NetworkId targetItemId)
    {
        //미리 변수 선언
        NetworkObject itemObj = null;

        if (Runner.TryFindObject(targetItemId, out itemObj))
        {
            if (itemObj.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;

            if (itemHolder != null)
            {
                itemObj.transform.SetParent(itemHolder);
                itemObj.transform.localPosition = Vector3.zero;
                itemObj.transform.localRotation = Quaternion.identity;
            }
        }
        if (Runner.TryFindObject(targetItemId, out itemObj))
        {
            _targetItem = itemObj; // LateUpdate에서 위치 강제 설정
        }

        if (animator != null) animator.SetTrigger("Crash");
        if (audioChange != null) audioChange.Open();

        yield return new WaitForSeconds(delayBeforeDespawn);

        if (audioChange != null) audioChange.Crash();

        // itemObj가 유효한지 체크 후 Despawn
        if (Runner.IsServer && itemObj != null && itemObj.IsValid)
        {
            Runner.Despawn(itemObj);
            Debug.Log("[파쇄장치] 아이템 파쇄 및 Despawn 완료");
        }
    }

    public void robot()
    {
        GrabAndDespawn();
    }
}