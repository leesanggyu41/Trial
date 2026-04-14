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
    public int DefaultItemNum = 1;

    public GameTurnManager GTM;

    void Start()
    {
        GTM = FindFirstObjectByType<GameTurnManager>();
    }

    public void ItemSpawner_Rpc()
{
    if (!Runner.IsServer) return;

    // 모든 플레이어 데이터를 가져와서 순서대로 정렬 (기존 코드 활용)
    var players = FindObjectsByType<PlayerGameData>(FindObjectsSortMode.None)
                    .OrderBy(p => p.GetComponent<PlayerObject>().PlayerIndex)
                    .ToList();

    for (int i = 0; i < players.Count; i++)
    {
        PlayerGameData data = players[i];
        PlayerControll pc = data.GetComponent<PlayerControll>();

        // 1. 현재 소지량 확인 및 생성 개수 제한 (최대 6개)
        int currentCount = PI[i].useitemNum; 
        int totalToGive = PI[i].sapwnitemNum + data.BonusItemCount;
        int finalSpawnCount = Mathf.Min(totalToGive, 6 - currentCount);

        for (int pp = 0; pp < finalSpawnCount; pp++)
        {
            // 2. 아이템 랜덤 생성
            int randIndex = UnityEngine.Random.Range(0, itemPrefeb.Length);
            NetworkObject ob = Runner.Spawn(itemPrefeb[randIndex], PI[i].spawnPoint.position, Quaternion.identity);

            // 3. 플레이어 컨트롤러에게 아이템 전달 (이동 시작)
            if (pc != null)
            {
                pc.ReceiveItem(ob.gameObject);
                PI[i].useitemNum++; // 데이터 갱신
            }
        }
        
        data.BonusItemCount = 0; // 보너스 초기화
    }

    GTM.GamesTurnChange(); // 아이템 지급 완료 후 턴 전환
}
}
