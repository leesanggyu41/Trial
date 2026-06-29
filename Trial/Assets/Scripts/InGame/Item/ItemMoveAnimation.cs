using UnityEngine;
using Fusion;
using System.Linq;

public class ItemMoveAnimation : NetworkBehaviour
{
    [Header("목표")]
    public Transform targetPoint;
    public Vector3 targetEulerRotation;
    public System.Action onMoveComplete;

    [Header("애니메이션 시간")]
    public float moveDuration = 1.5f;

    [Networked] private Vector3 NetworkedStartPosition { get; set; }
    [Networked] private Vector3 NetworkedTargetPosition { get; set; } // targetPoint 위치 동기화
    [Networked] private NetworkBool IsMoving { get; set; }
    [Networked] private float Timer { get; set; }

    private NetworkBool _prevIsMoving;
    private Quaternion startRotation;
    private Quaternion targetRotation;

    public void MoveToTarget(PlayerRef ownerRef)
    {
        if (Object.HasStateAuthority)
        {
            if (targetPoint == null) return;

            NetworkedStartPosition = transform.position;
            NetworkedTargetPosition = targetPoint.position;
            Timer = 0f;
            IsMoving = true;
        }

        startRotation = transform.rotation;

        // 아이템 소유 플레이어 찾기
        var ownerPlayer = FindObjectsByType<PlayerControll>(FindObjectsSortMode.None)
            .FirstOrDefault(p => p.Object.InputAuthority == ownerRef);

        if (ownerPlayer != null)
        {
            // 아이템 위치에서 플레이어를 바라보는 방향으로 회전
            Vector3 directionToPlayer = ownerPlayer.transform.position - NetworkedTargetPosition;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
                targetRotation = Quaternion.LookRotation(directionToPlayer);
            else
                targetRotation = targetPoint != null
                    ? targetPoint.rotation
                    : Quaternion.Euler(targetEulerRotation);
        }
        else
        {
            targetRotation = targetPoint != null
                ? targetPoint.rotation
                : Quaternion.Euler(targetEulerRotation);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!IsMoving) return;

        Timer += Runner.DeltaTime;

        float t = Mathf.Clamp01(Timer / moveDuration);
        t = Mathf.SmoothStep(0f, 1f, t);

        transform.position = Vector3.Lerp(NetworkedStartPosition, NetworkedTargetPosition, t);

        if (Timer >= moveDuration)
        {
            transform.position = NetworkedTargetPosition;
            transform.rotation = targetRotation;
            IsMoving = false;
            onMoveComplete?.Invoke();
        }
    }

    public override void Render()
    {
        if (!IsMoving) return;

        float t = Mathf.Clamp01(Timer / moveDuration);
        t = Mathf.SmoothStep(0f, 1f, t);

        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
    }
}