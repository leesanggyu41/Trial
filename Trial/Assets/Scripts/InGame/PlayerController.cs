using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class PlayerControll : NetworkBehaviour
{
    [Header("TV 설정")]
    public GameObject MyTv;
    private List<TvTargetButton> _myTvButtons = new List<TvTargetButton>();

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
    [Networked] public bool playerTurn { get; set; }

    [Header("아이템 위치")]
    public List<Transform> mySlot = new List<Transform>(); // 아이템이 생성될 위치 리스트 (인덱스 0~7)
    private List<GameObject> heldItems = new List<GameObject>(); // 현재 보유한 아이템 오브젝트 리스트

    public enum PlayerState { Idle, DecidingTarget }
    public PlayerState currentState = PlayerState.Idle;
    private ReactionObject selectedSyringe;
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
            // 맵에 있는 모든 TV 오브젝트를 찾음 (태그 "TV" 권장)
            GameObject[] allTvs = GameObject.FindGameObjectsWithTag("TV");

            if (allTvs.Length > 0)
            {
                // 내 위치에서 가장 가까운 TV 하나를 선택
                MyTv = allTvs
                    .OrderBy(tv => Vector3.Distance(transform.position, tv.transform.position))
                    .First();

                // 그 TV 안에 있는 모든 버튼(TvTargetButton)들을 미리 리스트에 담아둠
                _myTvButtons = MyTv.GetComponentsInChildren<TvTargetButton>(true).ToList();

                Debug.Log($"{gameObject.name}가 가장 가까운 TV({MyTv.name})를 할당받았습니다.");
            }
        }
        FindMyItemSlots();
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

    private void FindMyItemSlots()
    {
        // 씬에 있는 모든 슬롯(ItemSlot 태그)을 찾음
        GameObject[] allSlots = GameObject.FindGameObjectsWithTag("ItemSlot");

        // 내 위치에서 가까운 순서대로 6개를 뽑음
        mySlot = allSlots
            .OrderBy(slot => Vector3.Distance(transform.position, slot.transform.position))
            .Take(6)
            // 슬롯들이 왼쪽->오른쪽 순서대로 정렬되도록 X축(혹은 Z축) 정렬 추가
            .OrderBy(slot => slot.transform.position.x)
            .Select(slot => slot.transform)
            .ToList();

        Debug.Log($"{gameObject.name} 슬롯 {mySlot.Count}개 할당 완료");
    }
    // 아이템을 받아서 빈 슬롯으로 이동시키는 함수
    public void ReceiveItem(GameObject itemObj, int assignedIndex)
    {
        // 서버 리스트 업데이트
        if (!heldItems.Contains(itemObj)) heldItems.Add(itemObj);

        // 모든 클라이언트에게 "이 아이템은 n번 슬롯이다"라고 확실히 박아줌
        RPC_SyncItemParent(itemObj.GetComponent<NetworkObject>().Id, assignedIndex);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncItemParent(NetworkId itemID, int slotIndex)
    {
        if (Runner.TryFindObject(itemID, out var itemNO))
        {
            // 모든 클라이언트가 자기 리스트에 추가
            if (!heldItems.Contains(itemNO.gameObject)) heldItems.Add(itemNO.gameObject);

            if (slotIndex < mySlot.Count)
            {
                Transform targetSlot = mySlot[slotIndex];

                // 이제 여기서 코루틴 실행!
                StartCoroutine(MoveItemToSlot(itemNO.gameObject, targetSlot));
            }
        }
    }
    private IEnumerator MoveItemToSlot(GameObject item, Transform slot)
    {
        float duration = 0.7f; // 날아가는 시간
        float elapsed = 0f;
        Vector3 startPos = item.transform.position;

        while (elapsed < duration)
        {
            if (item == null) yield break;
            elapsed += Time.deltaTime;

            // 부드러운 이동 (Lerp)
            item.transform.position = Vector3.Lerp(startPos, slot.position, elapsed / duration);
            item.transform.rotation = Quaternion.Lerp(item.transform.rotation, slot.rotation, elapsed / duration);
            yield return null;
        }


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
                // 인터페이스를 상속받은 실제 MonoBehaviour 컴포넌트를 가져옴
                MonoBehaviour mono = reactionObj as MonoBehaviour;

                if (lastHighlightedObject != mono)
                {
                    GameObject currentObj = mono.gameObject;
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
        if (GameTurnManager.Instance.NowTurn != GameTurn.Player) return;
        if (!context.started || !playerTurn) return;

        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 200f))
        {
            ReactionObject interactable = hitInfo.collider.GetComponentInParent<ReactionObject>();

            if (interactable != null)
            {
                selectedSyringe = interactable;

                if (!selectedSyringe.NeedsTargeting)
                {
                    ConfirmUse(true);
                }
                // 타겟팅이 필요하고 타겟이 플레이어일 때 TV 작동
                else if (selectedSyringe.DesiredTarget == TargetType.Player)
                {
                    // 1. 타겟 맵 계산
                    InitializeTargetMap();

                    // 2. TV UI 갱신 (가까운 TV의 버튼들에 닉네임 할당)
                    UpdateTvButtonsUI();

                    // 3. TV 애니메이션 재생 (Animator가 MyTv에 있다고 가정)
                    if (MyTv != null && MyTv.TryGetComponent<Animator>(out var anim))
                    {
                        anim.SetTrigger("Open"); // "Open"은 Animator에 설정한 트리거 이름
                    }

                    currentState = PlayerState.DecidingTarget;
                    
                }
            }
        }
    }
    private void UpdateTvButtonsUI()
    {
        if (_myTvButtons == null || _myTvButtons.Count == 0) return;

        foreach (var btn in _myTvButtons)
        {
            if (_targetMap.TryGetValue(btn.DirectionKey, out PlayerControll target))
            {
                // 해당 방향에 플레이어가 있으면 닉네임 표시하고 활성화
                string nick = target.GetComponent<PlayerData>().Nickname.ToString();
                btn.SetName(nick);
                btn.gameObject.SetActive(true);
                // 만약 버튼에 콜라이더가 자식에 있다면 그것도 체크해야 함
            }
            else
            {
                // 플레이어가 없는 방향의 버튼은 비활성화
                btn.gameObject.SetActive(false);
            }
        }
    }
    private void CloseTv()
    {
        // TV 닫기 애니메이션
        if (MyTv != null && MyTv.TryGetComponent<Animator>(out var anim))
        {
            anim.SetTrigger("Close");
        }

        currentState = PlayerState.Idle;
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
        Vector3 myRight = HeadCameraPoint.right; myRight.y = 0; myRight.Normalize();

        foreach (var target in otherPlayers)
        {
            Vector3 dir = (target.transform.position - transform.position);
            dir.y = 0; dir.Normalize();

            float dotF = Vector3.Dot(myForward, dir);
            float dotR = Vector3.Dot(myRight, dir);

            // 엄격한 임계값 대신 가장 가까운 방향으로 매칭
            if (Mathf.Abs(dotF) >= Mathf.Abs(dotR))
            {
                if (dotF > 0) _targetMap[Vector2.up] = target;
                // 필요하다면: else _targetMap[Vector2.down] = target;
            }
            else
            {
                if (dotR > 0) _targetMap[Vector2.right] = target;
                else _targetMap[Vector2.left] = target;
            }

            Debug.Log($"[TargetMap] {target.name} → dotF:{dotF:F2} dotR:{dotR:F2}");
        }
    }

    private void HandleTargetSelection()
    {
        if (selectedSyringe == null) return;

        // 1. 즉시 사용 (각성제 등)
        if (!selectedSyringe.NeedsTargeting)
        {
            ConfirmUse(true);
            return;
        }

        // 2. TV 조준 클릭 (기존 키보드 로직을 대체)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                TvTargetButton btn = hit.collider.GetComponent<TvTargetButton>();

                // 기존에 키보드로 하던 짓: _targetMap[input] -> ConfirmUse
                // 이제 버튼이 가진 DirectionKey로 맵에서 플레이어를 찾음
                if (btn != null && _targetMap.TryGetValue(btn.DirectionKey, out PlayerControll target))
                {
                    Debug.Log($"{btn.DirectionKey} 방향의 {target.name} 선택!");
                    ConfirmUse(false, target.GetComponent<NetworkObject>());
                    CloseTv(); // TV 닫기
                }
            }
        }
    }

    private void ConfirmUse(bool isSelf, NetworkObject targetObj = null)
    {
        if (selectedSyringe != null)
        {
            // 자가 사용이면 내 ID, 타겟이 있으면 그 오브젝트(플레이어/주사기)의 ID
            NetworkId targetId = isSelf ? Object.Id : (targetObj != null ? targetObj.Id : default);

            selectedSyringe.OnEvent(isSelf, targetId);

            selectedSyringe = null;
            currentState = PlayerState.Idle;
        }
    }
}