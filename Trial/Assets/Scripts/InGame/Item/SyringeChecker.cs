using UnityEngine;
using Fusion;

public class SyringeChecker : ItemBase, ReactionObject
{
    public bool NeedsTargeting => false;
    public TargetType DesiredTarget => TargetType.None;

    public PowerPhone powerPhone;

    public void OnEvent(bool isSelfTarget, NetworkId targetId, PlayerRef usingPlayer = default)
    {
        BaseOnEvent(() => RPC_UseChecker(usingPlayer)); // ← Runner.LocalPlayer 대신 usingPlayer 사용
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseChecker(PlayerRef user)
    {
        if (!Runner.IsServer) return;

        SyringeTurn st = FindFirstObjectByType<SyringeTurn>();
        if (st == null) return;



        int toxinCount = 0;
        int nsCount = 0;

        foreach (var type in st.St)
        {
            if (type == SyringeType.Toxin) toxinCount++;
            else if (type == SyringeType.NS) nsCount++;
        }

        // 사용한 플레이어에게만 결과 전송
        RPC_ShowResult(toxinCount, nsCount, user);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowResult(int toxin, int ns, PlayerRef user)
    {
        powerPhone.Open();
        Debug.Log($"RPC_ShowResult called with Toxin: {toxin}, NS: {ns}");
        // 로컬 플레이어에게만 표시
        if (PlayerControll.Local == null) return;
        if (Runner.LocalPlayer != user) return;

        string message = $"독: {toxin}개 / 수액: {ns}개";
        if (RadioTextEffect.Instance != null)
            RadioTextEffect.Instance.ShowText(message);
        Debug.Log(message);

    }

    public void robot()
    {
        GrabAndDespawn();
    }
}