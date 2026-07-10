using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Fusion;

public class SyringeTurn : NetworkBehaviour
{
    public static SyringeTurn ins;

    public GameObject SyringeBox;
    public GameObject SyringePrefab;

    public GameTurnManager GTM;

    public Animator SyringeBoxAnim;

    [Networked, Capacity(100)]
    public NetworkLinkedList<NetworkId> So { get; }

    [Networked, Capacity(100)]
    public NetworkLinkedList<SyringeType> St { get; }

    //public TextMeshPro[] Toxin_Text;
    //public TextMeshPro[] NS_Text;

    private Screen_MUI Syringe_Screen;

    public void Awake()
    {
        ins = this; // 중복 체크 제거, 그냥 항상 최신으로 설정
    }

    public override void Spawned()
    {
        ins = this; // Spawned에서도 설정 (빌드 타이밍 보장)
        GTM = FindFirstObjectByType<GameTurnManager>();
        Syringe_Screen = SyringeBoxAnim.gameObject.GetComponent<Screen_MUI>();
    }


    public void SpawnSyringeWithGuarantee(int spawnCount)
    {
        if (!Runner.IsServer) return;

        int toxinMin = Mathf.CeilToInt(spawnCount * 0.3f);
        int nsMin = Mathf.CeilToInt(spawnCount * 0.3f);

        List<SyringeType> types = new List<SyringeType>();
        for (int i = 0; i < toxinMin; i++) types.Add(SyringeType.Toxin);
        for (int i = 0; i < nsMin; i++) types.Add(SyringeType.NS);

        int remaining = spawnCount - types.Count;
        for (int i = 0; i < remaining; i++)
            types.Add((SyringeType)(Random.Range(0, 2)));

        // 셔플
        for (int i = 0; i < types.Count; i++)
        {
            int rand = Random.Range(i, types.Count);
            (types[i], types[rand]) = (types[rand], types[i]);
        }

        SyringeSpawner_Rpc(spawnCount, types.ToArray());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void SyringeSpawner_Rpc(int spawnCount, SyringeType[] types)
    {
        int toxin = 0;
        int ns = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            SyringeType randomType = types[i];

            if (i >= So.Count)
            {
                if (Runner.IsServer)
                {
                    NetworkObject sy = Runner.Spawn(SyringePrefab, SyringeBox.transform.position, Quaternion.identity);
                    So.Add(sy.Id);
                    St.Add(randomType);

                    if (sy.TryGetComponent(out SyringeItem syringeScript))
                        syringeScript.MyType = randomType;
                }
            }
            else
            {
                if (Runner.TryFindObject(So.Get(i), out NetworkObject obj))
                {
                    obj.gameObject.SetActive(true);

                    if (Runner.IsServer)
                    {
                        St.Set(i, randomType);
                        if (obj.TryGetComponent(out SyringeItem syringeScript))
                            syringeScript.MyType = randomType;
                    }
                }
            }
        }


        for (int i = 0; i < St.Count; i++)
        {
            if (St.Get(i) == SyringeType.Toxin) toxin++;
            if (St.Get(i) == SyringeType.NS) ns++;
        }

        Syringe_Screen.UpdateText(toxin, ns);
        // for (int i = 0; i < Toxin_Text.Length; i++)
        // {
        //     Syringe_Screen.UpdateText(toxin, ns);
        //     Toxin_Text[i].text = toxin.ToString();
        //     NS_Text[i].text = ns.ToString();
        // }

        if (Runner.IsServer)
            Invoke("UpBox", 3f);
    }

    public GameObject GetSyringe(int index)
    {
        if (Runner.TryFindObject(So.Get(index), out NetworkObject obj))
            return obj.gameObject;
        return null;
    }

    public void UpBox()
    {
        Debug.Log($"[UpBox] 현재 턴: {GTM.NowTurn}");
        if (!Runner.IsServer) return; // 서버만 턴 전환

        RPC_UpBoxAnimation(); // 애니메이션은 모두에게
        Syringe_Screen.NullChick();
        GTM.GamesTurnChange();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpBoxAnimation()
    {
        SyringeBoxAnim.SetTrigger("Up");
        Syringe_Screen.NullChick();
    }

    public void OnSyringeUsed(NetworkId id, SyringeType type)
    {
        if (!Runner.IsServer) return;

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
            So.Remove(id);
            St.Remove(type);
            Debug.Log($"주사기 사용됨. 남은 개수: {So.Count}");
        }
        // if (So.Count == 0)
        // {
        //     GTM.GamesTurnChange();
        // }

        StartCoroutine(CheckSyringeCount());
    }

