using UnityEngine;
using Fusion;

public class PlayerTurnChange : NetworkBehaviour
{
    public Renderer rend;
    public Material turnMat;
    public Material normalMat;

    [Networked]
    public bool IsMyTurn { get; set; }

    public override void Render()
    {
        rend.material = IsMyTurn ? turnMat : normalMat;
    }

    [ContextMenu("색깔 바구기")]
    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void SetTurn(bool value)
    {
        if (Object.HasStateAuthority)
            IsMyTurn = value;
    }
}
