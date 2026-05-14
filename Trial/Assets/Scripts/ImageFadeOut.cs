using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageFadeOut : MonoBehaviour
{
    [Header("설정")]
    public Image targetImage;          // 페이드할 Image 컴포넌트
    public float fadeDuration = 1f;    // 페이드 아웃 걸리는 시간 (초)
    public float delayBeforeFade = 0f; // 시작 전 대기 시간 (초)

    void Start()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
            StartCoroutine(FadeOut());
    }

   

    private IEnumerator FadeOut()
    {
        // 시작 전 대기
        if (delayBeforeFade > 0f)
            yield return new WaitForSeconds(delayBeforeFade);

        float elapsed = 0f;
        Color color = targetImage.color;
        float startAlpha = color.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            color.a = Mathf.Lerp(startAlpha, 0f, t);
            targetImage.color = color;

            yield return null;
        }

        // 알파값 완전히 0으로 보정
        color.a = 0f;
        targetImage.color = color;

        // 마지막에 비활성화
        gameObject.SetActive(false);
    }
}