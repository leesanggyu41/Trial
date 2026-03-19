using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PlayerTurnChange : NetworkBehaviour
{
    public Renderer rend;
    public Material turnMat;
    public Material normalMat;
    public bool IsMyTurn { get; set; }


    
    public void SetTurn(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (Object.HasInputAuthority)
        {
            RPC_SetTurn();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetTurn()
    {
        IsMyTurn = !IsMyTurn;
        rend.material = IsMyTurn ? turnMat : normalMat;
    }
}
