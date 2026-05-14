using UnityEngine;

public class LocalRocate : MonoBehaviour
{
    [Header("회전 속도 (도/초)")]
    public float rotationX = 0f;
    public float rotationY = 90f;
    public float rotationZ = 0f;

    void Update()
    {
        transform.Rotate(rotationX * Time.deltaTime,
                         rotationY * Time.deltaTime,
                         rotationZ * Time.deltaTime,
                         Space.Self);  // Local 기준 회전
    }
}