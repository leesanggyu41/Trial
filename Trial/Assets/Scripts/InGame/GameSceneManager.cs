using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
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
    public Transform GetSpawnPoint(int playerIndex)
    {
        if (playerIndex < SpawnPoint.Length)
            return SpawnPoint[playerIndex];
        return SpawnPoint[0];
    }
}
