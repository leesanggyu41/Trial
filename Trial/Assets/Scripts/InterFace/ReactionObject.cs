using UnityEngine;
using Fusion;
public interface ReactionObject
{
    public void OnEvent(bool isSelfTarget, NetworkId targetId);
}
