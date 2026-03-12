using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public NetworkPrefabRef playerPrefab;
    public NetworkPrefabRef playerDatafab;
    public NetworkRunner _runner;


    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    // Spawned() 대신 SimulationBehaviour는 이미 Runner에 접근 가능
    // AddCallbacks도 필요 없음 - 인터페이스만 구현하면 Fusion이 자동으로 호출
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

        // 뒷배경 캐릭터 스폰
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

        // 닉네임 데이터 오브젝트 스폰
        Runner.Spawn(playerDatafab, Vector3.zero, Quaternion.identity, player);

        _spawnedCharacters.Add(player, networkPlayerObject);
    }
}

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer && _spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            Runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
            Debug.Log($"{player.PlayerId} 퇴장. 해당 슬롯이 비었습니다.");
        }
    }
}