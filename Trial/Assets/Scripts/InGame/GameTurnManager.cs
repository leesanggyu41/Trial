using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameTurn { Player, Syringe, Item, Animation, Win }

[DefaultExecutionOrder(-50)]
public class GameTurnManager : NetworkBehaviour
{
    public static GameTurnManager Instance;

    [Header("Components & UI")]
    public Animator syringeboxAnim;
    public SyringeTurn Sy_T;
    public ItemTurn It_T;
    public PlayerTurn Pt_T;

    // 💡 핵심: 턴이 바뀌면 모든 클라이언트가 자동으로 OnTurnChanged를 실행합니다. (RPC 대폭 제거 가능)
    [Networked, OnChangedRender(nameof(OnTurnChanged))] 
    public GameTurn NowTurn { get; set; }
    [Networked] public int CurrentTurnIndex { get; set; }

    private Dictionary<PlayerControll, int> _playerIndex = new Dictionary<PlayerControll, int>();
    private Coroutine _turnTimerCoroutine;

    private void Awake() => Instance = this;

    public override void Spawned()
{
    if (HasStateAuthority) 
    {
        StartCoroutine(WaitAndChangeTurn(3f));
    }
}

    /// <summary>
    /// [핵심 최적화] NowTurn이 바뀔 때마다 모든 피어(서버+클라)에서 자동 호출되는 네트워크 콜백
    /// </summary>
    private void OnTurnChanged()
    {
        Debug.LogWarning($"[턴 변경] 현재 턴은? -> {NowTurn}");

        switch (NowTurn)
        {
            case GameTurn.Syringe:
                if (syringeboxAnim != null) syringeboxAnim.SetTrigger("Down");
                // 3초 뒤 주사기 스폰하는 타이머 가동 (서버 전용)
                if (Runner.IsServer) ResetAndStartCoroutine(WaitAndSpawnSyringe(3f));
                break;

            case GameTurn.Item:
                if (Runner.IsServer && It_T != null) It_T.ItemSpawner_Rpc();
                break;

            case GameTurn.Player:
                if (Runner.IsServer && Pt_T != null) Pt_T.PlayerTurnStart_Rpc();
                break;
        }
    }

    private IEnumerator WaitAndSpawnSyringe(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (Runner.IsServer && Sy_T != null)
        {
            Sy_T.SyringeSpawner_Rpc(Random.Range(5, 10));
        }
    }

    private IEnumerator WaitAndChangeTurn(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        GamesTurnChange();
    }

    [ContextMenu("게임 턴 전환")]
    public void GamesTurnChange()
    {
        if (!Runner.IsServer) return;
        // Player(0) -> Syringe(1) -> Item(2) 순환
        NowTurn = (GameTurn)(((int)NowTurn + 1) % 3); 
    }

    #region 🌐 RPC 영역 (꼭 필요한 RPC만 남김)

    // 외부(ItemBase 등)에서 누구나 안전하게 서버의 턴을 바꿀 수 있는 통합 RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetTurn(GameTurn nextTurn)
    {
        if (Object.HasStateAuthority) NowTurn = nextTurn;
    }

    public void SetWinTurn(NetworkId winnerId)
    {
        if (!Runner.IsServer) return;
        NowTurn = GameTurn.Win;
        RPC_WinTurn(winnerId);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_WinTurn(NetworkId winnerId)
    {
        StopAllCoroutines();
        if (Runner.TryFindObject(winnerId, out var winnerObj) && winnerObj.TryGetComponent<PlayerControll>(out var winner))
        {
            WinUIManager.Instance?.ShowWinUI(winner.NameText.text);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_QuitAll() => StartCoroutine(QuitCoroutine());

    private IEnumerator QuitCoroutine()
    {
        yield return Runner.Shutdown(destroyGameObject: false);
        SceneManager.LoadScene("LobbyScene");
    }

    #endregion

    private void ResetAndStartCoroutine(IEnumerator routine)
    {
        if (_turnTimerCoroutine != null) StopCoroutine(_turnTimerCoroutine);
        _turnTimerCoroutine = StartCoroutine(routine);
    }
}