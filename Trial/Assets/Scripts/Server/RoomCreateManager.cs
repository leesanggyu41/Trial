using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System.Collections.Generic;

public class RoomCreateManager : MonoBehaviour
{
    // 필드에서 _runner를 유지하지 않고, 함수 내에서 생성해서 사용하거나 
    // 기존에 사용 중인게 있다면 파괴 후 새로 만들어야 합니다.
    private NetworkRunner _currentRunner; 

    [Header("UI 연결")]
    public TMP_InputField roomNameInput;
    public Toggle passwordToggle;
    public TMP_InputField passwordInput;

    [Header("Spawn 설정")]
    public NetworkPrefabRef playerPrefab;


    public async void OnClickCreateRoom()
    {
        // [수정 포인트 1] 이미 사용 중이거나 씬에 남은 Runner가 있다면 정리
        if (_currentRunner != null)
        {
            if (_currentRunner.IsRunning) await _currentRunner.Shutdown();
            Destroy(_currentRunner.gameObject); 
        }

        // [수정 포인트 2] 매번 새로운 Runner를 동적으로 생성 (가장 안전)
        GameObject runnerObj = new GameObject("Fusion_Runner");
        DontDestroyOnLoad(runnerObj);
        _currentRunner = runnerObj.AddComponent<NetworkRunner>();

        SpawnManager spawnManager = runnerObj.AddComponent<SpawnManager>();
        spawnManager.playerPrefab = playerPrefab;

        // 방 이름 설정
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName)) roomName = $"Experiment_{Random.Range(1000, 9999)}";

        // 커스텀 프로퍼티 설정
        var customProps = new Dictionary<string, SessionProperty>();
        customProps["IsPassword"] = passwordToggle.isOn ? 1 : 0;
        if (passwordToggle.isOn) customProps["PwData"] = passwordInput.text;

        // [수정 포인트 3] SceneManager도 새 Runner 오브젝트에 붙여서 전달
        var result = await _currentRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            PlayerCount = 4,
            SessionProperties = customProps,
            Scene = SceneRef.FromIndex(2),
            // 새 Runner 오브젝트에 매니저를 붙여서 관리 일원화
            SceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok) {
            Debug.Log("방 생성 성공");
        } else {
            Debug.LogError($"방 생성 실패: {result.ShutdownReason}");
            // 실패 시 생성했던 오브젝트 파괴
            Destroy(runnerObj);
        }
        
    }

}