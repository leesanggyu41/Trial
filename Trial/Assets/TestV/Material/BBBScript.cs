using JetBrains.Annotations;
using UnityEngine;
using Fusion;

public class BBBScript : NetworkBehaviour
{
    public Material mat;
    public Material mat2;

    public int type = 0;

    void Start()
    {
        mat = GetComponentInChildren<SkinnedMeshRenderer>().materials[4];
        mat2 = GetComponentInChildren<SkinnedMeshRenderer>().materials[1];
    }

    public void MatChaing(float v)
    {
        mat.SetFloat("_Speed", v);
    }

    public void SSS(int v)
    {
        switch (v)
        {
            case 0:
                 mat2.SetFloat("_OFF", 1);      // 전원 활성화  
                 mat2.SetFloat("_waiting", 0);   // 비기다리기 활성화
                break;
            case 1 :
                mat2.SetFloat("_OFF", 0);      // 전원 활성화
                mat2.SetFloat("_waiting", 1);   // 기다리기 활성화
            break;
            case 2:
                 mat2.SetFloat("_waiting", 0);   // 비기다리기 활성화
                break;
        }
        
    }

    public void UseResult()
    {
        Result(type);
    }

    public void Result(int v)
    {
       mat2.SetFloat("_waiting", 0);   // 비기다리기 활성화
       mat2.SetFloat("_Save", v);      // 안전한 주사
    }


    
}
