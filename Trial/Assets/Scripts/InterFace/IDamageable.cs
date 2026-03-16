using UnityEngine;

public interface IDamageable
{
    void TakeDamage (int damage);
    void Heal (int amount);
    int HP{get;}
}
