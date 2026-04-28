using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using Unity.Cinemachine;

public class PlayerControll : NetworkBehaviour
{
    public static PlayerControll Local { get; private set; }

    [Header("카메라 설정")]
    public Transform HeadCameraPoint;
    public Transform TopCameraPoint;
    public Camera PlayerCamera;
    private bool isTopView = false;

    public float mouseSensitivity = 1f;
    [Header("카메라 제한")]
    public float Xlimit = 60f;
    public float MinYlimit = -120f;
    public float MaxYlimit = 30f;

    [Header("닉네임")]
    public TMP_Text NameText;
    public Transform NamePoint;

    [Header("플레이어 턴")]
    [Networked] public bool playerTurn { get; set; }

    [Header("아이템 위치")]
    public List<Transform> mySlot = new List<Transform>();
    private List<GameObject> heldItems = new List<GameObject>();

    [Header("플레이어 TV")]
    [Networked] public int tvnumder { get; set; }
    public GameObject my_TV;

    public enum PlayerState { Idle, DecidingTarget }
    public PlayerState currentState = PlayerState.Idle;
    private ReactionObject selectedSyringe;
    private Dictionary<Vector2, PlayerControll> _targetMap = new Dictionary<Vector2, PlayerControll>();

    [Header("하이라이트 설정")]
    private GameObject lastHighlightedObject;
    private int defaultLayer;
    private const int OUTLINE_LAYER = 8;

