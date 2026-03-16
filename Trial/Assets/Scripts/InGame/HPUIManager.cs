using UnityEngine;
using UnityEngine.UI;

public class HPUIManager : MonoBehaviour, IHPUIHandler
{
    public static HPUIManager Instance;

    [Header("HPBar 슬롯 (PlayerIndex 순서대로)")]
    public HPBarSlot[] slots; // ✅ 씬에서 HPbar_0~3 드래그 연결

    private void Awake() => Instance = this;

    public void RefreshHP(int playerIndex, int hp)
    {
        if (playerIndex >= slots.Length) return;
        slots[playerIndex].RefreshHP(hp);
    }

    public void RefreshTurn(int currentTurnIndex)
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetTurn(i == currentTurnIndex);
    }
}