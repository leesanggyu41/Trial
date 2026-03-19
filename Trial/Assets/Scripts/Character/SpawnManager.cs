//SpawnManager는 플레이어가 게임에 참여하거나 퇴장할 때 네트워크 객체를 생성 및 제거하는 역할을 합니다.
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    // 네트워크 프리팹 참조와 스폰 포인트 배열을 설정합니다.
    public NetworkPrefabRef playerPrefab;
    public Transform[] SpawnPoints;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    // 씬이 로드될 때마다 스폰 포인트를 찾아서 정렬합니다.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    // 씬이 언로드될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
    // 씬이 로드될 때마다 스폰 포인트를 찾아서 정렬합니다.
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        FindSpawnPoints();
        if (scene.buildIndex == 3 && Runner.IsServer)
    {
        foreach (var networkObject in _spawnedCharacters.Values)
        {
            if (networkObject != null)
                Runner.Despawn(networkObject);
        }
        _spawnedCharacters.Clear();
        Debug.Log("대기씬 오브젝트 정리 완료");
    }
    }
    // 씬이 시작될 때 스폰 포인트를 찾아서 정렬합니다.
    private void FindSpawnPoints()
    {
        GameObject[] spawnObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
        System.Array.Sort(spawnObjs, (a, b) => string.Compare(a.name, b.name));

        SpawnPoints = new Transform[spawnObjs.Length];
        for (int i = 0; i < spawnObjs.Length; i++)
            SpawnPoints[i] = spawnObjs[i].transform;

        Debug.Log($"SpawnPoint {SpawnPoints.Length}개 찾음");
    }
    // 플레이어가 게임에 참여할 때 네트워크 객체를 생성하고, 플레이어의 인덱스를 할당하여 동기화합니다.
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
    // 플레이어가 게임에서 퇴장할 때 해당 플레이어의 네트워크 객체를 제거하여 게임에서 사라지도록 합니다.
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