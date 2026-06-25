using UnityEngine;
using Fusion;
using System.Collections;

public class GlassEffact : NetworkBehaviour
{
    // Spawned는 포톤 퓨전에서 오브젝트가 네트워크상에 생성 완료되었을 때 실행됩니다.
    public override void Spawned()
    {
        var particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var p in particles)
            p.Play();
        // 1.5초 뒤 디스폰하는 코루틴 시작
        StartCoroutine(DespawnRoutine());
    }

    private IEnumerator DespawnRoutine()
    {
        // 1.5초 동안 대기
        yield return new WaitForSeconds(1.5f);

        // 퓨전에서 오브젝트 삭제는 오직 '서버(StateAuthority)'만 할 수 있습니다.
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}