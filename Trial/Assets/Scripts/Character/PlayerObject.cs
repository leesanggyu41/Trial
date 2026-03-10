using Fusion;
using UnityEngine;
using TMPro;

public class PlayerObject : NetworkBehaviour
{
    // 네트워크를 통해 동기화될 인덱스 (0, 1, 2, 3...)
    [Networked] public int PlayerIndex { get; set; }
    

    public override void Spawned()
    {
        
        // 시각적으로 구분하기 위해 위치를 인덱스에 따라 정렬
        transform.position = new Vector3(PlayerIndex * 2.0f, 0, 0);
        Debug.Log($"플레이어 생성됨: {PlayerIndex}번 슬롯");
    }
}