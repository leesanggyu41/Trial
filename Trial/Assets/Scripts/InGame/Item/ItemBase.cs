using UnityEngine;
using Fusion;

public class ItemBase : NetworkBehaviour
{
    [Networked] public PlayerRef OwnerRef { get; set; }

    public bool CanUse()
    {
        return OwnerRef == Runner.LocalPlayer;
    }

    public void DestroyItem()
    {
        Runner.Despawn(Object);
    }
}
