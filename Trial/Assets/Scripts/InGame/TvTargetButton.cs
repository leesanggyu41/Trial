using UnityEngine;
using TMPro;
public class TvTargetButton : MonoBehaviour
{
    public Vector2 DirectionKey; // 인스펙터에서 (0,1), (-1,0), (1,0) 세팅
    public TMP_Text NameText;

    public void SetName(string name)
    {
        if (NameText != null) NameText.text = name;
    }
}
