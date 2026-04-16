using UnityEngine;
using Fusion;
public enum TargetType { None, Player, Syringe }
public interface ReactionObject
{
    bool NeedsTargeting { get; }
    TargetType DesiredTarget { get; }
    public void OnEvent(bool isSelfTarget, NetworkId targetId);
}