    //private Camera _camera;
    private float CameraX;
    private float CameraY;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            Debug.Log("로컬 플레이어 인스턴스 설정 완료");
        }
        if (HasInputAuthority)
        {
            PlayerCamera = Camera.main;
            TopCameraPoint = GameObject.FindGameObjectWithTag("TopCameraPoint").transform;

            // 시작할 때 HeadCameraPoint에 붙이기
            PlayerCamera.transform.SetParent(HeadCameraPoint);
            PlayerCamera.transform.localPosition = Vector3.zero;
            PlayerCamera.transform.localRotation = Quaternion.identity;

            Cursor.lockState = CursorLockMode.Locked;
        }

        // TV 및 슬롯 초기화
        if (GameSceneManager.Instance != null && GameSceneManager.Instance.TVPoint.Length > tvnumder)
        {
            my_TV = GameSceneManager.Instance.TVPoint[tvnumder];
        }

        FindMyItemSlots();
        StartCoroutine(WaitForNickname());
        //InitializeTargetMap();
    }



    private void Update()
    {
        if (!HasInputAuthority || PlayerCamera == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            OnTopViewButtonClick();
        }

        // 1. 타겟 결정 모드
        if (currentState == PlayerState.DecidingTarget)
        {

            if (selectedSyringe == null) return;

            // 타겟팅 불필요 → 즉시 실행
            if (!selectedSyringe.NeedsTargeting)
            {
                ConfirmUse(true);
                RPC_tvAnimation(false);
                return;
            }

            // 플레이어 타겟팅 → TV 방식
            if (selectedSyringe.DesiredTarget == TargetType.Player)
            {
                HandleTVState();
                if (Mouse.current.leftButton.wasPressedThisFrame) HandleTVClick();
                HandleKeyboardSelection();
                return;
            }

            // 주사기 타겟팅 → 탑뷰 방식
            if (selectedSyringe.DesiredTarget == TargetType.Syringe)
            {
                //HandleSyringeTargeting();
                return;
            }
        }

        // 2. 일반 모드 (아이템 조준 및 하이라이트)
        HandleHighlightUpdate();
    }

    #region [카메라]
    public void OnTopViewButtonClick()
    {
        isTopView = !isTopView;

        if (isTopView)
        {
            // TopCameraPoint에 붙이기
            PlayerCamera.transform.SetParent(TopCameraPoint);
            PlayerCamera.transform.localPosition = Vector3.zero;
            PlayerCamera.transform.localRotation = Quaternion.identity;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // HeadCameraPoint에 다시 붙이기
            PlayerCamera.transform.SetParent(HeadCameraPoint);
            PlayerCamera.transform.localPosition = Vector3.zero;
            PlayerCamera.transform.localRotation = Quaternion.identity;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    #endregion

    #region [TV 클릭 시스템]

    private void HandleTVState()
    {
        if (my_TV == null) return;

        var anim = my_TV.GetComponent<Animator>();
        if (anim != null && !anim.GetBool("open")) // TV가 열리지 않았을 때
        {
            RPC_tvAnimation(true);
        }
        else    // 마우스로 TV를 가리킬 때
        {
            Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                // TV_Script 구성 요소 확인
                TV_Script tvScript = hit.collider.GetComponentInParent<TV_Script>();

                if (tvScript != null && tvScript.gameObject == my_TV)
                {
                    // 클릭된 콜라이더의 인덱스 확인
                    tvScript.PointRotate(hit.collider);
                }
            }
        }


    }

    private void HandleTVClick()
    {
        // 화면 중앙에서 레이를 쏨
        Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            // TV_Script 구성 요소 확인
            TV_Script tvScript = hit.collider.GetComponentInParent<TV_Script>();

            if (tvScript != null && tvScript.gameObject == my_TV)
            {
                // 클릭된 콜라이더의 인덱스 확인
                int clickedIdx = tvScript.GetClickedIndex(hit.collider);
                if (clickedIdx != -1)
                {
                    ProcessTVIndexSelection(clickedIdx);
                }
            }
        }
    }

    private void ProcessTVIndexSelection(int index)
    {
        if (selectedSyringe == null) return;

        // 이미지로 제공해주신 Element 순서 기준
        switch (index)
        {
            case 3: // Element 3: Down (YOU 영역)
                ConfirmUse(true);
                RPC_tvAnimation(false);
                break;

            case 0: // Element 0: Up
                Debug.Log("[TV] 위쪽 방향 선택");
                ExecuteTargetByDirection(Vector2.up);
                break;

            case 2: // Element 2: Left
                ExecuteTargetByDirection(Vector2.left);
                break;

            case 1: // Element 1: Right
                ExecuteTargetByDirection(Vector2.right);
                break;

            default:
                Debug.Log($"[TV] 기능이 할당되지 않은 영역: {index}");
                break;
        }
    }

    private void ExecuteTargetByDirection(Vector2 dir)
    {
        Debug.Log($"[TV] 방향 선택: {dir}");

        Debug.Log($"[TV] _targetMap 크기: {_targetMap.Count}");
        foreach (var kvp in _targetMap)
        {
            Debug.Log($"  키: {kvp.Key} → 플레이어: {kvp.Value.NameText.text}");
        }

        if (_targetMap.TryGetValue(dir, out PlayerControll target))
        {
            Debug.Log($"[TV] 타겟 선택: {target.NameText.text}");
            RPC_tvAnimation(false);
            ConfirmUse(false, target.GetComponent<NetworkObject>());
        }
        else
        {
            Debug.LogWarning($"[TV] {dir} 방향에 해당하는 타겟이 없음!");
        }
    }

    private void HandleKeyboardSelection()
    {
        if (selectedSyringe == null) return;

        if (!selectedSyringe.NeedsTargeting)
        {
            ConfirmUse(true);
            RPC_tvAnimation(false);
            return;
        }

        if (selectedSyringe.DesiredTarget == TargetType.Player)
        {
            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                ConfirmUse(true);
                RPC_tvAnimation(false);
            }

            Vector2 input = Vector2.zero;
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) input = Vector2.up;
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) input = Vector2.left;
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) input = Vector2.right;

            if (input != Vector2.zero) ExecuteTargetByDirection(input);
        }
    }

    #endregion

    #region [아이템 상호작용 및 하이라이트]

    public void CanPlayerTouch(InputAction.CallbackContext context)
    {
        if (PlayerCamera == null) return;
        if (GameTurnManager.Instance == null || GameTurnManager.Instance.NowTurn != GameTurn.Player) return;
        if (!context.started || !playerTurn) return;
        if (currentState == PlayerState.DecidingTarget) return;

        Ray ray = isTopView
            ? PlayerCamera.ScreenPointToRay(Mouse.current.position.ReadValue())
            : PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 200f))
        {
            ReactionObject interactable = hitInfo.collider.GetComponentInParent<ReactionObject>();
            if (interactable != null)
            {
                ItemBase item = hitInfo.collider.GetComponentInParent<ItemBase>();
                if (item != null && item.OwnerRef != Runner.LocalPlayer)
                {
                    return;
                }

                selectedSyringe = interactable;
                //InitializeTargetMap();
                currentState = PlayerState.DecidingTarget;
                // Debug.Log($"[Click] {interactable.gameObject.name} 선택됨!");
            }
        }
    }

    private void HandleHighlightUpdate()
    {

        Ray ray = isTopView
            ? PlayerCamera.ScreenPointToRay(Mouse.current.position.ReadValue())
            : PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 200f))
        {
            ReactionObject reactionObj = hitInfo.collider.GetComponentInParent<ReactionObject>();
            if (reactionObj != null)
            {
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
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }

    #endregion

    #region [네트워크 및 유틸리티]

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        if (isTopView) return; // 탑뷰일 때 시야 이동 막기

        var mouse = Mouse.current;
        if (mouse == null) return;

        CameraX += mouse.delta.x.ReadValue() * mouseSensitivity * 0.1f;
        CameraY -= mouse.delta.y.ReadValue() * mouseSensitivity * 0.1f;
        CameraX = Mathf.Clamp(CameraX, -Xlimit, Xlimit);
        CameraY = Mathf.Clamp(CameraY, MinYlimit, MaxYlimit);
        HeadCameraPoint.localRotation = Quaternion.Euler(CameraY, CameraX, 0f);
    }

    private void ConfirmUse(bool isSelf, NetworkObject targetObj = null)
    {
        if (selectedSyringe != null)
        {
            NetworkId targetId = isSelf ? Object.Id : (targetObj != null ? targetObj.Id : default);
            selectedSyringe.OnEvent(isSelf, targetId);
            selectedSyringe = null;
            currentState = PlayerState.Idle;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_tvAnimation(bool isOpen)
    {
        if (my_TV != null)
        {
            var anim = my_TV.GetComponent<Animator>();
            var tv = my_TV.GetComponent<TV_Script>();
            if (anim != null) anim.SetBool("open", isOpen);
            if (tv != null) tv.TVScreenOn(isOpen);
        }
    }

    public void InitializeTargetMap()
    {
        // 코루틴으로 실행하여 인원이 다 찰 때까지 재시도합니다.
        StartCoroutine(RetryInitializeTargetMap());
    }

    private IEnumerator RetryInitializeTargetMap()
    {
        int expectedCount = Runner.ActivePlayers.Count() - 1;
        int retryCount = 0;
        int maxRetries = 10;

        while (retryCount < maxRetries)
        {
            _targetMap.Clear();
            var otherPlayers = FindObjectsByType<PlayerControll>(FindObjectsSortMode.None)
                                .Where(p => p != this && p.Object != null && p.Object.IsValid)
                                .ToList();

            if (otherPlayers.Count >= expectedCount)
            {
                Vector3 myForward = transform.forward; myForward.y = 0; myForward.Normalize();

                var sorted = otherPlayers.Select(target =>
                {
                    Vector3 dir = (target.transform.position - transform.position);
                    dir.y = 0; dir.Normalize();
                    float angle = Vector3.SignedAngle(myForward, dir, Vector3.up);
                    return (target, angle);
                })
                .OrderBy(x => x.angle)
                .ToList();

                var frontPlayer = sorted.OrderBy(x => Mathf.Abs(x.angle)).First();

                if (Mathf.Abs(frontPlayer.angle) < 30f)
                {
                    // 앞에 플레이어가 있는 경우 → up/left/right 배정
                    _targetMap[Vector2.up] = frontPlayer.target;
                    foreach (var p in sorted.Where(x => x.target != frontPlayer.target))
                    {
                        _targetMap[p.angle < 0 ? Vector2.left : Vector2.right] = p.target;
                    }
                }
                else
                {
                    // 앞에 플레이어가 없는 경우 → left/right만 배정
                    foreach (var p in sorted)
                    {
                        _targetMap[p.angle < 0 ? Vector2.left : Vector2.right] = p.target;
                    }
                }

                Debug.Log($"[TargetMap] 맵 초기화 완료! 타겟 수: {_targetMap.Count}");
                yield break;
            }

            retryCount++;
            Debug.Log($"[TargetMap] 플레이어를 찾는 중... ({retryCount}/{maxRetries})");
            yield return new WaitForSeconds(0.2f);
        }

        Debug.LogError("[TargetMap] 최대 재시도 횟수 초과!");
    }

    private void FindMyItemSlots()
    {
        GameObject[] allSlots = GameObject.FindGameObjectsWithTag("ItemSlot");
        mySlot = allSlots
            .OrderBy(slot => Vector3.Distance(transform.position, slot.transform.position))
            .Take(6)
            .OrderBy(slot => slot.transform.position.x)
            .Select(slot => slot.transform)
            .ToList();
    }

    public void ReceiveItem(GameObject itemObj, int assignedIndex)
    {
        if (!heldItems.Contains(itemObj)) heldItems.Add(itemObj);
        RPC_SyncItemParent(itemObj.GetComponent<NetworkObject>().Id, assignedIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncItemParent(NetworkId itemID, int slotIndex)
    {
        if (Runner.TryFindObject(itemID, out var itemNO))
        {
            if (!heldItems.Contains(itemNO.gameObject)) heldItems.Add(itemNO.gameObject);
            if (slotIndex < mySlot.Count) StartCoroutine(MoveItemToSlot(itemNO.gameObject, mySlot[slotIndex]));
        }
    }

    private IEnumerator MoveItemToSlot(GameObject item, Transform slot)
    {
        float duration = 0.7f;
        float elapsed = 0f;
        Vector3 startPos = item.transform.position;
        while (elapsed < duration)
        {
            if (item == null) yield break;
            elapsed += Time.deltaTime;
            item.transform.position = Vector3.Lerp(startPos, slot.position, elapsed / duration);
            item.transform.rotation = Quaternion.Lerp(item.transform.rotation, slot.rotation, elapsed / duration);
            yield return null;
        }
    }

    private IEnumerator WaitForNickname()
    {
        PlayerData playerData = GetComponent<PlayerData>();
        if (playerData == null) yield break;
        while (string.IsNullOrEmpty(playerData.Nickname.ToString())) yield return null;
        if (NameText != null) NameText.text = playerData.Nickname.ToString();
    }

    private void FixedUpdate()
    {
        if (HasInputAuthority || NamePoint == null || Camera.main == null) return;
        NamePoint.LookAt(NamePoint.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }
    #endregion
}
