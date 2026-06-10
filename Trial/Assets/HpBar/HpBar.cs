using System.Collections;
using UnityEngine;
using Fusion;

public class HpBar : NetworkBehaviour
{
    public int Hpbar;

    private int MaxHp = 4;
    private Material GreenLight;
    private Material RedLight;
    private Material Screen;
    private AudioSource audio;
    public AudioClip LightChange;

    public void Start()
    {
        Hpbar = MaxHp;
        Screen = GetComponent<MeshRenderer>().materials[1];
        RedLight = GetComponent<MeshRenderer>().materials[4];
        GreenLight = GetComponent<MeshRenderer>().materials[5];
        audio = GetComponent<AudioSource>();

        RedLight.DisableKeyword("_EMISSION");
        GreenLight.DisableKeyword("_EMISSION");
        Screen.SetFloat("_HP", Hpbar);
    }
    public void SetHP(int hp)
    {
        int damage = Hpbar - hp;
        for (int i = 0; i < damage; i++)
            Hit();
    }

    [ContextMenu("내턴이야!")]
    public void IsMyTurn()
    {
        GreenLight.EnableKeyword("_EMISSION");
        //audio.PlayOneShot(LightChange);
    }

    [ContextMenu("내턴이 아니야!")]
    public void TurnEnd()
    {
        GreenLight.DisableKeyword("_EMISSION");
        //audio.PlayOneShot(LightChange);
    }


    [ContextMenu("아야!")]
    public void Hit()
    {
        Hpbar -= 1;
        StartCoroutine(Flash());
        Screen.SetFloat("_HP", Hpbar);
        audio.Play();

        if (Hpbar <= 0)
        {
            audio.PlayOneShot(LightChange);
            GreenLight.DisableKeyword("_EMISSION");
            RedLight.EnableKeyword("_EMISSION");

        }
    }

    public void Heal()
    {
        Hpbar = Mathf.Clamp(0, MaxHp, Hpbar + 1);
        Screen.SetFloat("_HP", Hpbar);
    }

    IEnumerator Flash()
    {
        Screen.SetFloat("_FlashOn", 1);
        yield return new WaitForSeconds(3f);
        Screen.SetFloat("_FlashOn", 0);
    }


}
