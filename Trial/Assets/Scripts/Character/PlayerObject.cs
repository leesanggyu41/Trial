using Fusion;
using UnityEngine;
using TMPro;

public class PlayerObject : NetworkBehaviour
{
    // 네트워크를 통해 동기화될 인덱스 (0, 1, 2, 3...)
    [Networked] public int PlayerIndex { get; set; }
    

    public override void Spawned()
    {
        Debug.Log($"플레이어 생성됨: {PlayerIndex}번 슬롯");
    }
}