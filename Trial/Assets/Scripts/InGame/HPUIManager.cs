using UnityEngine;

public class HPUIManager : MonoBehaviour, IHPUIHandler
{
    public static HPUIManager Instance;

    [Header("HpBar (PlayerIndex 순서대로)")]
    public HpBar[] hpBars;

    private void Awake() => Instance = this;

    public void RefreshHP(int playerIndex, int hp)
    {
        if (playerIndex >= hpBars.Length) return;
        hpBars[playerIndex].SetHP(hp);
    }

    // 현재 턴 플레이어 모니터에만 남은 시간 표시, 나머지는 숨김 (seconds < 0 이면 전부 숨김)
    public void RefreshTimer(int currentTurnIndex, int seconds)
    {
        for (int i = 0; i < hpBars.Length; i++)
        {
            if (hpBars[i] == null) continue;
            hpBars[i].SetTimer(i == currentTurnIndex ? seconds : -1);
        }
    }

    public void RefreshTurn(int currentTurnIndex)
    {
        for (int i = 0; i < hpBars.Length; i++)
        {
            if (i == currentTurnIndex)
                hpBars[i].IsMyTurn();
            else
                hpBars[i].TurnEnd();
        }
    }
}