using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class PlayerTurn : NetworkBehaviour
{
    

    [Networked] public int CurrentTurnIndex { get; set; }

    private Dictionary<PlayerControll, int> _playerIndex = new Dictionary<PlayerControll, int>();

    [Networked] public bool IsTurnOn {get; set;}


    public void RegisterPlayer(PlayerControll player)
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
    public void PlayerTurnStart_Rpc()
    {
        IsTurnOn = true;
        ApplyTurn(0);
    }

    [ContextMenu("다음 플레이어 턴")]
    public void NextTurn()
    {
        if (!Runner.IsServer) return;

        if(IsTurnOn == false) return;

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
    public void DeletePlayer(PlayerControll player)
    {
        if (_playerIndex.ContainsKey(player))
        _playerIndex.Remove(player);
    }
}
