using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class GameSceneManager : NetworkBehaviour
{
    public static GameSceneManager Instance;

    [Header("캐릭터 스폰포인트")]
    public Transform[] SpawnPoint;
    [Header("캐릭터 프리팹")]
    public GameObject PlayerPrefab;
    
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            foreach(var player in Runner.ActivePlayers)
            {
                SpawnPlayer(player);
            }
        }
    }

    public void SpawnPlayer(PlayerRef player)
    {
        int index = 0;

        foreach(var play in Runner.ActivePlayers)
        {
            if(play == player) break;

            index++;
            
        }

        Transform spawnPoint = SpawnPoint[index % SpawnPoint.Length];

        NetworkObject Playerobj = Runner.Spawn
        (
            PlayerPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            player,
            onBeforeSpawned: (r, obj) =>
            {
                obj.GetComponent<PlayerObject>().PlayerIndex = index;
            }
        );

        _spawnedPlayers.Add(player, Playerobj);
    }
    public Transform GetSpawnPoint(int playerIndex)
    {
        if (playerIndex < SpawnPoint.Length)
            return SpawnPoint[playerIndex];
        return SpawnPoint[0];
    }
}
