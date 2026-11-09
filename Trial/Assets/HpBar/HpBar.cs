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

    [Header("턴 제한시간 표시 (모니터 위 작은 화면)")]
    public TMPro.TMP_Text turnTimerText;
    public Color timerNormalColor = new Color(0.4f, 1f, 0.4f);
    public Color timerWarningColor = Color.red;
    public int timerWarningSeconds = 5;
    private int _shownSeconds = int.MinValue;

    public void Start()
    {
        if (turnTimerText == null)
        {
            Transform t = transform.Find("TurnTimerText");
            if (t != null) turnTimerText = t.GetComponent<TMPro.TMP_Text>();
        }
        SetTimer(-1);

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


    // 남은 초를 표시. 음수면 숨김
    public void SetTimer(int seconds)
    {
        if (turnTimerText == null || seconds == _shownSeconds) return;
        _shownSeconds = seconds;

        if (seconds < 0)
        {
            turnTimerText.text = "";
            return;
        }

        turnTimerText.text = seconds.ToString();
        turnTimerText.color = seconds <= timerWarningSeconds ? timerWarningColor : timerNormalColor;
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
