using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Fusion;

public class SyringeTurn : NetworkBehaviour
{
    public GameObject SyringeBox;
    public GameObject SyringePrefab;

    // 주사기 리스트
    [Networked, Capacity(10)]
    public NetworkLinkedList<NetworkId> So { get; }

    [Networked, Capacity(10)]
    public NetworkLinkedList<SyringeType> St { get; }

    public TextMeshPro Toxin_Text;
    public TextMeshPro NS_Text;

    public void SyringeSpawner_Rpc(int spawnCount)
    {
        int toxin = 0;
        int ns = 0;


        for(int i = 0; i < spawnCount; i++)
        {
            // 아이템 생성
            if(i >= So.Count)
            {
                if(Runner.IsServer)
                {
                    NetworkObject sy = Runner.Spawn(SyringePrefab);
                    So.Add(sy.Id);  // NetworkId로 저장
                    St.Add((SyringeType)(Random.Range(0,10) % 2));
                }
            }
            // 아이템 재활용
            else
            {
                if(Runner.TryFindObject(So.Get(i), out NetworkObject obj))
                {
                    obj.gameObject.SetActive(true);

                    if(Runner.IsServer)
                    St.Set(i, (SyringeType)(Random.Range(0, 10) % 2));
                }
            }

                    // 독소 주사기와 NS 주사기 개수 세기
                    if(St.Get(i) == SyringeType.Toxin) toxin++;
                    if(St.Get(i) == SyringeType.NS) ns++;

        }


        Toxin_Text.text = toxin.ToString();
        NS_Text.text = ns.ToString();
    }

    // NetworkId → GameObject 변환 헬퍼
    public GameObject GetSyringe(int index)
    {
        if(Runner.TryFindObject(So.Get(index), out NetworkObject obj))
            return obj.gameObject;
        return null;
    }
}

public enum SyringeType
{
    Toxin,
    NS
}