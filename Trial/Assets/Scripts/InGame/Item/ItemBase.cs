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
    private GameTurnManager _turnManager;

    private ItemMoveAnimation Move
    {
        get
        {
            if (move == null)
                move = GetComponent<ItemMoveAnimation>();
            return move;
        }
    }

    private GameTurnManager TurnManager
    {
        get
        {
            if (_turnManager == null)
                _turnManager = GameTurnManager.Instance;
            if (_turnManager == null)
                _turnManager = FindFirstObjectByType<GameTurnManager>();
            return _turnManager;
        }
    }

    public override void Spawned()
{
    _armController = FindFirstObjectByType<RobotArmFixer>();
    _turnManager = GameTurnManager.Instance;

    // Client는 Spawned 시점에 이미 OwnerRef가 설정되어 있을 수 있음
    if (OwnerRef != PlayerRef.None)
    {
        _prevOwner = OwnerRef;
        SetItemTarget();
        //Debug.LogError($"Spawned에서 SetItemTarget 호출 - HasStateAuthority={Object.HasStateAuthority}");
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
        if (TurnManager == null) { Debug.LogError("TurnManager 없음!"); return; }
        if (TurnManager.NowTurn == GameTurn.Syringe) return;

        TurnManager.RPC_SetTurn(GameTurn.Animation);

        if (Move == null)
        {
            if (_armController != null)
                _armController.GrabAndReturn(transform, Object.Id, () => RPC_Despawn());
            else
                RPC_Despawn();
            return;
        }

        Move.onMoveComplete = () =>
        {
            if (_armController != null)
                _armController.GrabAndReturn(transform, Object.Id, () => RPC_Despawn());
            else
                RPC_Despawn();

            Move.onMoveComplete = null;
        };

        Move.MoveToTarget();
    }
public void BaseOnEvent(System.Action rpcCall)
{

    if (!Object.HasStateAuthority)
        return;
    if (TurnManager == null) { Debug.LogError("TurnManager 없음!"); return; }
    if (TurnManager.NowTurn == GameTurn.Syringe) return;

    TurnManager.RPC_SetTurn(GameTurn.Animation);

    if (Move == null || Move.targetPoint == null)
    {
        rpcCall?.Invoke();
        ArmGrabAndDespawn();
        return;
    }

    Move.onMoveComplete = () =>
    {
        Move.onMoveComplete = null;
        rpcCall?.Invoke();
        ArmGrabAndDespawn();
    };

    Move.MoveToTarget();
}

private void ArmGrabAndDespawn()
{
    if (_armController != null)
        _armController.GrabAndReturn(transform, Object.Id, () => RPC_Despawn());
    else
        RPC_Despawn();
}
   private void SetItemTarget()
{
    //Debug.LogError($"SetItemTarget 호출됨 - playerIndex={OwnerRef.PlayerId - 1}, HasStateAuthority={Object.HasStateAuthority}");

    if (GameSceneManager.Instance == null) { Debug.LogError("GameSceneManager 없음!"); return; }

    int playerIndex = OwnerRef.PlayerId - 1;

    if (playerIndex < 0 || playerIndex >= GameSceneManager.Instance.playerItemPositions.Length)
    {
        Debug.LogError($"playerIndex {playerIndex} 범위 초과!");
        return;
    }

    if (Move == null) { Debug.LogError("Move 없음!"); return; }

    // 모든 클라이언트에서 로컬로 targetPoint 설정
    Move.targetPoint = GameSceneManager.Instance.playerItemPositions[playerIndex];
  //  Debug.LogError($"targetPoint 설정 완료: {Move.targetPoint.name}");
}

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Despawn()
    {
        Debug.Log($"[Despawn] 호출됨: {Object.Id}");
        Runner.Despawn(Object);
    }

   public void OnUse()
{
    if (TurnManager == null) { Debug.LogError("TurnManager 없음!"); return; }
    if (Move == null) { Debug.LogWarning("Move 없음, 이동 스킵"); return; }
    
   // Debug.LogError($"OnUse 호출 - targetPoint={Move.targetPoint}");
    
    if (Move.targetPoint == null) { Debug.LogError("targetPoint 없음!"); return; }

    Move.MoveToTarget();
}
}