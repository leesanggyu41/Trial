using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Linq;


[Serializable]
public class PlayerItems
{
    [Header("아이템 생성 지점")]
    public Transform spawnPoint;    // 아이템을 생성할 지점
    [Header("아이템 생성 횟수")]
    public int sapwnitemNum;        // 생성할 아이템 횟수

    [Header("현재 소지한 아이템 개수")]
    public int useitemNum;          // 현재 보유한 아이템 개수, 일단 최대 8개로 지정
}
public class ItemTurn : NetworkBehaviour
{
    
    public GameObject[] itemPrefeb;
    public List<PlayerItems> PI;
    public int DefaultItemNum = 2;

    void Start()
    {
        
    }

    public void ItemSpawner_Rpc()
    {
        if(Runner.IsServer != true) return;

        // 현제 플레이어 수만큼 반복
        for(int i = 0; i < Runner.ActivePlayers.Count(); i++)
        {
                
            for(int pp = 0; pp < PI[i].sapwnitemNum; pp++)
            {
                int index = UnityEngine.Random.Range(0, itemPrefeb.Length);
                NetworkObject ob = Runner.Spawn(itemPrefeb[index]);
                
                ob.transform.localPosition = PI[i].spawnPoint.position;
            }

        }

        


    }
}
