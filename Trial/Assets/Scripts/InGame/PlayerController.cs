using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

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
    [Networked] public bool playerTurn { get; set; }

    [Header("아이템 위치")]
    public List<Transform> mySlot = new List<Transform>(); // 아이템이 생성될 위치 리스트 (인덱스 0~7)
    private List<GameObject> heldItems = new List<GameObject>(); // 현재 보유한 아이템 오브젝트 리스트

    [Header("플레이어 TV")]
    [Networked] public int tvnumder{get; set;}
    public GameObject my_TV;

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

        }

        // tv 입력받기
        my_TV = GameSceneManager.Instance.TVPoint[tvnumder];
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


        if (Object.HasInputAuthority)
        {
        // --- 추가: 타겟 고르는 중이면 방향키 입력만 받음 ---
        if (currentState == PlayerState.DecidingTarget)
        {
            HandleTargetSelection();
            return;
        }
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
            // 핵심: 이제 SyringeItem만 찾는게 아니라 모든 ReactionObject를 찾습니다.
            ReactionObject interactable = hitInfo.collider.GetComponentInParent<ReactionObject>();

            if (interactable != null)
            {
                selectedSyringe = interactable; // 리모컨, 주사기 모두 여기 담깁니다.
                InitializeTargetMap();
                currentState = PlayerState.DecidingTarget;

                Debug.Log($"[Click] {hitInfo.collider.gameObject.name} 선택됨! 타겟팅 모드 진입.");
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_tvAnimation(bool isOpen)
    {
        if (my_TV != null)
    {
        var animator = my_TV.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("open", isOpen);
        }
    }
    }
    private void HandleTargetSelection()
    {

        // tv를 한번 열시 상태 반복 x
        if (!my_TV.GetComponent<Animator>().GetBool("open"))
        {
            // tv 애니메이션 작동
            RPC_tvAnimation(true);
        }
        if (selectedSyringe == null) return;


        // 1. 타겟팅이 필요 없는 아이템(리모컨 등)은 즉시 실행
        if (!selectedSyringe.NeedsTargeting)
        {
            Debug.Log("즉시 사용");
            ConfirmUse(true);
            return;
        }

        // 2. 플레이어 타겟팅 아이템 (기존 로직 유지) 아이템 주사기 모두 포함
        if (selectedSyringe.DesiredTarget == TargetType.Player)
        {

            if (Keyboard.current.downArrowKey.wasPressedThisFrame) ConfirmUse(true);

            Vector2 input = Vector2.zero;
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) input = Vector2.up;
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) input = Vector2.left;
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) input = Vector2.right;

            if (input != Vector2.zero && _targetMap.TryGetValue(input, out PlayerControll target))
            {
                RPC_tvAnimation(false);

                ConfirmUse(false, target.GetComponent<NetworkObject>()); // NetworkObject로 전달
            }
        }
        // 3. 주사기 타겟팅 아이템 (감별기, 망치 등)
        else if (selectedSyringe.DesiredTarget == TargetType.Syringe)
        {
            // 여기서 숫자키(1~6)나 다른 방식으로 테이블 위 주사기 선택 로직 추가
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                // 예: 1번 주사기 오브젝트를 찾아서 ConfirmUse 호출
                // ConfirmUse(false, firstSyringeOnTable);
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