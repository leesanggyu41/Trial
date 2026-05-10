using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class RadioTextEffect : MonoBehaviour
{
    public static RadioTextEffect Instance;

    [Header("텍스트 설정")]
    public TMP_Text radioText;
    public float typingSpeed = 0.05f;
    public float glitchChance = 0.3f;
    public float glitchDuration = 0.05f;
    public float displayDuration = 3f; // 다 나온 후 유지 시간

    private const string glitchChars = "!@#$%^&*[]{}|<>?/\\~`0123456789";

    private void Awake()
    {
        Instance = this;
        radioText.color = new Color(0f, 1f, 0f); // 녹색
        gameObject.SetActive(false); // 시작할 때 숨김
    }

    public void ShowText(string message)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        radioText.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            // 지지직 효과
            if (Random.value < glitchChance)
                yield return StartCoroutine(GlitchEffect(message, i));

            // 글자 추가
            radioText.text += message[i];
            radioText.DOFade(1f, typingSpeed).From(0.3f);

            yield return new WaitForSeconds(typingSpeed);
        }

        // 다 나온 후 깜빡임
        yield return StartCoroutine(BlinkEffect());

        // 유지 후 사라짐
        yield return new WaitForSeconds(displayDuration);
        radioText.DOFade(0f, 0.5f).OnComplete(() => gameObject.SetActive(false));
    }

    private IEnumerator GlitchEffect(string message, int currentIndex)
    {
        string originalText = message.Substring(0, currentIndex);

        for (int g = 0; g < 3; g++)
        {
            string glitch = "";
            int glitchCount = Random.Range(1, 4);
            for (int j = 0; j < glitchCount; j++)
                glitch += glitchChars[Random.Range(0, glitchChars.Length)];

            radioText.text = originalText + $"<color=#00FF00>{glitch}</color>";
            yield return new WaitForSeconds(glitchDuration);
        }

        radioText.text = originalText;
    }

    private IEnumerator BlinkEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            radioText.DOFade(0.2f, 0.1f);
            yield return new WaitForSeconds(0.1f);
            radioText.DOFade(1f, 0.1f);
            yield return new WaitForSeconds(0.1f);
        }
    }
}