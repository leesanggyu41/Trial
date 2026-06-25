using UnityEngine;
using System.Linq;
using Fusion;

public class DieArm : MonoBehaviour
{
    public int armIndex; // 이 팔이 어느 플레이어의 팔인지 식별하는 인덱스
    public Animator animator;

    public Transform ArmTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerGameData playerGameData;

    public AudioSource audio;
    public AudioClip move;

    public AudioClip crunch;
    public AudioClip dieSound;

    private bool _isDead = false;






    private void Start()
    {
        Invoke("chPlayer", 2f);
    }

    public void Die()
    {
        animator.SetTrigger("Die");
        audio.PlayOneShot(move);
    }

    public void cer()
    {
        audio.PlayOneShot(crunch);
    }

    public void DieSound()
    {
        audio.PlayOneShot(dieSound);
    }

    public void chPlayer()
    {
        var player = FindObjectsByType<PlayerControll>(FindObjectsSortMode.None)
            .FirstOrDefault(p => p.GetComponent<PlayerObject>().PlayerIndex == armIndex);

        if (player != null)
            playerTransform = player.transform;

        if (playerTransform != null)
            playerGameData = playerTransform.GetComponent<PlayerGameData>();
    }

    private void Update()
    {
        if (playerGameData == null) return;
        if (playerGameData.Object == null || !playerGameData.Object.IsValid) return;
        if (!playerGameData.Object.IsInSimulation) return;

        if (!_isDead && playerGameData.IsDead)
        {
            _isDead = true;
            Die();
        }
    }
    public void OnGrabPlayer()
    {
        if (playerTransform != null)
            playerTransform.SetParent(ArmTransform);
    }

}
