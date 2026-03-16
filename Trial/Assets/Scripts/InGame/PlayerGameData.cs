// PlayerGameData.cs
using Fusion;
using UnityEngine;

public class PlayerGameData : NetworkBehaviour, IDamageable
{
    public int MaxHP => 4;

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
            Debug.Log($"{gameObject.name} 탈락!");
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