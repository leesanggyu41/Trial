
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class DeathUIManager : MonoBehaviour
{
    public static DeathUIManager Instance;
    public GameObject deathPanel; // UI Panel ("You Died" 등)
    public TMP_Text deathMessage; // 중앙 메시지 TMP 텍스트
    public AudioSource deathAudio; // 사망 효과음
    public GameObject blurEffect; // 블러 효과 오브젝트

    private bool isShowing = false;
    private float fadeDuration = 1.2f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // HP 비율에 따라 효과 조절 (0~1)
    // hpRatio: 1 = 최대 HP, 0 = 죽음
    public void SetLowHpEffect(float hpRatio)
    {
        // 예시: HP가 낮을수록 블러/붉은 이미지 투명도, 사운드 볼륨 증가
        float effectStrength = 1f - Mathf.Clamp01(hpRatio); // 0(최대HP)~1(0HP)

        // 이미지 투명도 조절 (예: 붉은 vignette 등)
        if (blurEffect != null)
        {
            var img = blurEffect.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                // 최대 투명도 0.7 예시
                c.a = Mathf.Lerp(0f, 0.7f, effectStrength);
                img.color = c;
            }
        }

        // 사운드 볼륨 조절 (예: 심장박동 등)
        if (deathAudio != null)
        {
            // 최대 볼륨 0.8 예시
            deathAudio.volume = Mathf.Lerp(0f, 0.8f, effectStrength);
        }
    }

    public void ShowDeathUI(string message = "You Died", float delay = 2.0f)
    {
        if (isShowing) return;
        isShowing = true;
        StartCoroutine(ShowDeathUIRoutine(message, delay));
    }

    private IEnumerator ShowDeathUIRoutine(string message, float delay)
    {
        // 연출: 블러, 사운드 등 먼저 실행
        if (blurEffect != null)
            blurEffect.SetActive(true);
        if (deathAudio != null)
            deathAudio.Play();

        // UI는 아직 비활성화
        if (deathPanel != null)
            deathPanel.SetActive(false);
        if (deathMessage != null)
        {
            deathMessage.text = message;
            var color = deathMessage.color;
            color.a = 0f;
            deathMessage.color = color;
        }

        // 연출 후 대기
        yield return new WaitForSecondsRealtime(delay);

        // UI 활성화 및 텍스트 페이드 인
        if (deathPanel != null)
            deathPanel.SetActive(true);
        if (deathMessage != null)
            StartCoroutine(FadeInTMPText(deathMessage, fadeDuration));

        // 입력 제한 (선택)
        if (EventSystem.current != null)
            EventSystem.current.sendNavigationEvents = false;

        yield return new WaitForSecondsRealtime(2f);
        HideDeathUI();
        SpectatorManager.Instance?.StartSpectating();
    }


    private IEnumerator FadeInTMPText(TMP_Text tmpText, float duration)
    {
        float t = 0;
        Color color = tmpText.color;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(0, 1, t / duration);
            tmpText.color = color;
            yield return null;
        }
        color.a = 1f;
        tmpText.color = color;
    }

    public void HideDeathUI()
    {
        if (blurEffect != null)
            blurEffect.SetActive(false);
        if (deathPanel != null)
            deathPanel.SetActive(false);
        if (deathMessage != null)
        {
            var color = deathMessage.color;
            color.a = 0f;
            deathMessage.color = color;
        }
        isShowing = false;
        // 입력 해제 (선택)
        if (EventSystem.current != null)
            EventSystem.current.sendNavigationEvents = true;
    }
}
