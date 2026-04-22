using UnityEngine;

public class TV_Script : MonoBehaviour
{
    
    public Renderer tvScreen;
    public Collider[] TVScreenPoint;

    public void Start()
    {
        tvScreen = GetComponentInChildren<Renderer>();
        TVScreenOn(false);
    }


    public void TVScreenOn(bool isOn)
    {
        float v = isOn == false ? 1f : 0f;
        if(tvScreen == null) return;
         foreach (var mat in tvScreen.materials)
    {
        mat.SetFloat("_TVScreen", v);
        print(mat.name);
    }
    }

    public int GetClickedIndex(Collider hitCollider)
    {
        if (TVScreenPoint == null) return -1;

        for (int i = 0; i < TVScreenPoint.Length; i++)
        {
            // 내가 맞춘 콜라이더가 배열의 몇 번째(i)와 같은지 비교합니다.
            if (TVScreenPoint[i] == hitCollider) 
            {
                return i; // 찾았다면 그 번호를 돌려줌
            }
        }
        return -1; // 못 찾았다면 -1을 돌려줌
    }
}
