using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndSceneManager : MonoBehaviour
{
    [Header("카메라 애니메이션")]
    public Animator cameraAnimator;
    public string animationTrigger = "Play"; // 애니메이터 트리거 이름

    [Header("암전 UI")]
    public CanvasGroup fadeCanvasGroup;      // 검은 화면 Image의 CanvasGroup
    public float fadeDuration = 1.5f;

    [Header("결과 UI")]
    public GameObject resultUI;             // 닉네임 + "탈출" 문구 UI 루트
    public TMP_Text winnerNameText;         // "홍길동"
    public TMP_Text resultText;             // "탈출"

    [Header("타이밍")]
    public float cameraAnimationDuration = 22.45f;
    public float waitAfterResult = 5f;      // 결과 표시 후 메인화면까지 대기 시간

    private void Start()
    {
        string winnerName = PlayerPrefs.GetString("WinnerName", "???");
        PlayerPrefs.DeleteKey("WinnerName"); // 사용 후 삭제

        resultUI.SetActive(false);
        fadeCanvasGroup.alpha = 0f;

        StartCoroutine(EndSequence(winnerName));
    }

    private IEnumerator EndSequence(string winnerName)
    {
        // 1. 애니메이션이 자동 재생되므로 한 프레임 대기 후 길이 읽기
        yield return new WaitForSeconds(cameraAnimationDuration);

        float animLength = 0f;
        if (cameraAnimator != null)
        {
            AnimatorStateInfo stateInfo = cameraAnimator.GetCurrentAnimatorStateInfo(0);
            animLength = stateInfo.length;
            yield return new WaitForSeconds(animLength);
        }

        // 2. 암전 페이드 인
        yield return StartCoroutine(Fade(0f, 1f));

        // 3. 결과 UI 표시
        winnerNameText.text = winnerName;
        resultText.text = "탈출";
        resultUI.SetActive(true);

        // 4. 5초 대기 후 메인화면
        yield return new WaitForSeconds(waitAfterResult);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (ServerConnectionManager.Instance != null)
            ServerConnectionManager.Instance.ReturnToLobby();
        else
            SceneManager.LoadScene(1); // 혹시 Instance가 없을 경우 대비
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        fadeCanvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
    }
}