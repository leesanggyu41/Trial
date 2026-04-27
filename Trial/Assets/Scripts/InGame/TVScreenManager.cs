using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TVScreenManager : MonoBehaviour
{

    public Transform pointer;
    public Image[] persion;

    int Savehit = -1;

    // public void Start()
    // {
    //     SetScreenPointerDiraction(Vector2.up);
    // } 



    // public void SetScreenPointerDiraction(Vector2 dir)
    // {
    //     dir = -dir.normalized;
    //     pointer.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90);
    // }

    public void ColorChange(int index)
    {
         foreach(Image p in persion)
        {
                    p.color = Color.white;
                    p.rectTransform.localScale = new Vector3(1, 1, 1);

        }

        
        if (index != -1)
        {
        persion[index].color = Color.red;
        persion[index].rectTransform.localScale = new Vector3(1.2f, 1.2f, 1);
        }
    }
    public void PointRotate(int hitCollider)
    {
        if(Savehit == hitCollider) return;
        switch (hitCollider)
        {
            case 0:
                pointer.rotation = Quaternion.Euler(0, 0, 180);
                ColorChange(2);
                break;
            case 1:
                pointer.rotation = Quaternion.Euler(0, 0, 90);
                ColorChange(1);
                break;
            case 2:
                pointer.rotation = Quaternion.Euler(0, 0, -90);
                ColorChange(0);
                break;
            case 3:
                pointer.rotation = Quaternion.Euler(0, 0, 0);
                ColorChange(3);
                break;
            case 4:
                
                break;
            case 5:
                 pointer.rotation = Quaternion.Euler(0, 0, 135);
                 ColorChange(-1);
                break;
            case 6:
                pointer.rotation = Quaternion.Euler(0, 0, -135);
                ColorChange(-1);
                break;
            case 7:
                 pointer.rotation = Quaternion.Euler(0, 0, 45);
                 ColorChange(-1);
                break;
            case 8:
                 pointer.rotation = Quaternion.Euler(0, 0, -45);
                 ColorChange(-1);
                break;
            default:
            pointer.rotation = Quaternion.Euler(0, 0, 0);
            ColorChange(-1);
                break;

        }

        Savehit = hitCollider;
    }
}
