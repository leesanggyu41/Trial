using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using Unity.Cinemachine;
using Linework.WideOutline;

public class PlayerControll : NetworkBehaviour
{
    public static PlayerControll Local { get; private set; }

    [SerializeField] private WideOutlineSettings wideOutlineSettings;

    [Header("카메라 설정")]
    public Transform HeadCameraPoint;
    public Transform TopCameraPoint;
    public Camera PlayerCamera;
    public bool isTopView = false;

    private bool _isSeizuring = false;

    public Transform neckBone;

    public Animator animator;

    [Networked] public Quaternion NetworkedHeadRotation { get; set; }

    public float mouseSensitivity = 1f;
    [Header("카메라 제한")]
    public float Xlimit = 60f;
    public float MinYlimit = -120f;
    public float MaxYlimit = 30f;

    [Networked] public float NetworkedCameraX { get; set; }
    [Networked] public float NetworkedCameraY { get; set; }

    [Header("닉네임")]
    public TMP_Text NameText;
    public Transform NamePoint;

    [Header("플레이어 턴")]
    [Networked] public bool playerTurn { get; set; }

    [Header("아이템 위치")]
    public List<Transform> mySlot = new List<Transform>();
    private List<GameObject> heldItems = new List<GameObject>();

    [Header("아이템 설명 UI")]
    private GameObject itemNameUI;
    private GameObject itemExplanationUI;

    [Header("플레이어 TV")]
    [Networked] public int tvnumder { get; set; }
    public GameObject my_TV;

    [Header("로봇팔")]
    private RobotArmFixer _armController;

    public enum PlayerState { Idle, DecidingTarget }
    public PlayerState currentState = PlayerState.Idle;
    private ReactionObject selectedSyringe;
    private Dictionary<Vector2, PlayerControll> _targetMap = new Dictionary<Vector2, PlayerControll>();

    [Header("하이라이트 설정")]
    private GameObject lastHighlightedObject;
    private GameObject selectedHighlightedObject;
    private int defaultLayer;
    private int selectedDefaultLayer;
    private const int OUTLINE_LAYER = 8;

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
            _armController = FindFirstObjectByType<RobotArmFixer>();

            itemNameUI = GameObject.FindGameObjectWithTag("ItemNameUI");
            itemExplanationUI = GameObject.FindGameObjectWithTag("ItemExplanationUI");

            PlayerCamera.transform.SetParent(HeadCameraPoint);
            PlayerCamera.transform.localPosition = Vector3.zero;
            PlayerCamera.transform.localRotation = Quaternion.identity;
            animator = GetComponentInChildren<Animator>();

