using UnityEngine;
using Fusion;

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

    public void MoveToTarget()
    {
        // StateAuthority만 targetPoint 필요
        if (Object.HasStateAuthority)
        {
            if (targetPoint == null)
            {
                Debug.LogWarning("Target Point가 지정되지 않았습니다.");
                return;
            }

            NetworkedStartPosition = transform.position;
            NetworkedTargetPosition = targetPoint.position;
            Timer = 0f;
            IsMoving = true;
        }

        startRotation = transform.rotation;
          targetRotation = targetPoint != null 
        ? targetPoint.rotation  // targetPoint의 회전값을 목표로
        : Quaternion.Euler(targetEulerRotation);
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
        transform.rotation = targetPoint != null ? targetPoint.rotation : Quaternion.Euler(targetEulerRotation);
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