using UnityEngine;

public class Pill : MonoBehaviour
{
    public AudioClip open;
    public AudioClip eat;

    private AudioSource aus;

    void Start()
    {
        aus = GetComponent<AudioSource>();
    }
    public void Open()
    {
        aus.PlayOneShot(open);
    }

    public void Eat()
    {
         aus.PlayOneShot(eat);
    }
}
