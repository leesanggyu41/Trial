using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum GameTurn
{
    Player,      // 플레이어 턴 시작
    Syringe,        // 주사기 지급
    Item        // 아이템 지급



}

[DefaultExecutionOrder(-50)]
public class GameTurnManager : NetworkBehaviour

{
    public static GameTurnManager Instance;

    // 애니메이션 ----------------------------------------

    public Animator syringeboxAnim;
    // 게임 턴 관리 부분 -----------------------------------------------------------------------------------------------------
    [Networked] public GameTurn NowTurn { get; set; }
    public SyringeTurn Sy_T;
    public ItemTurn It_T;
    public PlayerTurn Pt_T;


    // 플레이어 턴 관리 부분 -------------------------------------------------------------------------------------------------
    [Networked] public int CurrentTurnIndex { get; set; }

    private Dictionary<PlayerControll, int> _playerIndex = new Dictionary<PlayerControll, int>();


    private void Awake() => Instance = this;

    // 게임 턴 관련 메서드 ---------------------------------------------------------------------------------------------------

    private void Start()
    {
        StartCoroutine(WaitTurnManager());
    }

    public void GameTurns()
    {
        Debug.LogWarning("턴을 골라 볼까요잉!!!");
        if (!Runner.IsServer) return;

        switch (NowTurn)
        {
            case GameTurn.Syringe:
                SyringeTurn_Rpc();
                break;

            case GameTurn.Item:
                ItemTurn_Rpc();
                break;

            case GameTurn.Player:
                PlayerTurn_Rpc();
                break;
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)] // 방장이 실행하고 다른 플레이어에게 결과를 똑같이 전달
    public void SyringeTurn_Rpc()
    {
        syringeboxAnim.SetTrigger("Down");

        // 중요: 오직 서버(방장)만 다음 RPC를 실행할 타이머를 돌립니다.
        if (Object.HasStateAuthority)
        {
            StartCoroutine(WaitAndCallGameTurns(3f));
        }
    }
    private IEnumerator WaitAndCallGameTurns(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // 이제 서버권한이 있는 곳에서만 호출하므로 에러가 나지 않습니다.
        GameTurns_Rpc();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void GameTurns_Rpc()
    {
        Debug.LogWarning("주사기");
        Sy_T.SyringeSpawner_Rpc(10);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void ItemTurn_Rpc()
    {
        Debug.LogWarning("아이템");
        It_T.ItemSpawner_Rpc();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void PlayerTurn_Rpc()
    {
        Debug.LogWarning("플레이어");
        Pt_T.PlayerTurnStart_Rpc();


    }

    IEnumerator WaitTurnManager()
    {
        yield return new WaitForSeconds(3f);
        GamesTurnChange();
    }


    // 주사기 턴 -> 아이템 턴 -> 플레이어 턴 -> 주사기 턴 ...
    IEnumerator NextGameTurn(float waitTime)
    {

        yield return new WaitForSeconds(waitTime);
        int turn = (int)NowTurn;
        NowTurn = (GameTurn)((turn + 1) % (System.Enum.GetNames(typeof(GameTurn)).Length - 1));
    }


    [ContextMenu("게임 턴 전환")]
    public void GamesTurnChange()
    {
        if (!Runner.IsServer) return;

        int turn = (int)NowTurn;
        NowTurn = (GameTurn)((turn + 1) % (System.Enum.GetNames(typeof(GameTurn)).Length));
        GameTurns();

    }
}


