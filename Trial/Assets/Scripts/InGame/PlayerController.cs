// PlayerController는 플레이어의 카메라 제어와 닉네임 표시를 담당하는 클래스입니다.
// 플레이어가 게임에 참여할 때 카메라를 설정하고, 마우스 입력을 받아 카메라의 회전을 제어합니다. 
//또한, 다른 플레이어의 닉네임이 로컬 카메라를 바라보도록 업데이트하여 게임 내에서 플레이어 간의 상호작용을 향상시킵니다.
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.InputSystem;
public class PlayerController : NetworkBehaviour
{
    [Header("카메라 설정")]
    public Transform HeadCameraPoint;
    public float mouseSensitivity = 1f;  
    [Header("카메라 제한")]
    public float Xlimit = 60f;
    public float MinYlimit = -30f;
    public float MaxYlimit = 30f;
    [Header("닉네임")]
    public TMP_Text NameText;
    public Transform NamePoint;

    [Header("플레이어 턴")]
    [Networked] public bool playerTurn {get; set;}

    private Camera _camera;
    private float CameraX;
    private float CameraY;


    public override void Spawned()
{
    if (HasInputAuthority)
    {
        _camera = Camera.main;
        _camera.transform.SetParent(HeadCameraPoint);
        _camera.transform.localPosition = Vector3.zero;
        _camera.transform.localRotation = Quaternion.identity;
        Cursor.lockState = CursorLockMode.Locked;
    }
    StartCoroutine(WaitForNickname());
    
    // PlayerData playerData = GetComponent<PlayerData>();
    // if (playerData != null)
    //     UpdateNameText(playerData.Nickname.ToString());

    // GameSceneManager null 체크
    // if (GameSceneManager.Instance == null)
    // {
    //     Debug.LogError("GameSceneManager를 찾을 수 없음!");
    //     return;
    // }

    // int index = GetComponent<PlayerObject>().PlayerIndex;
    // Transform spawnPoint = GameSceneManager.Instance.GetSpawnPoint(index);

    // if (spawnPoint != null)
    // {
    //     transform.position = spawnPoint.position;
    //     transform.rotation = spawnPoint.rotation;
    // }
}

private System.Collections.IEnumerator WaitForNickname()
{
    PlayerData playerData = GetComponent<PlayerData>();
    if (playerData == null) yield break;

   
    // 자기 오브젝트 저장
    NetworkObject thisObject = GetComponent<NetworkObject>();
    while (string.IsNullOrEmpty(playerData.Nickname.ToString()))
        yield return null;

    // 아직 이 오브젝트가 살아있는지 확인
    if (thisObject == null || !thisObject.IsValid) yield break;

    Debug.Log($"[WaitForNickname] Object: {gameObject.name}, Nickname: {playerData.Nickname}");
    UpdateNameText(playerData.Nickname.ToString());
}
public void UpdateNameText(string nickname)
{
    if (NameText != null)
        NameText.text = nickname;
}
    
    public override void FixedUpdateNetwork()
{
    if (!HasInputAuthority) return;

    var mouse = Mouse.current;
    if (mouse == null) return;

    float mouseX = mouse.delta.x.ReadValue() * mouseSensitivity * 0.1f;
    float mouseY = mouse.delta.y.ReadValue() * mouseSensitivity * 0.1f;

    CameraX += mouseX;
    CameraY -= mouseY;

    CameraX = Mathf.Clamp(CameraX, -Xlimit, Xlimit);
    CameraY = Mathf.Clamp(CameraY, MinYlimit, MaxYlimit);

    HeadCameraPoint.localRotation = Quaternion.Euler(CameraY, CameraX, 0f);
}

// 성씨가 만듬
// 플레이어가 좌클릭으로 오브젝트와 상호 작용
public void CanPlayerTouch(InputAction.CallbackContext context)
{
    // 자신이 오브젝트를 선택할 수 있는지 여부를 검색
    if(!context.started || !playerTurn) return;
    
    Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
    RaycastHit hitInof;

    // 만약 해당 오브젝트가 ReactionObject타입이면 클릭 이벤트 실행
    if(Physics.Raycast(ray, out hitInof, 200f)&& hitInof.collider.TryGetComponent(out ReactionObject reactionObj))
    {
        reactionObj.OnEvent();
    }


}

    private void FixedUpdate()
{
    // 내 캐릭터 닉네임은 움직일 필요 없음
    if (HasInputAuthority) return;
    
    // 상대 닉네임이 로컬 카메라를 바라보게
    Camera localCamera = Camera.main;
    if (NamePoint == null || localCamera == null) return;

    NamePoint.LookAt(NamePoint.position + localCamera.transform.rotation * Vector3.forward, 
        localCamera.transform.rotation * Vector3.up);
}
}


// 성씨가 만듬
// 플레이어가 클릭할 시 반응하는 인터페이스 함수
public interface ReactionObject
{
    public void OnEvent();
}