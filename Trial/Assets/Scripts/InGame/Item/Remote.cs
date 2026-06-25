using UnityEngine;
using Fusion;

public class Remote : ItemBase, ReactionObject
{

    public bool NeedsTargeting => false;

    public TargetType DesiredTarget => TargetType.None;

    public AudioSource audio;
    public AudioClip remoteSound;
    
    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
       BaseOnEvent(() => RPC_UseRemote());
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseRemote()
    {
        if (!Runner.IsServer) return;

        // PlayerTurn을 찾아서 스위치 반전 실행
        PlayerTurn pt = FindFirstObjectByType<PlayerTurn>();
        if (pt != null)
        {
            pt.ToggleReverse();
        }
        Invoke("PlayRemoteSound", 0.4f);
        // 사용한 리모컨 오브젝트 삭제
        //Runner.Despawn(Object);
    }

    public void PlayRemoteSound()
    {
        if (audio != null && remoteSound != null)
        {
            audio.PlayOneShot(remoteSound);
        }
    }
}
