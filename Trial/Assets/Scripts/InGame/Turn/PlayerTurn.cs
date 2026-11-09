using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class PlayerTurn : NetworkBehaviour
{


    [Networked] public int CurrentTurnIndex { get; set; }

    private Dictionary<PlayerControll, int> _playerIndex = new Dictionary<PlayerControll, int>();

    [Networked] public bool IsTurnOn { get; set; }

    [Networked] public bool IsReversed { get; set; } = false;

    [Header("턴 제한시간")]
    [Tooltip("한 플레이어가 행동할 수 있는 시간(초). 지나면 자동으로 다음 플레이어에게 넘어감")]
    public float turnTimeLimit = 30f;

    [Networked] public TickTimer TurnTimer { get; set; }

    private GameTurn _lastSeenTurn = GameTurn.Animation; // 서버 전용: Player 턴으로 돌아온 순간 감지용


    // 지금까지 등록된 자리 수 (플레이어가 삭제돼도 줄어들지 않음 → 인덱스가 겹치거나 비는 문제 방지)
    private int _slotCount = 0;

    public void RegisterPlayer(PlayerControll player)
    {
        if (!_playerIndex.ContainsKey(player))
        {
            int index = _slotCount++;
            _playerIndex.Add(player, index);
        }
    }

    public override void Spawned()
    {
        if (Runner.IsServer) ApplyTurn(0);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void PlayerTurnStart_Rpc()
    {
        Debug.Log($"[RPC] PlayerTurnStart_Rpc 실행됨! Local: {PlayerControll.Local}"); // ← 추가

        if (PlayerControll.Local == null)
        {
            Debug.LogError("[RPC] PlayerControll.Local이 null임!");
            return;
        }

        PlayerControll.Local.InitializeTargetMap();

        if (Runner.IsServer)
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

        if (IsTurnOn == false) return;

        int step = IsReversed ? -1 : 1;
        int slotCount = _slotCount; // 삭제된 플레이어가 있어도 인덱스 범위는 그대로 유지
        if (slotCount <= 0) return;

        int index = CurrentTurnIndex;

        // 한 바퀴(+자기 자신)까지만 탐색 → 무한 재귀 방지
        for (int i = 0; i < slotCount; i++)
        {
            index = ((index + step) % slotCount + slotCount) % slotCount;

            PlayerControll nextPlayer = GetPlayerByIndex(index);

            // 나가거나 삭제된 자리 → 건너뜀
            if (nextPlayer == null)
            {
                Debug.Log($"플레이어 {index} 자리가 비어 있습니다. 다음 플레이어로 넘어갑니다.");
                continue;
            }

            PlayerGameData nextplayerData = nextPlayer.GetComponent<PlayerGameData>();

            if (nextplayerData != null && nextplayerData.IsDead)
            {
                Debug.Log($"플레이어 {index}는 사망 상태입니다. 다음 플레이어로 넘어갑니다.");
                continue;
            }

            if (nextplayerData != null && nextplayerData.IsStunned)
            {
                Debug.Log($"플레이어 {index}는 스턴 상태입니다. 다음 플레이어로 넘어갑니다.");
                CurrentTurnIndex = index; // 턴은 넘어가지만 행동은 불가능
                nextplayerData.IsStunned = false; // 스턴 상태 해제 (감전 이펙트는 OnChangedRender로 모든 클라이언트에서 꺼짐)
                continue;
            }

            ApplyTurn(index);
            return;
        }

        Debug.LogWarning("[NextTurn] 턴을 넘길 수 있는 플레이어가 없습니다.");
    }

    private PlayerControll GetPlayerByIndex(int index)
    {
        foreach (var (player, playerIndex) in _playerIndex)
        {
            if (playerIndex == index) return player;
        }
        return null;
    }

    #region [턴 제한시간]
    // 제한시간 다시 시작 (서버 전용)
    public void RestartTurnTimer()
    {
        if (!Runner.IsServer) return;
        TurnTimer = TickTimer.CreateFromSeconds(Runner, turnTimeLimit);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;

        var gtm = GameTurnManager.Instance;
        if (gtm == null) return;

        GameTurn now = gtm.NowTurn;

        // 아이템 연출(Animation) 등이 끝나고 Player 턴으로 돌아오면 시간을 새로 줌
        if (now == GameTurn.Player && _lastSeenTurn != GameTurn.Player)
            RestartTurnTimer();
        _lastSeenTurn = now;

        if (now != GameTurn.Player || !IsTurnOn) return;

        if (TurnTimer.Expired(Runner))
        {
            Debug.Log($"[턴 제한시간] 플레이어 {CurrentTurnIndex} 시간 초과! 다음 플레이어로 넘어갑니다.");
            TurnTimer = TickTimer.None;
            NextTurn(); // 다음 플레이어가 정해지면 ApplyTurn에서 타이머 재시작
        }
    }

    // 모든 클라이언트: 현재 턴 플레이어의 HP 모니터에 남은 초 표시
    public override void Render()
    {
        if (HPUIManager.Instance == null) return;

        int seconds = -1;
        var gtm = GameTurnManager.Instance;
        if (gtm != null && gtm.NowTurn == GameTurn.Player && IsTurnOn && TurnTimer.IsRunning)
        {
            float? remaining = TurnTimer.RemainingTime(Runner);
            seconds = Mathf.Max(0, Mathf.CeilToInt(remaining ?? 0f));
        }

        HPUIManager.Instance.RefreshTimer(CurrentTurnIndex, seconds);
    }
    #endregion

    // 턴 지정
    private void ApplyTurn(int index)
    {
        CurrentTurnIndex = index;
        RestartTurnTimer();
        foreach (var (player, playerIndex) in _playerIndex)
            player.playerTurn = (playerIndex == index);

        if (HPUIManager.Instance != null)
            RPC_RefreshTurn(index);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RefreshTurn(int index)
    {
        if (HPUIManager.Instance != null)
            HPUIManager.Instance.RefreshTurn(index);
    }

    // 해당 플레이어 턴 삭제
    public void DeletePlayer(PlayerControll player)
    {
        if (_playerIndex.ContainsKey(player))
            _playerIndex.Remove(player);
    }
}
