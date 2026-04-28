// GameSceneManager는 게임 씬에서 플레이어의 스폰과 관련된 기능을 관리하는 클래스입니다. 
//플레이어가 게임에 참여할 때 적절한 스폰 포인트를 찾아서 캐릭터를 생성하고, 플레이어의 인덱스를 할당하여 동기화합니다. 
//또한, 플레이어가 게임에서 퇴장할 때 해당 플레이어의 네트워크 객체를 제거하여 게임에서 사라지도록 합니다.
using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[DefaultExecutionOrder(-100)]
public class GameSceneManager : NetworkBehaviour
{
    public static GameSceneManager Instance;

    [Header("캐릭터 스폰포인트 & TV")]
    public Transform[] SpawnPoint;
    public GameObject[] TVPoint;
    [Header("캐릭터 프리팹")]
    public GameObject PlayerPrefab;
    // 플레이어 참조와 네트워크 객체를 관리하는 딕셔너리입니다.
    public Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

    private void Awake()
    {
        Instance = this;
    }
    // 씬이 시작될 때 스폰 포인트를 찾아서 정렬합니다.
    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            foreach (var player in Runner.ActivePlayers)
            {
                SpawnPlayer(player);
            }
        }

    }
    // 플레이어가 게임에 참여할 때 네트워크 객체를 생성하고, 플레이어의 인덱스를 할당하여 동기화합니다.
    public void SpawnPlayer(PlayerRef player)
    {
        int index = 0;

        foreach (var play in Runner.ActivePlayers)
        {
            if (play == player) break;

            index++;

        }
        int count = Runner.ActivePlayers.Count();

        Transform spawnPoint = SpawnPoint[index % SpawnPoint.Length];
        GameObject tv = TVPoint[index % SpawnPoint.Length];

        NetworkObject Playerobj = Runner.Spawn
        (
            PlayerPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            player,
            onBeforeSpawned: (r, obj) =>
            {
                obj.GetComponent<PlayerObject>().PlayerIndex = index;
                obj.GetComponent<PlayerControll>().tvnumder = index % TVPoint.Length;

            }

        );

        _spawnedPlayers.Add(player, Playerobj);
        Runner.SetPlayerObject(player, Playerobj);

        GameTurnManager.Instance.Pt_T.RegisterPlayer(Playerobj.GetComponent<PlayerControll>());

        if (count == Runner.SessionInfo.MaxPlayers // 혹은 아래처럼 스폰된 수로 비교
        || _spawnedPlayers.Count == Runner.SessionInfo.MaxPlayers)
        {
            Debug.Log("모든 인원 스폰 확인! 타겟 맵 초기화 시작.");
            StartCoroutine(DelayedRPC());
        }



    }

    private IEnumerator DelayedRPC()
    {
        // 중요: 모든 플레이어의 NetworkObject가 각 클라이언트에 전파될 시간을 줍니다.
        yield return new WaitForSeconds(1.0f);
        RPC_InitializeTargetMap();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_InitializeTargetMap()
    {
        // 각 클라이언트에서 실행
        StartCoroutine(SafeLocalInit());
    }

    private IEnumerator SafeLocalInit()
    {
        // Local 인스턴스가 잡힐 때까지 대기
        while (PlayerControll.Local == null) yield return null;

        // 마지막으로 한 번 더 아주 잠깐 대기 (물리적 객체 로드 보장)
        yield return new WaitForSeconds(0.2f);

        PlayerControll.Local.InitializeTargetMap();
        Debug.Log($"[TargetMap] {Runner.LocalPlayer.PlayerId}번 플레이어 지도 생성 완료");
    }
    // 플레이어가 게임에서 퇴장할 때 해당 플레이어의 네트워크 객체를 제거하여 게임에서 사라지도록 합니다.
    public Transform GetSpawnPoint(int playerIndex)
    {
        if (playerIndex < SpawnPoint.Length)
            return SpawnPoint[playerIndex];
        return SpawnPoint[0];
    }


}