    private IEnumerator CheckSyringeCount()
    {
        yield return null; // 한 프레임 대기


        Debug.Log($"[체크] 남은 주사기: {So.Count}");
        if (So.Count == 0)
        {
            GTM.GamesTurnChange();
        }
    }

    public void AddSyringes(int count)
    {
        if (!Runner.IsServer) return;
        StartCoroutine(AddSyringesCoroutine(count));
    }

    private IEnumerator AddSyringesCoroutine(int count)
    {
        RPC_PlayBoxAnimation(true);
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < count; i++)
        {

            SyringeType randomType = (SyringeType)(Random.Range(0, 2));
            NetworkObject sy = Runner.Spawn(SyringePrefab, SyringeBox.transform.position, Quaternion.identity);
            So.Add(sy.Id);
            St.Add(randomType);

            if (sy.TryGetComponent(out SyringeItem syringeScript))
                syringeScript.MyType = randomType;
        }

        RPC_PlayBoxAnimation(false);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayBoxAnimation(bool isDown)
    {
        SyringeBoxAnim.SetTrigger(isDown ? "Down" : "Up");
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplySyringeEffect(bool isSelfTarget, NetworkId targetId, SyringeType type, NetworkId syringeId)
    {
        Debug.Log($"[RPC_ApplySyringeEffect] 호출됨 - IsServer: {Runner.IsServer}, targetId: {targetId}, type: {type}");

        if (!Runner.IsServer) return;

        PlayerTurn pt = FindFirstObjectByType<PlayerTurn>();
        Debug.Log($"[RPC_ApplySyringeEffect] pt: {pt}");

        PlayerGameData targetData = null;
        NetworkObject targetObj = null;
        if (Runner.TryFindObject(targetId, out targetObj))
            targetData = targetObj.GetComponent<PlayerGameData>();

        Debug.Log($"[RPC_ApplySyringeEffect] targetObj: {targetObj}, targetData: {targetData}");

        if (targetData == null)
        {
            Debug.LogError("[RPC_ApplySyringeEffect] targetData가 null! 여기서 리턴됨");
            return;
        }

        targetData.RPC_ShowSyringeHit();
        Debug.Log($"[RPC_ApplySyringeEffect] IsAwakening: {targetData.IsAwakening}, type: {type}");

        if (targetData.IsAwakening)
        {
            if (targetData.HP < targetData.MaxHP)
                targetData.HP += 1;
            targetData.IsAwakening = false;
            pt.NextTurn();
        }
        else
        {
            if (type == SyringeType.Toxin)
            {
                targetData.HP -= 1;
                if (targetData.HP <= 0)
                {
                    targetData.IsDead = true;
                    Debug.Log($"서버: {targetData.gameObject.name} 사망!");
                }

                PlayerControll targetPlayer = targetObj.GetComponent<PlayerControll>();
                if (targetPlayer != null)
                    targetPlayer.RPC_PlaySeizureAnimation();

                Debug.Log($"서버: 독 주사기 사용됨. 타겟 체력: {targetData.HP}");
                pt.NextTurn();
            }
            else if (type == SyringeType.NS)
            {
                if (isSelfTarget)
                {
                    targetData.BonusItemCount += 1;
                    Debug.Log($"서버: NS 자가 사격. 보너스 아이템 수: {targetData.BonusItemCount}");
                }
                else
                {
                    Debug.Log("서버: NS 타인 사격. 효과 없이 턴 넘어감.");
                    pt.NextTurn();
                }
            }
        }

        // 주사기는 이미 Despawn 됐으니 OnSyringeUsed로 So/St 리스트 정리만
        OnSyringeUsed(syringeId, type);
    }
}

public enum SyringeType
{
    Toxin,
    NS
}