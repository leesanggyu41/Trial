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