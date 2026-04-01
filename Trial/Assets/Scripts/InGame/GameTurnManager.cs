using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameTurn
{
    Syringe,        // 주사기 지급
    Item,           // 아이템 지급

    Player        // 플레이어 턴 시작


}

[DefaultExecutionOrder(-50)]
public class GameTurnManager : NetworkBehaviour
{
    public static GameTurnManager Instance;
    // 게임 턴 관리 부분 -----------------------------------------------------------------------------------------------------
    [Networked] public GameTurn NowTurn {get; set;}





    // 플레이어 턴 관리 부분 -------------------------------------------------------------------------------------------------
    [Networked] public int CurrentTurnIndex { get; set; }

    private Dictionary<PlayerController, int> _playerIndex = new Dictionary<PlayerController, int>();


    private void Awake() => Instance = this;

    // 게임 턴 관련 메서드 ---------------------------------------------------------------------------------------------------



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


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)] // 반장이 실행하고 다른 플레이어에게 결과를 똑같이 전달
    public void SyringeTurn_Rpc()
    {
        Debug.LogWarning("주사기");
    }

     [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void ItemTurn_Rpc()
    {
        Debug.LogWarning("아이템");
    }
     [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void PlayerTurn_Rpc()
    {
        Debug.LogWarning("플레이어");
    }


    // 주사기 턴 -> 아이템 턴 -> 플레이어 턴 -> 주사기 턴 ...
    IEnumerator NextGameTurn(float waitTime)
    {

        yield return new WaitForSeconds(waitTime);
        int turn = (int)NowTurn;
        NowTurn = (GameTurn)((turn + 1) % (System.Enum.GetNames(typeof(GameTurn)).Length -1));
    }


    [ContextMenu("게임 턴 전환")]
    public void GamesTurnChange()
    {
        if (!Runner.IsServer) return;

        int turn = (int)NowTurn;
        NowTurn = (GameTurn)((turn + 1) % (System.Enum.GetNames(typeof(GameTurn)).Length -1));
        GameTurns();

    }





    // 플레이어 턴 관련 메서드 -----------------------------------------------------------------------------------------------
    public void RegisterPlayer(PlayerController player)
    {
        if (!_playerIndex.ContainsKey(player))
        {
            int index = _playerIndex.Count;
            _playerIndex.Add(player, index);
        }
    }

    public override void Spawned()
    {
        if (Runner.IsServer) ApplyTurn(0);
    }

    [ContextMenu("다음 플레이어 턴")]
    public void NextTurn()
    {
        if (!Runner.IsServer) return;
        ApplyTurn((CurrentTurnIndex + 1) % _playerIndex.Count);
    }

    // 턴 지정
    private void ApplyTurn(int index)
    {
        CurrentTurnIndex = index;
        foreach (var (player, playerIndex) in _playerIndex)
            player.playerTurn = (playerIndex == index);
    }

    // 해당 플레이어 턴 삭제
    public void DeletePlayer(PlayerController player)
    {
        if (_playerIndex.ContainsKey(player))
        _playerIndex.Remove(player);
    }
}