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
    public GameObject resultUI;
    public TMP_Text winnerNameText;
    public TMP_Text[] deadNameTexts;           // "탈출"

    [Header("타이밍")]
    public float cameraAnimationDuration = 22.45f;
    public float waitAfterResult = 5f;      // 결과 표시 후 메인화면까지 대기 시간

    private void Start()
    {
        string winnerName = "???";
        string deadNamesRaw = "";

        if (GameResultData.Instance != null)
        {
            winnerName = GameResultData.Instance.WinnerName;
            deadNamesRaw = GameResultData.Instance.DeadNamesJoined;
        }

        Debug.Log($"[EndScene] winnerName: '{winnerName}'");

        resultUI.SetActive(false);
        fadeCanvasGroup.alpha = 0f;

        StartCoroutine(EndSequence(winnerName, deadNamesRaw));
    }

    private IEnumerator EndSequence(string winnerName, string deadNamesRaw)
    {
        yield return new WaitForSeconds(cameraAnimationDuration);

        yield return StartCoroutine(Fade(0f, 1f));

        winnerNameText.text = $"{winnerName}";

        string[] deadNames = deadNamesRaw.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

        // 죽은 사람 수만큼 활성화하고 텍스트 설정
        for (int i = 0; i < deadNameTexts.Length; i++)
        {
            if (i < deadNames.Length)
            {
                deadNameTexts[i].gameObject.SetActive(true);
                deadNameTexts[i].text = $"{deadNames[i]}";
            }
            else
            {
                deadNameTexts[i].gameObject.SetActive(false); // 남는 슬롯은 비활성화
            }
        }

        resultUI.SetActive(true);

        yield return new WaitForSeconds(waitAfterResult);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (ServerConnectionManager.Instance != null)
            ServerConnectionManager.Instance.ReturnToLobby();
        else
            SceneManager.LoadScene(1);
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