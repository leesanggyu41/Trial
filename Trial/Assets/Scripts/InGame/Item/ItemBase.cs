using UnityEngine;
using Fusion;

public class ItemBase : NetworkBehaviour
{
    [Networked] public PlayerRef OwnerRef { get; set; }

    public string ItemName;
    public string Explanation;

    private RobotArmFixer _armController;

    public override void Spawned()
    {
        _armController = FindFirstObjectByType<RobotArmFixer>();
    }

    public bool CanUse()
    {
        return OwnerRef == Runner.LocalPlayer;
    }

    public void GrabAndDespawn()
    {
        NetworkId id = Object.Id;
        if (GameTurnManager.Instance.NowTurn == GameTurn.Syringe) return;

        GameTurnManager.Instance.RPC_SetTurn(GameTurn.Animation);
        if (_armController != null)
            _armController.GrabAndReturn(transform, Object.Id, () => RPC_Despawn());
        else
            RPC_Despawn();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Despawn()
    {
        Debug.Log($"[Despawn] 호출됨: {Object.Id}");
        //GameTurnManager.Instance.RPC_SetTurn(GameTurn.Player);
        Runner.Despawn(Object);
    }

    public void OnUse()
    {

        GameTurnManager.Instance.RPC_SetTurn(GameTurn.Animation);
    }
}