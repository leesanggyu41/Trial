using UnityEngine;

public class Syringe : MonoBehaviour,ReactionObject
{
   public enum SyringeType{Normal,Poison}
   public SyringeType type;

   



    public void OnEvent()
    {
        // 주사기 사용
    }
}
