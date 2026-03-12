using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public NetworkPrefabRef playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            var usedIndices = _spawnedCharacters.Values
                .Select(obj => obj.GetComponent<PlayerObject>().PlayerIndex)
                .ToList();

            int assignedIndex = 0;
            while (usedIndices.Contains(assignedIndex))
                assignedIndex++;

            NetworkObject networkPlayerObject = Runner.Spawn(
                playerPrefab,
                Vector3.zero,
                Quaternion.identity,
                player,
                onBeforeSpawned: (r, obj) =>
                {
                    obj.GetComponent<PlayerObject>().PlayerIndex = assignedIndex;
                }
            );

            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer && _spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            Runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
            Debug.Log($"{player.PlayerId} 퇴장.");
        }
    }
}