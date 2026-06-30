using UnityEngine;
using Fusion;
using System.Collections;

public class telephone : ItemBase, ReactionObject
{
    public bool NeedsTargeting => false;

    public TargetType DesiredTarget => TargetType.None;

    public AudioSource audio;
    public AudioClip telephoneSound;


    public void OnEvent(bool myself, NetworkId targetId, PlayerRef usingPlayer = default)
    {
        BaseOnEvent(() => RPC_PlaySoundAndUse(Object.InputAuthority, targetId));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySoundAndUse(PlayerRef player, NetworkId targetId)
    {
        //모든 클라이언트에서 사운드 재생
        if (audio != null)
            audio.PlayOneShot(telephoneSound);

        //서버만 사운드 길이만큼 기다렸다가 처리
        if (Runner.IsServer)
            StartCoroutine(WaitAndUse(player, targetId));
    }

    private IEnumerator WaitAndUse(PlayerRef player, NetworkId targetId)
    {
        // 사운드 길이만큼 대기
        float soundLength = telephoneSound != null ? telephoneSound.length : 0f;
        yield return new WaitForSeconds(soundLength);

        SyringeTurn.ins.AddSyringes(3);
    }
}
