// HPUIManager는 게임 내에서 플레이어의 체력 바와 턴 표시를 관리하는 클래스입니다. 
//각 플레이어의 체력 상태를 갱신하고, 현재 턴인 플레이어를 시각적으로 구분하기 위해 HPBarSlot을 사용하여 UI를 업데이트합니다. 
//또한, 싱글톤 패턴을 사용하여 다른 클래스에서 쉽게 접근할 수 있도록 구현되어 있습니다.
using UnityEngine;
using UnityEngine.UI;

public class HPUIManager : MonoBehaviour, IHPUIHandler
{
    public static HPUIManager Instance;

    [Header("HPBar 슬롯 (PlayerIndex 순서대로)")]
    public HPBarSlot[] slots; //씬에서 HPbar_0~3 드래그 연결

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