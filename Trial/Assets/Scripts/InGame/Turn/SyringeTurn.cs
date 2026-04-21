using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Fusion;

public class SyringeTurn : NetworkBehaviour
{
    public GameObject SyringeBox;
    public GameObject SyringePrefab;

    public GameTurnManager GTM;

    public Animator SyringeBoxAnim;

    // 주사기 리스트
    [Networked, Capacity(10)]
    public NetworkLinkedList<NetworkId> So { get; }

    [Networked, Capacity(10)]
    public NetworkLinkedList<SyringeType> St { get; }

    public TextMeshPro[] Toxin_Text;
    public TextMeshPro[] NS_Text;

    public void Start()
    {
        GTM = FindFirstObjectByType<GameTurnManager>();
        
    }

    public void SyringeSpawner_Rpc(int spawnCount)
    {
        int toxin = 0;
        int ns = 0;

        for(int i = 0; i < spawnCount; i++)
        {
            SyringeType randomType = (SyringeType)(Random.Range(0, 10) % 2);

            // 1. 아이템 신규 생성
            if(i >= So.Count)
            {
                if(Runner.IsServer)
                {
                    NetworkObject sy = Runner.Spawn(SyringePrefab, SyringeBox.transform.position, Quaternion.identity);
                    So.Add(sy.Id);
                    St.Add(randomType); // 리스트에 저장

                    //  핵심: 생성된 주사기 스크립트에 타입 전달
                    if(sy.TryGetComponent(out SyringeItem syringeScript))
                    {
                        syringeScript.MyType = randomType;
                    }
                }
            }
            // 2. 아이템 재활용
            else
            {
                if(Runner.TryFindObject(So.Get(i), out NetworkObject obj))
                {
                    obj.gameObject.SetActive(true);

                    if(Runner.IsServer)
                    {
                        St.Set(i, randomType);
                        
                        //  핵심: 재활용 시에도 새로운 타입을 알려줌
                        if(obj.TryGetComponent(out SyringeItem syringeScript))
                        {
                            syringeScript.MyType = randomType;
                        }
                    }
                }
            }

            // UI 표시용 카운트 (St 리스트 기준)
            if(St.Get(i) == SyringeType.Toxin) toxin++;
            if(St.Get(i) == SyringeType.NS) ns++;
        }
        for(int i = 0; i < Toxin_Text.Length; i++)
        {
            Toxin_Text[i].text = toxin.ToString();
            NS_Text[i].text = ns.ToString();
        }

        Invoke("UpBox", 3f);
    }

    // NetworkId → GameObject 변환 헬퍼
    public GameObject GetSyringe(int index)
    {
        if(Runner.TryFindObject(So.Get(index), out NetworkObject obj))
            return obj.gameObject;
        return null;
    }

    public void UpBox()
    {
        SyringeBoxAnim.SetTrigger("Up");
        GTM.GamesTurnChange();
    }

    public void OnSyringeUsed(NetworkId id, SyringeType type)
{
    if (!Runner.IsServer) return;

    // 1. 리스트에서 해당 주사기 정보 제거
    int index = -1;
    for (int i = 0; i < So.Count; i++)
    {
        if (So.Get(i) == id)
        {
            index = i;
            break;
        }
    }

    if (index != -1)
    {
        bool removedSo = So.Remove(id);
        bool removedSt = St.Remove(type);
        Debug.Log($"주사기 사용됨. 남은 개수: {So.Count}");
    }

    // 3. 주사기가 0개라면 아이템 턴 혹은 다음 라운드로 전환
    if (So.Count == 0)
    {
        Debug.Log("모든 주사기 사용 완료! 다음 단계로 넘어갑니다.");
        // GameTurnManager를 통해 다음 턴(Item 턴 등)을 호출하는 로직을 여기에 넣습니다.
        GTM.GamesTurnChange();
    }
}
}

public enum SyringeType
{
    Toxin,
    NS
}