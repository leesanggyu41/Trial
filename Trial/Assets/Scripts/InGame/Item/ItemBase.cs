using UnityEngine;
using Fusion;

public class ItemBase : NetworkBehaviour
{
    [Networked] public PlayerRef OwnerRef { get; set; }

    public string ItemName;
    public string Explanation;

    private PlayerRef _prevOwner = PlayerRef.None;
    private RobotArmFixer _armController;
    private ItemMoveAnimation move;

    public override void Spawned()
    {
        _armController = FindFirstObjectByType<RobotArmFixer>();
        move = GetComponent<ItemMoveAnimation>();

        // Spawned 시점에 이미 OwnerRef가 설정되어 있을 수 있음
        if (OwnerRef != PlayerRef.None)
        {
            _prevOwner = OwnerRef;
            SetItemTarget();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (OwnerRef != PlayerRef.None && OwnerRef != _prevOwner)
        {
            _prevOwner = OwnerRef;
            SetItemTarget();
        }
    }

    public bool CanUse()
    {
        return OwnerRef == Runner.LocalPlayer;
    }

    public void GrabAndDespawn()
    {
        if (GameTurnManager.Instance.NowTurn == GameTurn.Syringe) return;

        GameTurnManager.Instance.RPC_SetTurn(GameTurn.Animation);
        if (_armController != null)
            _armController.GrabAndReturn(transform, Object.Id, () => RPC_Despawn());
        else
            RPC_Despawn();
    }

    private void SetItemTarget()
    {
        int playerIndex = OwnerRef.PlayerId - 1;

        Debug.LogWarning($"-------------------{playerIndex}----------------");

        if (playerIndex < 0 || playerIndex >= GameSceneManager.Instance.playerItemPositions.Length)
        {
            Debug.LogError($"playerIndex {playerIndex} 범위 초과! 배열 길이: {GameSceneManager.Instance.playerItemPositions.Length}");
            return;
        }

        Debug.LogWarning("SetItemTarget 정상 실행됨");

        move.targetPoint = GameSceneManager.Instance.playerItemPositions[playerIndex];
        move.MoveToTarget();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Despawn()
    {
        Debug.Log($"[Despawn] 호출됨: {Object.Id}");
        Runner.Despawn(Object);
    }

    public void OnUse()
    {
        GameTurnManager.Instance.RPC_SetTurn(GameTurn.Animation);
    }
}