using UnityEngine;

public class PowerPhone : MonoBehaviour
{
    private Animator animator;
    private AudioSource audio;

    void Start()
    {
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
    }

    public void Open()
    {
        animator.SetTrigger("Open");
        audio.Play();
    }
 
    public void Close()
    {
        animator.SetTrigger("Close");
    }

}
