using UnityEngine;
using Fusion;

public class SyringeItem : NetworkBehaviour, ReactionObject
{

    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Player;

    // 네트워크를 통해 동기화되는 주사기 타입
    [Networked] public SyringeType MyType { get; set; }

    // 인터페이스 구현: 클릭 시 실행
    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {

        RPC_UseSyringe(isSelfTarget, targetId);

    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseSyringe(bool isSelfTarget, NetworkId targetId)
    {
        // 호스트(서버)에서만 실행됨
        if (!Runner.IsServer) return;

        PlayerTurn pt = FindFirstObjectByType<PlayerTurn>();
        SyringeTurn st = FindFirstObjectByType<SyringeTurn>();

        PlayerGameData targetData = null;
        if (Runner.TryFindObject(targetId, out NetworkObject targetObj))
            targetData = targetObj.GetComponent<PlayerGameData>();

        if (targetData == null) return;
        else
        {
            if (targetData.IsAwakening)
            {
                if(targetData.HP < targetData.MaxHP)
                targetData.HP += 1; // 각성 상태에서 주사기 사용 시 체력 증가
                targetData.IsAwakening = false; // 각성 상태 해제
                pt.NextTurn(); // 다음 사람으로 턴 넘김
            }
            else
            {
                // 1. 주사기 종류 및 타겟에 따른 턴 처리 로직
                if (MyType == SyringeType.Toxin)
                {
                    if (targetData != null)
                    {
                        targetData.HP -= 1; // 체력 감소
                        Debug.Log($"서버: 독 주사기 사용됨. 타겟 체력: {targetData.HP}");
                    }
                    else
                    {
                        Debug.Log("서버: 독 주사기 사용됨. 타겟이 없음.");
                    }

                    Debug.Log("서버: 독 주사기 사용됨. 무조건 다음 턴으로.");
                    pt.NextTurn(); // 다음 사람으로 턴 넘김
                }
                else if (MyType == SyringeType.NS)
                {
                    if (isSelfTarget)
                    {
                        if (targetData != null)
                        {
                            targetData.BonusItemCount += 1; // NS 자가 사격 보너스 증가
                            Debug.Log($"서버: NS 자가 사격. 보너스 아이템 수: {targetData.BonusItemCount}");
                        }
                        else
                        {
                            Debug.Log("서버: NS 자가 사격. 타겟이 없음.");
                        }
                        Debug.Log("서버: NS 자가 사격. 턴 유지!");
                        // pt.NextTurn()을 호출하지 않음으로써 현재 플레이어가 계속 행동하게 함
                    }
                    else
                    {
                        Debug.Log("서버: NS 타인 사격. 효과 없이 턴 넘어감.");
                        pt.NextTurn(); // 효과는 없지만 기회는 날아갔으므로 다음 턴
                    }
                }
            }
        }
        // 2. 주사기 수량 체크 (SyringeTurn에 로직이 있다면 호출)
        if (st != null) st.OnSyringeUsed(Object.Id, MyType);

        // 3. 주사기 오브젝트 삭제
        if (Object != null)
        {
            Runner.Despawn(Object);
        }
    }
}