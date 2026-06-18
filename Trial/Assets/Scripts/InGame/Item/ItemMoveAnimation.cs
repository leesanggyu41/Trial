using UnityEngine;


public class ItemMoveAnimation : MonoBehaviour
{
    [Header("목표")]
    public Transform targetPoint;
    public Vector3 targetEulerRotation;

    [Header("애니메이션 시간")]
    public float moveDuration = 1.5f;

    private bool isMoving;
    private float timer;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion targetRotation;



    [ContextMenu("타깃으로 이동")]
    public void MoveToTarget()
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("Target Point가 지정되지 않았습니다.");
            return;
        }
        Debug.Log("이동해요용요요용요요요요요요ㅛ요요요요요요요요요요요용");

        startPosition = transform.position;
        startRotation = transform.rotation;

        targetRotation = Quaternion.Euler(targetEulerRotation);

        timer = 0f;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / moveDuration);

        // 부드러운 가속/감속
        t = Mathf.SmoothStep(0f, 1f, t);

        // 위치 보간
        transform.position = Vector3.Lerp(
            startPosition,
            targetPoint.position,
            t);

        // 회전 보간
        transform.rotation = Quaternion.Slerp(
            startRotation,
            targetRotation,
            t);

        if (timer >= moveDuration)
        {
            transform.position = targetPoint.position;
            transform.rotation = targetRotation;
            isMoving = false;
        }
    }
}