using UnityEngine;
using UnityEngine.UI;

public class TVScreenManager : MonoBehaviour
{

    public Transform ScreenPointer;
    public Image[] persion;

    public void Start()
    {
        SetScreenPointerDiraction(Vector2.up);
    } 



    public void SetScreenPointerDiraction(Vector2 dir)
    {
        dir = -dir.normalized;
       ScreenPointer.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90);
    }
}
