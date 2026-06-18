using UnityEngine;
using Fusion;

public class Converter : ItemBase, ReactionObject
{
    public bool NeedsTargeting => true;
    public TargetType DesiredTarget => TargetType.Syringe;

    [SerializeField] private TTTTimeTTT timeGauge;
    private NetworkId savedTargetId;

    private void Awake()
    {
        if (timeGauge == null)
        {
            timeGauge = GetComponent<TTTTimeTTT>();
        }
    }

    // 1. 아이템 사용 트리거 (로컬에서 실행)
    public void OnEvent(bool isSelfTarget, NetworkId targetId)
    {
        OnUse();

        // 곧바로 서버에게 충전을 시작하라고 요청합니다.
        RPC_StartCharging(targetId);
    }

    // 2. 모든 클라이언트(All)에게 충전을 시작하라고 명령하는 RPC
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_StartCharging(NetworkId targetId)
    {
        savedTargetId = targetId;
        Debug.Log($"[Converter] RPC_StartCharging 실행됨, timeGauge: {timeGauge}");

        if (timeGauge != null)
        {
            Debug.Log($"[Converter] 충전 시작, current: {timeGauge.current}");
            timeGauge.OnChargeComplete -= HandleChargeComplete;
            timeGauge.OnChargeComplete += HandleChargeComplete;
            timeGauge.current = 4.93f;
            timeGauge.ResetGauge();
            timeGauge.isCharging = true;
        }
        else
        {
            Debug.LogError("[Converter] timeGauge가 null!");
            if (Runner.IsServer) HandleChargeComplete();
        }
    }

    private void ApplySyringeChange()
    {
        if (Runner.TryFindObject(savedTargetId, out var targetObj))
        {
            var syringeScript = targetObj.GetComponent<SyringeItem>();
            if (syringeScript == null) return;
            syringeScript.MyType = syringeScript.MyType == SyringeType.Toxin ? SyringeType.NS : SyringeType.Toxin;
        }
    }// Converter - 서버가 완료 판정
    private void HandleChargeComplete()
    {
        if (timeGauge != null)
            timeGauge.OnChargeComplete -= HandleChargeComplete;

        if (!Runner.IsServer) return;

        // 모든 클라이언트에 완료 알림
        RPC_OnChargeComplete();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnChargeComplete()
    {
        // 사운드 재생 (예정)
        // GetComponent<AudioSource>()?.Play();

        if (Runner.IsServer)
        {
            ApplySyringeChange();
            GrabAndDespawn();
        }
    }
}