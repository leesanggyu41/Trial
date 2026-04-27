// PlayerGameData는 플레이어의 체력과 관련된 게임 데이터를 관리하는 클래스입니다.
// 플레이어의 체력을 네트워크로 동기화하여 모든 클라이언트에서 일관된 상태를 유지하며,
// 체력 변화에 따른 UI 업데이트를 처리합니다. 또한, IDamageable 인터페이스를 구현하여 다른 오브젝트로부터 피해를 받을 수 있도록 합니다.
using Fusion;
using UnityEngine;

public class PlayerGameData : NetworkBehaviour, IDamageable
{
    public int MaxHP => 4;

    [Networked] public int BonusItemCount { get; set; } = 0; // NS 자가 사격 보너스

    [Networked] public bool IsAwakening { get; set; } = false; // 각성 상태

    [Networked] public bool IsStunned { get; set; } = false; // 스턴 상태

    [Networked] public bool IsDead { get; set; } = false; // 사망 상태

    [Networked, OnChangedRender(nameof(OnHPChanged))]
    public int HP { get; set; } 

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