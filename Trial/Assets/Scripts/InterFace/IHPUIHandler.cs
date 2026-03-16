using UnityEngine;

public interface IHPUIHandler
{
    void RefreshHP(int playerIndex, int hp);
    void RefreshTurn(int currentTurnIndex);
}
