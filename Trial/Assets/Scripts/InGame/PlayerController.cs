using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerControll : NetworkBehaviour
{
    [Header("카메라 설정")]
    public Transform HeadCameraPoint;
    public float mouseSensitivity = 1f;  
    [Header("카메라 제한")]
    public float Xlimit = 60f;
    public float MinYlimit = -120f;
    public float MaxYlimit = 30f;
    [Header("닉네임")]
    public TMP_Text NameText;
    public Transform NamePoint;

    [Header("플레이어 턴")]
    [Networked] public bool playerTurn {get; set;}

    public enum PlayerState { Idle, DecidingTarget }
public PlayerState currentState = PlayerState.Idle;
private SyringeItem selectedSyringe;
private Dictionary<Vector2, PlayerControll> _targetMap = new Dictionary<Vector2, PlayerControll>();
    
    // --- 윤곽선 관련 변수 추가 ---
    private GameObject lastHighlightedObject; 
    private int defaultLayer; 
    private const int OUTLINE_LAYER = 8; // 설정하신 8번 레이어
    // --------------------------

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
    }

    private System.Collections.IEnumerator WaitForNickname()
    {
        PlayerData playerData = GetComponent<PlayerData>();
        if (playerData == null) yield break;

        NetworkObject thisObject = GetComponent<NetworkObject>();
        while (string.IsNullOrEmpty(playerData.Nickname.ToString()))
            yield return null;

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

    // --- 추가: 매 프레임 레이캐스트로 윤곽선 레이어 변경 ---
    private void Update()
{
    if (!HasInputAuthority || _camera == null) return;

    // --- 추가: 타겟 고르는 중이면 방향키 입력만 받음 ---
    if (currentState == PlayerState.DecidingTarget)
    {
        HandleTargetSelection();
        return; 
    }

    Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    RaycastHit hitInfo;

    if (Physics.Raycast(ray, out hitInfo, 200f))
    {
        // 자식 콜라이더를 쳐도 부모의 스크립트를 찾아냄
        ReactionObject reactionObj = hitInfo.collider.GetComponentInParent<ReactionObject>();

        if (reactionObj != null)
        {
            // 실제 레이어를 바꿔야 하는 대상은 최상위 부모(스크립트가 붙은 오브젝트)
            GameObject currentObj = (reactionObj as MonoBehaviour).gameObject;

            if (lastHighlightedObject != currentObj)
            {
                ResetHighlight();
                lastHighlightedObject = currentObj;
                defaultLayer = currentObj.layer;
                SetLayerRecursively(currentObj, OUTLINE_LAYER);
            }
        }
        else { ResetHighlight(); }
    }
    else { ResetHighlight(); }
}

    private void ResetHighlight()
    {
        if (lastHighlightedObject != null)
        {
            SetLayerRecursively(lastHighlightedObject, defaultLayer);
            lastHighlightedObject = null;
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    // ---------------------------------------------------

public void CanPlayerTouch(InputAction.CallbackContext context)
{
    // 1. 클릭 시작(Started) 시점인지 + 내 턴인지 확인
    if (!context.started || !playerTurn) return;

    // 2. 카메라나 마우스가 없는 예외 상황 방지
    if (_camera == null || Mouse.current == null) return;
    Debug.Log("플레이어가 클릭을 시작했습니다. 레이캐스트를 발사합니다.");

    // 3. 화면 중앙 조준점 기준으로 레이 발사
    Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    RaycastHit hitInfo;

    // 200f 거리 내에 있는 모든 물체 감지
    if (Physics.Raycast(ray, out hitInfo, 200f))
    {
        // 핵심: 자식(Mesh)을 쳤더라도 부모에 있는 SyringeItem 스크립트를 찾아냄
        SyringeItem syringe = hitInfo.collider.GetComponentInParent<SyringeItem>();

        if (syringe != null)
        {
            Debug.Log($"[Click] 주사기 {syringe.gameObject.name} 선택됨! 방향키로 타겟을 정하세요.");
            
            // 1. 선택한 주사기 저장
            selectedSyringe = syringe;
            InitializeTargetMap();
            // 2. 상태를 '타겟 결정 중'으로 변경 (이제 Update에서 HandleTargetSelection이 실행됨)
            currentState = PlayerState.DecidingTarget;
        }    
    }
}

    private void FixedUpdate()
    {
        if (HasInputAuthority) return;
        
        Camera localCamera = Camera.main;
        if (NamePoint == null || localCamera == null) return;

        NamePoint.LookAt(NamePoint.position + localCamera.transform.rotation * Vector3.forward, 
            localCamera.transform.rotation * Vector3.up);
    }

public void InitializeTargetMap()
{
    _targetMap.Clear();
    var otherPlayers = FindObjectsByType<PlayerControll>(FindObjectsSortMode.None)
        .Where(p => p != this).ToList();

    Vector3 myForward = HeadCameraPoint.forward; myForward.y = 0; myForward.Normalize();
    Vector3 myRight   = HeadCameraPoint.right;   myRight.y   = 0; myRight.Normalize();

    foreach (var target in otherPlayers)
    {
        Vector3 dir = (target.transform.position - transform.position);
        dir.y = 0; dir.Normalize();

        float dotF = Vector3.Dot(myForward, dir);
        float dotR = Vector3.Dot(myRight, dir);

        // 엄격한 임계값 대신 가장 가까운 방향으로 매칭
        if (Mathf.Abs(dotF) >= Mathf.Abs(dotR))
        {
            if (dotF > 0) _targetMap[Vector2.up]   = target;
            // 필요하다면: else _targetMap[Vector2.down] = target;
        }
        else
        {
            if (dotR > 0) _targetMap[Vector2.right] = target;
            else          _targetMap[Vector2.left]  = target;
        }

        Debug.Log($"[TargetMap] {target.name} → dotF:{dotF:F2} dotR:{dotR:F2}");
    }
}

private void HandleTargetSelection()
{
    if (Keyboard.current.downArrowKey.wasPressedThisFrame) ConfirmUse(true);
    
    Vector2 input = Vector2.zero;
    if (Keyboard.current.upArrowKey.wasPressedThisFrame) input = Vector2.up;
    else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) input = Vector2.left;
    else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) input = Vector2.right;

    if (input != Vector2.zero)
    {
        if (_targetMap.TryGetValue(input, out PlayerControll target))
            ConfirmUse(false, target);
        else
            Debug.Log($"[TargetSelection] {input} 방향에 타겟 없음. 현재 맵: {string.Join(", ", _targetMap.Keys.Select(k => k.ToString()))}");
    }

    
}

private void ConfirmUse(bool isSelf, PlayerControll target = null)
{
    if (selectedSyringe != null)
    {
        NetworkId targetId = isSelf ? Object.Id : (target != null ? target.Object.Id : default);
        selectedSyringe.OnEvent(isSelf, targetId); 
        selectedSyringe = null;
        currentState = PlayerState.Idle;
    }
}
}