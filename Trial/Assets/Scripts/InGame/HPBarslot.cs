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