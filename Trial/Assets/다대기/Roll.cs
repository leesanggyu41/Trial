using UnityEngine;

public class Roll : MonoBehaviour
{
    public float RollSpeed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = new Vector3(0,1,0) * RollSpeed * Time.deltaTime;
        transform.Rotate(dir);
    }
}
