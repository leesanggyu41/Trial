using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class AudioChange : MonoBehaviour
{
    public AudioSource au;
    public AudioClip crash;
    public AudioClip open;
    public ParticleSystem[] pa;

    public void Start()
    {
        au = GetComponent<AudioSource>();
        for(int i = 0; i < pa.Length; i++)
        {
            pa[i].Stop();
        }
    }

    public void Open()
    {
        au.PlayOneShot(open);
    }

    public void Crash()
    {
        au.Stop();
        au.PlayOneShot(crash);
        for(int i = 0; i < pa.Length; i++)
        {
            pa[i].Play();
        }
    }

    public void CrashStop()
    {
         au.PlayOneShot(open);
        for(int i = 0; i < pa.Length; i++)
        {
            pa[i].Stop();
        }
    }
}
