// HPBarSlot은 플레이어의 체력 바를 관리하는 클래스입니다.
// 각 플레이어의 체력 상태를 시각적으로 표시하기 위해 앞면과 뒷면의 이미지 배열을 사용하여 체력 바를 갱신합니다. 
//또한, 플레이어의 턴이 시작될 때 턴 바를 활성화하여 현재 턴인 플레이어를 구분할 수 있도록 합니다.
using UnityEngine;
using UnityEngine.UI;

public class HPBarSlot : MonoBehaviour
{
    public GameObject turnBar;
    public Image[] frontHpImages; // 앞면 Canvas 이미지 4개
    public Image[] backHpImages;  // 뒷면 Canvas 이미지 4개

    public void RefreshHP(int hp)
    {
        // 앞면 뒷면 둘 다 갱신
        for (int i = 0; i < frontHpImages.Length; i++)
        {
            if (frontHpImages[i] != null)
                frontHpImages[i].gameObject.SetActive(i < hp);
        }
        for (int i = 0; i < backHpImages.Length; i++)
        {
            if (backHpImages[i] != null)
                backHpImages[i].gameObject.SetActive(i < hp);
        }
    }

    public void SetTurn(bool isMyTurn)
    {
        if (turnBar != null)
            turnBar.SetActive(isMyTurn);
    }
}