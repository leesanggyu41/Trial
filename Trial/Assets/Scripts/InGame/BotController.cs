// BotController는 봇 대전에서 AI 캐릭터의 턴 행동을 결정하는 클래스입니다. (서버/호스트에서만 동작)
// 사람이 클릭할 때와 같은 경로(ReactionObject.OnEvent)로 아이템과 주사기를 사용하므로,
// 판정·연출·턴 진행은 기존 시스템을 그대로 따릅니다.
//
// 판단 규칙
//  1) 쓸 만한 아이템이 있으면 먼저 사용 (각성제, 전기충격기, 파쇄기, 스캐너, 변환기)
//  2) 주사기: 스캔으로 아는 주사기가 있으면 그 결과대로, 모르면 남은 독/NS 비율로 결정
//     - 독 확률이 높으면 상대에게, 낮으면 자신에게 (NS 자가 사격 → 보너스 아이템 + 턴 유지)
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("생각하는 시간 (초)")]
    public float thinkMin = 1.2f;
    public float thinkMax = 2.8f;

    [Header("난이도")]
    [Range(0f, 1f)] public float mistakeChance = 0.15f; // 일부러 확률과 반대로 고를 확률
    [Range(0f, 1f)] public float itemUseChance = 0.8f;  // 쓸 수 있는 아이템이 있을 때 실제로 쓸 확률

    private PlayerControll _pc;
    private PlayerGameData _data;
    private NetworkObject _obj;
    private readonly HashSet<NetworkId> _triedItems = new HashSet<NetworkId>(); // 이번 턴에 이미 시도한 아이템

    private GameTurnManager Gtm => GameTurnManager.Instance;
    private PlayerTurn Pt => Gtm != null ? Gtm.Pt_T : null;
    private SyringeTurn St => SyringeTurn.ins;

    private void Start()
    {
        _pc = GetComponent<PlayerControll>();
        _data = GetComponent<PlayerGameData>();
        _obj = GetComponent<NetworkObject>();
        StartCoroutine(BrainLoop());
    }

    private bool IsServer => _obj != null && _obj.IsValid && _obj.Runner != null && _obj.Runner.IsServer;

    // 지금 내가 행동할 차례인지
    private bool IsMyTurnActive()
    {
        if (!IsServer || _pc == null || _data == null || Gtm == null || Pt == null) return false;
        if (_data.IsDead) return false;
        return _pc.playerTurn && Pt.IsTurnOn && Gtm.NowTurn == GameTurn.Player;
    }

    private IEnumerator BrainLoop()
    {
        bool wasMyTurn = false;

        while (true)
        {
            // 내 차례가 될 때까지 대기
            while (!IsMyTurnActive())
            {
                if (wasMyTurn && (_pc == null || !_pc.playerTurn))
                {
                    _triedItems.Clear(); // 턴이 끝나면 기록 초기화
                    LookAt(null);
                    wasMyTurn = false;
                }
                if (Gtm != null && Gtm.NowTurn == GameTurn.Win) yield break;
                yield return null;
            }
            wasMyTurn = true;

            // 사람처럼 잠깐 고민
            yield return new WaitForSeconds(Random.Range(thinkMin, thinkMax));
            if (!IsMyTurnActive()) continue;

            bool acted = TryUseItem() || UseSyringe();
            if (!acted)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // 행동이 시작되어 Player 턴을 벗어날 때까지 잠깐 대기 (연출 시작 확인)
            float t = 0f;
            while (Gtm.NowTurn == GameTurn.Player && t < 3f) { t += Time.deltaTime; yield return null; }
            // 연출이 끝나 다시 Player 턴이 될 때까지 대기 → 루프 처음에서 내 차례면 다시 행동
            while (Gtm.NowTurn != GameTurn.Player && Gtm.NowTurn != GameTurn.Win) yield return null;
        }
    }

    #region [아이템]
    private bool TryUseItem()
    {
        if (Random.value > itemUseChance) return false;

        PlayerRef myKey = _pc.OwnerKey;
        // 일부 아이템은 한 오브젝트에 기본 ItemBase와 실제 아이템 스크립트가 같이 붙어 있고,
        // 소유자(OwnerRef)는 기본 ItemBase 쪽에만 들어가므로 같은 오브젝트의 '실제 아이템'으로 바꿔서 판단
        var myItems = FindObjectsByType<ItemBase>(FindObjectsSortMode.None)
            .Where(i => i != null && i.Object != null && i.Object.IsValid && i.OwnerRef == myKey
                        && !_triedItems.Contains(i.Object.Id))
            .Select(ResolveItem)
            .Where(i => i != null && !(i is SyringeItem))
            .Distinct()
            .ToList();
        if (myItems.Count == 0) return false;

        PlayerGameData opponent = GetOpponent();

        foreach (var item in myItems)
        {
            switch (item)
            {
                case Stimulant s:
                    if (_data.HP < _data.MaxHP && !_data.IsAwakening)
                        return Use(item, true, _obj.Id, _obj);
                    break;

                case StunGun g:
                    if (opponent != null && !opponent.IsStunned)
                        return Use(item, false, opponent.Object.Id, opponent.Object);
                    break;

                case Destroyer d:
                    if (opponent != null && OpponentHasItems(opponent))
                        return Use(item, false, opponent.Object.Id, opponent.Object);
                    break;

                case Scanner sc:
                    {
                        var unknown = GetSyringes().Where(sy => !IsKnownByMe(sy)).ToList();
                        if (unknown.Count >= 2 && GetKnownSyringe() == null)
                        {
                            var target = unknown[Random.Range(0, unknown.Count)];
                            return Use(item, false, target.Object.Id, target.Object);
                        }
                    }
                    break;

                case Converter c:
                    {
                        // 아는 NS 주사기를 독으로 바꿔 상대에게 쓰기 위한 준비
                        var knownNs = GetSyringes().FirstOrDefault(sy => IsKnownByMe(sy) && sy.MyType == SyringeType.NS);
                        if (knownNs != null && opponent != null && _data.BonusItemCount == 0)
                            return Use(item, false, knownNs.Object.Id, knownNs.Object);
                    }
                    break;
            }
        }
        return false;
    }

    // 같은 오브젝트에 붙은 컴포넌트 중 기본 ItemBase가 아닌 실제 아이템 스크립트를 반환
    private static ItemBase ResolveItem(ItemBase item)
    {
        foreach (var c in item.GetComponents<ItemBase>())
            if (c.GetType() != typeof(ItemBase)) return c;
        return item;
    }

    private bool OpponentHasItems(PlayerGameData opponent)
    {
        var pc = opponent.GetComponent<PlayerControll>();
        if (pc == null) return false;
        PlayerRef key = pc.OwnerKey;
        return FindObjectsByType<ItemBase>(FindObjectsSortMode.None)
            .Any(i => i != null && i.Object != null && i.Object.IsValid && i.OwnerRef == key && !(i is SyringeItem));
    }
    #endregion

    #region [주사기]
    private bool UseSyringe()
    {
        var syringes = GetSyringes();
        if (syringes.Count == 0) return false;

        PlayerGameData opponent = GetOpponent();
        if (opponent == null) return false;

        SyringeItem chosen;
        bool shootSelf;

        SyringeItem known = GetKnownSyringe();
        if (known != null)
        {
            // 스캔으로 아는 주사기: 독이면 상대, NS면 자신
            chosen = known;
            shootSelf = known.MyType == SyringeType.NS;
        }
        else
        {
            // 공개된 남은 개수로 독 확률 계산
            CountRemaining(out int toxin, out int ns);
            int total = Mathf.Max(1, toxin + ns);
            float toxinChance = (float)toxin / total;

            shootSelf = toxinChance < 0.5f;

            // 체력이 1이면 자신에게 쏘는 모험은 피함
            if (_data.HP <= 1 && toxinChance > 0f) shootSelf = false;

            // 가끔 실수 (너무 완벽하면 재미없음)
            if (Random.value < mistakeChance) shootSelf = !shootSelf;

            chosen = syringes[Random.Range(0, syringes.Count)];
        }

        if (shootSelf) return Use(chosen, true, _obj.Id, _obj);
        return Use(chosen, false, opponent.Object.Id, opponent.Object);
    }

    private List<SyringeItem> GetSyringes()
    {
        var list = new List<SyringeItem>();
        if (St == null || _obj.Runner == null) return list;

        for (int i = 0; i < St.So.Count; i++)
        {
            if (_obj.Runner.TryFindObject(St.So.Get(i), out NetworkObject o) && o.gameObject.activeInHierarchy)
            {
                var sy = o.GetComponent<SyringeItem>();
                if (sy != null) list.Add(sy);
            }
        }
        return list;
    }

    private bool IsKnownByMe(SyringeItem sy) => sy.ScannedByPlayer == _pc.OwnerKey;

    private SyringeItem GetKnownSyringe() => GetSyringes().FirstOrDefault(IsKnownByMe);

    private void CountRemaining(out int toxin, out int ns)
    {
        toxin = 0; ns = 0;
        if (St == null) return;
        for (int i = 0; i < St.St.Count; i++)
        {
            if (St.St.Get(i) == SyringeType.Toxin) toxin++;
            else ns++;
        }
    }
    #endregion

    #region [공통]
    // 살아 있는 상대 중 체력이 가장 낮은 플레이어
    private PlayerGameData GetOpponent()
    {
        return FindObjectsByType<PlayerGameData>(FindObjectsSortMode.None)
            .Where(p => p != _data && p.Object != null && p.Object.IsValid && !p.IsDead)
            .OrderBy(p => p.HP)
            .FirstOrDefault();
    }

    // 사람의 RPC_ConfirmUse와 같은 순서로 서버에서 직접 실행
    private bool Use(ItemBase item, bool isSelf, NetworkId targetId, NetworkObject lookTarget)
    {
        if (item == null || item.Object == null || !item.Object.IsValid) return false;
        var reaction = item.GetComponent<ReactionObject>();
        if (reaction == null) return false;

        _triedItems.Add(item.Object.Id);
        LookAt(isSelf ? null : lookTarget);

        Debug.Log($"[Bot] {item.GetType().Name} 사용 → {(isSelf ? "자신" : lookTarget != null ? lookTarget.name : "?")}");

        if (Gtm.NowTurn != GameTurn.Win)
            Gtm.RPC_SetTurn(GameTurn.Animation);

        reaction.OnEvent(isSelf, targetId, _pc.OwnerKey);
        return true;
    }

    // 고개를 대상 쪽으로 돌림 (NetworkedCameraX/Y는 다른 화면에서 목 회전에 쓰임)
    private void LookAt(NetworkObject target)
    {
        if (_pc == null || !IsServer) return;

        if (target == null)
        {
            _pc.NetworkedCameraX = 0f;
            _pc.NetworkedCameraY = 0f;
            return;
        }

        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0f;
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        _pc.NetworkedCameraX = Mathf.Clamp(angle, -_pc.Xlimit, _pc.Xlimit);
        _pc.NetworkedCameraY = 10f;
    }
    #endregion
}
