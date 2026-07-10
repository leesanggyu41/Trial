// PlayerGameData는 플레이어의 체력과 관련된 게임 데이터를 관리하는 클래스입니다.
// 플레이어의 체력을 네트워크로 동기화하여 모든 클라이언트에서 일관된 상태를 유지하며,
// 체력 변화에 따른 UI 업데이트를 처리합니다. 또한, IDamageable 인터페이스를 구현하여 다른 오브젝트로부터 피해를 받을 수 있도록 합니다.
using Fusion;
using UnityEngine;
using System.Linq;
using System.Collections;

public class PlayerGameData : NetworkBehaviour, IDamageable
{
    public int MaxHP => 4;
    private static bool _winTriggered = false;
    public static void ResetWinFlag() => _winTriggered = false;
    [Networked] public int BonusItemCount { get; set; } = 0; // NS 자가 사격 보너스

    [Networked] public bool IsAwakening { get; set; } = false; // 각성 상태

    [Networked] public bool IsStunned { get; set; } = false; // 스턴 상태

    [Networked, OnChangedRender(nameof(OnIsDeadChanged))]
    public bool IsDead { get; set; } = false; // 사망 상태

    [Networked, OnChangedRender(nameof(OnHPChanged))]
    public int HP { get; set; }

    public GameObject gamjaun; // 감전모션

    [Header("주사기 표시")]
    public GameObject syringeModel; // 처음엔 비활성화

    public override void Spawned()
    {
        if (Runner.IsServer)
            HP = MaxHP;
    }

    public void TakeDamage(int damage)
    {
        if (!Runner.IsServer) return;
        HP = Mathf.Max(0, HP - damage);

        if (HP <= 0)
        {
            Debug.Log($"{gameObject.name} 탈락!");
            IsDead = true;
        }

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowSyringeHit()
    {
        if (syringeModel != null)
        {
            syringeModel.SetActive(true);
            StartCoroutine(HideSyringeAfterDelay());
        }
    }

    private IEnumerator HideSyringeAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (syringeModel != null)
            syringeModel.SetActive(false);
    }

    void OnIsDeadChanged()
    {
        PlayerControll pc = GetComponent<PlayerControll>();
        string myNickname = pc.NameText.text; // 지우기 전에 먼저 백업

        if (GetComponent<NetworkObject>().HasInputAuthority && IsDead)
        {
            var deathUI = DeathUIManager.Instance;
            if (deathUI != null)
                deathUI.ShowDeathUI();
        }

        if (Runner.IsServer && IsDead)
        {
            PlayerTurn pt = GameTurnManager.Instance?.Pt_T;
            if (pt != null && pc != null)
                pt.DeletePlayer(pc);

            // 죽는 즉시 닉네임 등록 (모든 클라이언트에서 실행됨, 서버 기준으로만 판정해도 되지만 등록은 다 해도 무방)
            GameTurnManager.Instance.RegisterDeadPlayer(myNickname);

            var allPlayers = FindObjectsByType<PlayerGameData>(FindObjectsSortMode.None).ToList();
            var alivePlayers = allPlayers.Where(p => !p.IsDead).ToList();

            if (alivePlayers.Count == 1 && allPlayers.Count > 1 && !_winTriggered)
            {
                _winTriggered = true;

                PlayerControll winner = alivePlayers[0].GetComponent<PlayerControll>();

                // 누적해온 리스트를 그대로 사용
                string deadNamesJoined = string.Join(",", GameTurnManager.Instance.GetDeadPlayerNames());

                GameTurnManager.Instance.SetWinTurn(winner.Object.Id, deadNamesJoined);
            }
        }

        pc.NameText.text = "";
    }
    void Update()
    {
        if (!IsDead && HP <= 0)
        {
            IsDead = true;
        }
    }

    public void Heal(int amount)
    {
        if (!Runner.IsServer) return;
        HP = Mathf.Min(MaxHP, HP + amount);
    }

    void OnHPChanged()
    {
        int index = GetComponent<PlayerObject>().PlayerIndex;

        if (HPUIManager.Instance != null)
            HPUIManager.Instance.RefreshHP(index, HP);
    }

}