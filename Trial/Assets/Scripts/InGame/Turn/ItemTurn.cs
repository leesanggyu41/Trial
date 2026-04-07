using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;


[Serializable]
public class PlayerItems
{
    public Transform spawnPoint;    // 아이템을 생성할 지점
    public int sapwnitemNum;        // 생성할 아이템 횟수
    public int useitemNum;          // 현재 보유한 아이템 개수, 일단 최대 8개로 지정
}
public class ItemTurn : NetworkBehaviour
{
    
    public List<GameObject> itemPrefeb;
    public List<PlayerItems> playerItems;

    void Start()
    {
        
    }

    public void ItemSpawner_Rpc()
    {
        
    }
}
