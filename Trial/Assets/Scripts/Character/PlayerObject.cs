//플레이어 오브젝트는 각 플레이어가 게임에 참여할 때 생성되는 네트워크 객체입니다.
//플레이어의 인덱스를 네트워크를 통해 동기화하여 각 플레이어가 자신이 몇 번째 슬롯에 있는지 알 수 있도록 합니다.
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