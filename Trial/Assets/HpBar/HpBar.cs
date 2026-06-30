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

    public AudioClip TurnChangeSound;

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
    int diff = Hpbar - hp;
    
    if (diff > 0)
    {
        // 데미지
        for (int i = 0; i < diff; i++)
            Hit();
    }
    else if (diff < 0)
    {
        // 힐
        for (int i = 0; i < -diff; i++)
            Heal();
    }
}

    [ContextMenu("내턴이야!")]
    public void IsMyTurn()
    {
        GreenLight.EnableKeyword("_EMISSION");
        audio.PlayOneShot(TurnChangeSound);
    }

    [ContextMenu("내턴이 아니야!")]
    public void TurnEnd()
    {
        GreenLight.DisableKeyword("_EMISSION");
        //audio.PlayOneShot(TurnChangeSound);
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
        Hpbar = Mathf.Clamp(Hpbar + 1, 0, MaxHp);
        Screen.SetFloat("_HP", Hpbar);
    }

    IEnumerator Flash()
    {
        Screen.SetFloat("_FlashOn", 1);
        yield return new WaitForSeconds(3f);
        Screen.SetFloat("_FlashOn", 0);
    }


}
