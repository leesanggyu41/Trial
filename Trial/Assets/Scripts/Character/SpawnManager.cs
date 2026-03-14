using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public NetworkPrefabRef playerPrefab;
    public Transform[] SpawnPoints;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        FindSpawnPoints();
    }

    private void FindSpawnPoints()
    {
        GameObject[] spawnObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
        System.Array.Sort(spawnObjs, (a, b) => string.Compare(a.name, b.name));

        SpawnPoints = new Transform[spawnObjs.Length];
        for (int i = 0; i < spawnObjs.Length; i++)
            SpawnPoints[i] = spawnObjs[i].transform;

        Debug.Log($"SpawnPoint {SpawnPoints.Length}개 찾음");
    }
    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log("플레이어 들어옴");
        if (Runner.IsServer)
        {
            var usedIndices = _spawnedCharacters.Values
                .Select(obj => obj.GetComponent<PlayerObject>().PlayerIndex)
                .ToList();

            int assignedIndex = 0;
            while (usedIndices.Contains(assignedIndex))
                assignedIndex++;

            Transform spawnPoint = SpawnPoints[assignedIndex % SpawnPoints.Length];

            NetworkObject networkPlayerObject = Runner.Spawn(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
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