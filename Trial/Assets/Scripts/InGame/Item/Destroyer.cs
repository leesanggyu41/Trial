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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDestroySequence(NetworkId targetItemId)
    {
        StartCoroutine(DestroySequenceRoutine(targetItemId));
    }

    private IEnumerator DestroySequenceRoutine(NetworkId targetItemId)
    {
        // 1. 맵에서 해당 아이템 오브젝트 탐색 및 집게 위치로 강제 이동
        if (Runner.TryFindObject(targetItemId, out var itemObj))
        {
            if (itemObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true; // 물리 엔진 잠시 끄기
            }

            if (itemHolder != null)
            {
                itemObj.transform.SetParent(itemHolder);
                itemObj.transform.localPosition = Vector3.zero;
                itemObj.transform.localRotation = Quaternion.identity;
            }
        }

        // 2. 파쇄 시작 애니메이션 실행 및 소리 재생
        if (animator != null) 
        {
            animator.SetTrigger("Crash"); // 애니메이터의 'Crash' 트리거 작동
        }
        
        if (audioChange != null) 
        {
            audioChange.Open(); // 작동 시작 사운드
        }

        // 3. 집게가 마구 씹히는(Armature_Crach) 도중, 아이템이 파괴될 타이밍까지 대기
        yield return new WaitForSeconds(delayBeforeDespawn);

        // 4. 파티클 펑! 터지고 와작하는 Crash 사운드 재생
        if (audioChange != null) 
        {
            audioChange.Crash(); 
        }

        // 5. 서버에서 최종적으로 아이템 Despawn (화면에서 제거)
        if (Runner.IsServer && itemObj != null)
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