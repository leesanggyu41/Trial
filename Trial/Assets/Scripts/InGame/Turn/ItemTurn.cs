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
                int randIndex = UnityEngine.Random.Range(0, itemPrefeb.Length);
                NetworkObject ob = Runner.Spawn(itemPrefeb[randIndex], PI[i].spawnPoint.position, Quaternion.identity);
                ItemBase ibase = ob.GetComponent<ItemBase>();

                if(ibase != null)
                {
                    ibase.OwnerRef = pc.Object.InputAuthority; // 아이템의 소유자 설정
                }

                if (pc != null)
                {
                    // 핵심: 현재 소지 개수(useitemNum)를 '슬롯 번호'로 사용합니다.
                    int targetIndex = PI[i].useitemNum;

                    // ReceiveItem을 호출할 때 이 번호를 같이 보냅니다.
                    pc.ReceiveItem(ob.gameObject, targetIndex);

                    PI[i].useitemNum++; // 그 다음 아이템은 다음 슬롯으로 가도록 증가
                }
            }

            data.BonusItemCount = 0; // 보너스 초기화
        }

        GTM.GamesTurnChange(); // 아이템 지급 완료 후 턴 전환
    }
}
