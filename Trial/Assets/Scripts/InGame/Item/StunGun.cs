using UnityEngine;
using Fusion;

public class StunGun : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;

    public TargetType DesiredTarget => TargetType.Player;

    public AudioClip stunSound; // 스턴건 사용 시 재생할 사운드
    public AudioSource audioSource; // 사운드를 재생할 AudioSource

    public void OnEvent(bool isSelfTarget, NetworkId targetId, PlayerRef usingPlayer = default)
    {
        // 타겟에게 이동
        BaseOnEventToTarget(() =>
        {
            RPC_UseStunGun(targetId);
            RPC_PlaySound();
        }, targetId);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySound()
    {
        if (audioSource != null)
            audioSource.PlayOneShot(stunSound);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseStunGun(NetworkId targetId)
    {
        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            var playerData = targetObj.GetComponent<PlayerGameData>();
            if (playerData == null) return;

            // 1. 플레이어의 행동을 1턴 동안 스턴 상태로 만듭니다.
            playerData.IsStunned = true;
            playerData.gamjaun.SetActive(true); // 감전모션 활성화

            //Runner.Despawn(Object); // 아이템 사용 후 삭제

        }
    }
}
