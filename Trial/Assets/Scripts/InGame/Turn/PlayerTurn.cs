using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class PlayerTurn : NetworkBehaviour
{
    

    [Networked] public int CurrentTurnIndex { get; set; }

    private Dictionary<PlayerControll, int> _playerIndex = new Dictionary<PlayerControll, int>();

    [Networked] public bool IsTurnOn {get; set;}

    [Networked] public bool IsReversed { get; set; } = false;


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
        if (!Runner.IsServer) return;

    
        foreach (var pc in FindObjectsByType<PlayerControll>(FindObjectsSortMode.None))
        {
        pc.InitializeTargetMap();
        }
        IsTurnOn = true;
    }

    // [리모컨 아이템에서 호출할 함수]
    public void ToggleReverse()
    {
        if (!Runner.IsServer) return;

        // 스위치 반전: false -> true / true -> false
        IsReversed = !IsReversed;
        
        Debug.Log($"서버: 리모컨 사용! 현재 역방향 모드: {IsReversed}");
    }

    [ContextMenu("다음 플레이어 턴")]
    public void NextTurn()
    {
        if (!Runner.IsServer) return;

        if(IsTurnOn == false) return;

        int step = IsReversed ? -1 : 1;

        ApplyTurn((CurrentTurnIndex + step + _playerIndex.Count) % _playerIndex.Count);
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
