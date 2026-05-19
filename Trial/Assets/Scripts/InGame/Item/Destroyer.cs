using UnityEngine;
using Fusion;
using System.Linq;

public class Destroyer : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Player;

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        OnUse();
        GrabAndDespawn();
        RPC_UseDestroyer(targetId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseDestroyer(NetworkId targetId)
    {
        if (!Runner.IsServer) return;

        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            PlayerRef targetPlayer = targetObj.InputAuthority;

            // 해당 플레이어의 아이템 목록 찾기
            var items = FindObjectsByType<ItemBase>(FindObjectsSortMode.None)
                .Where(item => item.OwnerRef == targetPlayer)
                .ToList();

            if (items.Count > 0)
            {
                // 랜덤으로 하나 선택해서 삭제
                ItemBase randomItem = items[Random.Range(0, items.Count)];
                Runner.Despawn(randomItem.Object);
                Debug.Log($"[파쇄장치] {targetPlayer} 의 아이템 파괴!");
            }
            else
            {
                Debug.Log($"[파쇄장치] {targetPlayer} 의 아이템이 없음!");
            }
        }

        //Runner.Despawn(Object);
    }
}