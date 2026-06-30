using UnityEngine;
using Fusion;
public enum TargetType { None, Player, Syringe }
public interface ReactionObject
{
    bool NeedsTargeting { get; }
    TargetType DesiredTarget { get; }
    void OnEvent(bool isSelf, NetworkId targetId, PlayerRef usingPlayer = default);
}