            Cursor.lockState = CursorLockMode.Locked;
        }

        if (GameSceneManager.Instance != null && GameSceneManager.Instance.TVPoint.Length > tvnumder)
        {
            my_TV = GameSceneManager.Instance.TVPoint[tvnumder];
        }

        FindMyItemSlots();
        StartCoroutine(WaitForNickname());
    }

    private void LateUpdate()
    {
        if (neckBone == null) return;
        if (_isSeizuring) return;

        if (HasInputAuthority)
            neckBone.localRotation = Quaternion.Euler(CameraX * -0.5f, 0f, CameraY * -0.5f);
        else
            neckBone.localRotation = Quaternion.Euler(NetworkedCameraX * -0.5f, 0f, NetworkedCameraY * -0.5f);
    }

    private void Update()
    {
        if (!HasInputAuthority || PlayerCamera == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (SpectatorManager.Instance != null && SpectatorManager.Instance.IsSpectating) return;
            OnTopViewButtonClick();
        }

        if (currentState == PlayerState.DecidingTarget)
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
                HandleTVState();
                var anim = my_TV.GetComponent<Animator>();
                if (anim != null && anim.GetBool("open"))
                {
                    if (Mouse.current.leftButton.wasPressedThisFrame) HandleTVClick();
                }
                HandleKeyboardSelection();
                return;
            }

            if (selectedSyringe.DesiredTarget == TargetType.Syringe)
            {
                HandleSyringeTargeting();
                HandleHighlightUpdate();
                return;
            }
        }

        HandleHighlightUpdate();
    }
    public void OnQuitButton()
    {
        if (Runner == null || !Runner.IsRunning) return;

        // 본인 사망 처리 후 나가기
        RPC_RequestLeave(Object.InputAuthority);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestLeave(PlayerRef leavingPlayer)
    {
        // 사망 처리
        PlayerGameData myData = GetComponent<PlayerGameData>();
        if (myData != null && !myData.IsDead)
            myData.IsDead = true;

        // 서버가 플레이어 정리 (OnPlayerLeft와 동일한 흐름)
        PlayerTurn pt = FindFirstObjectByType<PlayerTurn>();
        if (pt != null) pt.DeletePlayer(this);
    }

    #region [카메라]
    public void OnTopViewButtonClick()
    {
        isTopView = !isTopView;

        if (isTopView)
        {
            PlayerCamera.transform.SetParent(TopCameraPoint);
            PlayerCamera.transform.localPosition = Vector3.zero;
            PlayerCamera.transform.localRotation = Quaternion.identity;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
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
        if (anim != null && !anim.GetBool("open"))
        {
            RPC_tvAnimation(true);
        }
        else
        {
            Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                TV_Script tvScript = hit.collider.GetComponentInParent<TV_Script>();
                if (tvScript != null && tvScript.gameObject == my_TV)
                    tvScript.PointRotate(hit.collider);
            }
        }
    }

    private void HandleTVClick()
    {
        Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            TV_Script tvScript = hit.collider.GetComponentInParent<TV_Script>();
            if (tvScript != null && tvScript.gameObject == my_TV)
            {
                int clickedIdx = tvScript.GetClickedIndex(hit.collider);
                if (clickedIdx != -1)
                    ProcessTVIndexSelection(clickedIdx);
            }
        }
    }

    private void ProcessTVIndexSelection(int index)
    {
        if (selectedSyringe == null) return;

        switch (index)
        {
            case 3:
                ConfirmUse(true);
                RPC_tvAnimation(false);
                break;
            case 0:
                ExecuteTargetByDirection(Vector2.up);
                break;
            case 2:
                ExecuteTargetByDirection(Vector2.left);
                break;
            case 1:
                ExecuteTargetByDirection(Vector2.right);
                break;
            default:
                Debug.Log($"[TV] 기능이 할당되지 않은 영역: {index}");
                break;
        }
    }

    private void ExecuteTargetByDirection(Vector2 dir)
    {
        if (_targetMap.TryGetValue(dir, out PlayerControll target))
        {
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

    #region [주사기 타겟팅 시스템]
    private void HandleSyringeTargeting()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = isTopView
                ? PlayerCamera.ScreenPointToRay(Mouse.current.position.ReadValue())
                : PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                SyringeItem syringe = hit.collider.GetComponentInParent<SyringeItem>();
                if (syringe != null)
                    ConfirmUse(false, syringe.GetComponent<NetworkObject>());
            }
        }
    }
    #endregion

    #region [아이템 상호작용 및 하이라이트]
    public void CanPlayerTouch(InputAction.CallbackContext context)
    {
        if (Runner == null || !Runner.IsRunning) return;
        // 오브젝트가 유효하지 않으면 즉시 리턴
        if (Object == null || !Object.IsValid) return;
        if (PlayerCamera == null) return;
        if (GameTurnManager.Instance == null || GameTurnManager.Instance.NowTurn != GameTurn.Player) return;
        if (!context.started || !playerTurn) return;
        if (currentState == PlayerState.DecidingTarget) return;
        if (SpectatorManager.Instance != null && SpectatorManager.Instance.IsSpectating) return;

        Ray ray = isTopView
            ? PlayerCamera.ScreenPointToRay(Mouse.current.position.ReadValue())
            : PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 200f))
        {
            ReactionObject interactable = hitInfo.collider.GetComponentInParent<ReactionObject>();
            if (interactable != null)
            {
                ItemBase item = hitInfo.collider.GetComponentInParent<ItemBase>();
                if (item != null && item.OwnerRef != default && item.OwnerRef != Runner.LocalPlayer)
                    return;

                selectedSyringe = interactable;
                currentState = PlayerState.DecidingTarget;

                GameObject selectedObj = (selectedSyringe as MonoBehaviour)?.gameObject;
                if (selectedObj != null)
                {
                    selectedHighlightedObject = selectedObj;
                    selectedDefaultLayer = selectedObj.layer;
                    SetLayerRecursively(selectedObj, OUTLINE_LAYER);
                }
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
                if (currentObj == selectedHighlightedObject) return;

                if (lastHighlightedObject != currentObj)
                {
                    ResetHighlight();
                    lastHighlightedObject = currentObj;
                    defaultLayer = currentObj.layer;
                    SetLayerRecursively(currentObj, OUTLINE_LAYER);

                    // 여기서 선언
                    SyringeItem syringe = hitInfo.collider.GetComponentInParent<SyringeItem>();
                    
                    Color outlineColor = Color.white;

                    if (syringe != null
                        && syringe.ScannedByPlayer != PlayerRef.None
                        && syringe.ScannedByPlayer == Runner.LocalPlayer)
                    {
                        outlineColor = syringe.MyType == SyringeType.Toxin ? Color.red : Color.green;
                    }

                    if (wideOutlineSettings.Outlines != null && wideOutlineSettings.Outlines.Count > 0)
                        foreach (var outline in wideOutlineSettings.Outlines)
                            outline.color = outlineColor;

                    wideOutlineSettings.Changed();

                    ItemBase itemBase = hitInfo.collider.GetComponentInParent<ItemBase>();
                    ShowItemInfo(itemBase);
                }
            }
            else
            {
                ResetHighlight();
                ShowItemInfo(null);
            }
        }
        else
        {
            ResetHighlight();
            ShowItemInfo(null);
        }
    }

    private void ShowItemInfo(ItemBase itemBase)
    {
        if (itemNameUI != null)
        {
            bool hasItem = itemBase != null;
            itemNameUI.SetActive(hasItem);
            itemExplanationUI?.SetActive(hasItem);

            if (hasItem)
            {
                var nameTmp = itemNameUI.GetComponentInChildren<TMP_Text>();
                var expTmp = itemExplanationUI?.GetComponentInChildren<TMP_Text>();
                if (nameTmp != null) nameTmp.text = itemBase.ItemName;
                if (expTmp != null) expTmp.text = itemBase.Explanation;
            }
        }
    }

    private void ResetHighlight()
    {
        if (lastHighlightedObject != null)
        {
            SetLayerRecursively(lastHighlightedObject, defaultLayer);
            lastHighlightedObject = null;
            ShowItemInfo(null);
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }
    #endregion

    #region [네트워크 및 유틸리티]
    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        if (isTopView) return;
        if (SpectatorManager.Instance != null && SpectatorManager.Instance.IsSpectating) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        CameraX += mouse.delta.x.ReadValue() * mouseSensitivity * 0.1f;
        CameraY -= mouse.delta.y.ReadValue() * mouseSensitivity * 0.1f;
        CameraX = Mathf.Clamp(CameraX, -Xlimit, Xlimit);
        CameraY = Mathf.Clamp(CameraY, MinYlimit, MaxYlimit);
        HeadCameraPoint.localRotation = Quaternion.Euler(CameraY, CameraX, 0f);

        RPC_SyncCameraRotation(CameraX, CameraY);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SyncCameraRotation(float camX, float camY)
    {
        NetworkedCameraX = camX;
        NetworkedCameraY = camY;
    }

    // ★ 핵심 수정: ConfirmUse → RPC로 모든 클라이언트에 전파
    private void ConfirmUse(bool isSelf, NetworkObject targetObj = null)
    {
        if (selectedSyringe == null) return;

        if (selectedHighlightedObject != null)
        {
            SetLayerRecursively(selectedHighlightedObject, selectedDefaultLayer);
            selectedHighlightedObject = null;
        }

        NetworkId targetId = isSelf ? Object.Id : (targetObj != null ? targetObj.Id : default);
        NetworkObject itemNetObj = (selectedSyringe as MonoBehaviour).GetComponent<NetworkObject>();

        if (itemNetObj == null)
        {
            Debug.LogError("아이템에 NetworkObject가 없음!");
            return;
        }

        selectedSyringe = null;
        currentState = PlayerState.Idle;

        // 모든 클라이언트에서 OnEvent 실행
        RPC_ConfirmUse(itemNetObj.Id, isSelf, targetId);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ConfirmUse(NetworkId itemId, bool isSelf, NetworkId targetId)
    {
        if (!Runner.TryFindObject(itemId, out var itemObj))
        {
            Debug.LogError($"아이템을 찾을 수 없음: {itemId}");
            return;
        }

        ReactionObject reaction = itemObj.GetComponent<ReactionObject>();
        if (reaction == null)
        {
            Debug.LogError("ReactionObject 컴포넌트 없음!");
            return;
        }

        reaction.OnEvent(isSelf, targetId, Object.InputAuthority);
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlaySeizureAnimation()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            StartCoroutine(SeizureDuration());
            Debug.Log($"[Animator] 찾은 애니메이터: {(animator != null ? animator.gameObject.name : "없음")}");
            Debug.Log($"[Seizure] 발작 애니메이션 시작됨: {gameObject.name}");
        }
    }

    private IEnumerator SeizureDuration()
    {
        _isSeizuring = true;
        yield return null; // 한 프레임 대기 후 애니메이션 길이 읽기
        animator.SetTrigger("shock");
        float length = GetComponentInChildren<Animator>()
            .GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);
        _isSeizuring = false;
    }

    public void InitializeTargetMap()
    {
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
                    _targetMap[Vector2.up] = frontPlayer.target;
                    foreach (var p in sorted.Where(x => x.target != frontPlayer.target))
                        _targetMap[p.angle < 0 ? Vector2.left : Vector2.right] = p.target;
                }
                else
                {
                    foreach (var p in sorted)
                        _targetMap[p.angle < 0 ? Vector2.left : Vector2.right] = p.target;
                }

                Debug.Log($"[TargetMap] 맵 초기화 완료! 타겟 수: {_targetMap.Count}");
                yield break;
            }

            retryCount++;
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
            if (slotIndex < mySlot.Count)
                StartCoroutine(MoveItemToSlot(itemNO.gameObject, mySlot[slotIndex]));
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
        NamePoint.LookAt(NamePoint.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);
    }
    #endregion
}