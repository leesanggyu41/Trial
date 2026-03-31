using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class GameTurnManager : NetworkBehaviour
{
    public static GameTurnManager Instance;

    [Networked] public int CurrentTurnIndex { get; set; }

    private Dictionary<PlayerController, int> _playerIndex = new Dictionary<PlayerController, int>();

    private void Awake() => Instance = this;

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

    [ContextMenu("다음 턴")]
    public void NextTurn()
    {
        if (!Runner.IsServer) return;
        ApplyTurn((CurrentTurnIndex + 1) % _playerIndex.Count);
    }

    private void ApplyTurn(int index)
    {
        CurrentTurnIndex = index;
        foreach (var (player, playerIndex) in _playerIndex)
            player.playerTurn = (playerIndex == index);
    }
}