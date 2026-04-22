using UnityEngine;
using Fusion;

public class Mammer : NetworkBehaviour, ReactionObject
{
    public bool NeedsTargeting => true;

    public TargetType DesiredTarget => TargetType.Syringe;

    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        RPC_UseMammer(targetId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UseMammer(NetworkId targetId)
    {
        if (Runner.TryFindObject(targetId, out var targetObj))
        {
            // 주사기의 타입(독/해독제) 정보를 가져옵니다.
            // SyringeItem 스크립트에 Type 변수가 있다고 가정합니다.
            var syringeScript = targetObj.GetComponent<SyringeItem>();
            if (syringeScript == null) return;

            SyringeType type = syringeScript.MyType;

            // 2. SyringeTurn 스크립트의 OnSyringeUsed를 호출하여 리스트에서 제거합니다.
            // ST는 SyringeTurn 인스턴스를 가리킨다고 가정합니다.
            SyringeTurn.ins.OnSyringeUsed(targetId, type);

            // 3. 모든 클라이언트에게 망치 연출(로봇팔 동작, 이펙트 등)을 재생하도록 명령합니다.
            //RPC_PlayHammerVisual(targetObj.transform.position);

            // 4. 서버에서 주사기 오브젝트를 물리적으로 파괴(Despawn)합니다.이 코드는 데모이며 나중에 파괴되는 애니메이션과 함께 구현할 것
            Runner.Despawn(targetObj);
        }
    }
}
