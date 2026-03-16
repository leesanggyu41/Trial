using UnityEngine;
using UnityEngine.InputSystem;

public class TestCube : MonoBehaviour
{
    public enum CubeType { Damage, Heal }
    public CubeType cubeType;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 레이캐스트로 클릭한 오브젝트 감지
            Vector2 mousePos = Mouse.current.position.ReadValue();
Ray ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 클릭한 오브젝트가 이 큐브인지 확인
                if (hit.collider.gameObject == gameObject)
                    OnClicked();
            }
        }
    }

    private void OnClicked()
    {
        PlayerGameData playerData = FindLocalPlayer();
        if (playerData == null) return;

        if (cubeType == CubeType.Damage)
            playerData.TakeDamage(1);
        else
            playerData.Heal(1);
    }

    private PlayerGameData FindLocalPlayer()
    {
        var playerDataList = FindObjectsByType<PlayerGameData>(FindObjectsSortMode.None);
        foreach (var playerData in playerDataList)
        {
            if (playerData.HasInputAuthority)
                return playerData;
        }
        return null;
    }
}