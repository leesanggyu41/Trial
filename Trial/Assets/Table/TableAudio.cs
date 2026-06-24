using UnityEngine;

public class TableAudio : MonoBehaviour
{
   private AudioSource audio;

    void Start()
    {
        audio = GetComponent<AudioSource>();

    }

    public void Play()
    {
        audio.Play();
    }
}
