using UnityEngine;
using Fusion;
using System.Linq;

public class GameStartManager : NetworkBehaviour 
{
    // [게임 시작 버튼]에 연결될 함수
    public void RequestStartGame()
    {
        // 방장(Host)만 게임을 시작할 수 있어야 함
        if (Runner.IsServer == false) return;

        // 1. 최소 인원 체크 (2명 이상)
        // Runner.ActivePlayers는 현재 방에 들어와 있는 모든 유저 목록입니다.
        if (Runner.ActivePlayers.Count() < 2)
        {
            Debug.LogWarning("피험자가 부족합니다. (최소 2명 필요)");
            return;
        }

        // 2. 방을 리스트에서 숨기기
        // IsVisible을 false로 바꾸는 순간, 로비에 있는 사람들의 목록에서 이 방이 사라집니다.
        // 더 이상 새로운 사람이 난입하지 못하게 막는 효과도 있습니다.
        Runner.SessionInfo.IsVisible = false;

        // 3. 모든 플레이어를 게임 플레이 씬으로 강제 이동
        // Fusion 2의 SceneManager가 모든 클라이언트를 동시에 이동시킵니다.
        Runner.LoadScene(SceneRef.FromIndex(2));
        
        Debug.Log("실험 시작: 방이 비공개로 전환되었습니다.");
    }
}