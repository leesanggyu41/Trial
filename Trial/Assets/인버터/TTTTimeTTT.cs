using UnityEngine;
using System;

public class TTTTimeTTT : MonoBehaviour
{
    MeshRenderer mesh;
    Material mt;
    
    public bool isCharging = false;
    public float ChargeSpeed = 2f;
    
    // [수정] 시작 값을 최소치(4.68)로 변경하고, 목표 값을 최대치(4.93)로 설정합니다.
    public float current = 4.68f; 
    private float maxCharge = 4.93f;

    public event Action OnChargeComplete;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        mt = mesh.materials[3];
        
        // 처음에는 충전이 안 된 비어있는 상태(4.68)로 셰이더 설정
        mt.SetFloat("_PerScent", current); 
    }

    void Update()
    {
        // 사용하기 전(isCharging이 false)에는 대기
        if (!isCharging) return;

        // 아이템 사용 시점부터 게이지를 채웁니다.
        Charging();
    }

    public void Charging()
    {
        // [수정] -= 에서 += 로 변경하여 게이지가 차오르게 만듭니다.
        current += ChargeSpeed * 0.01f * Time.deltaTime;
        mt.SetFloat("_PerScent", current);

        // [수정] 목표 수치(maxCharge = 4.93)에 도달하면 충전 완료 처리
        if (current >= maxCharge)
        {
            isCharging = false; 
            current = maxCharge;
            mt.SetFloat("_PerScent", current);

            // Converter에게 완료 신호 송신
            OnChargeComplete?.Invoke();
        }
    }
public void ResetGauge()
{
    current = 4.68f;
    mt.SetFloat("_PerScent", current);
}
}