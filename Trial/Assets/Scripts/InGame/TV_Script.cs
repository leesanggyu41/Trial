using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class TV_Script : MonoBehaviour
{
    public Renderer tvScreen;
    public Collider[] TVScreenPoint;

    public TVScreenManager tv_S;

    public void Start()
    {
        tvScreen = GetComponentInChildren<Renderer>();
        tv_S = FindAnyObjectByType<TVScreenManager>();
        TVScreenOn(false);
    }


    public void TVScreenOn(bool isOn)
    {
        tv_S.PointRotate(-1);
        float v = isOn == false ? 1f : 0f;
        if(tvScreen == null) return;
         foreach (var mat in tvScreen.materials)
    {
        mat.SetFloat("_TVScreen", v);
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
                //print(TVScreenPoint[i].name + "을(를) 클릭!");
                return i; // 찾았다면 그 번호를 돌려줌
            }
        }
        return -1; // 못 찾았다면 -1을 돌려줌
    }

    public void PointRotate(Collider hitCollider)
    {
       tv_S.PointRotate(GetClickedIndex(hitCollider));
    }
}
