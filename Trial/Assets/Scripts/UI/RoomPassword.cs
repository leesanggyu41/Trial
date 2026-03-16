// RoomPassword는 방 생성 UI에서 비밀번호 설정과 관련된 UI 요소들을 관리하는 클래스입니다.
// 사용자가 비밀번호 사용 여부를 토글하면, 해당 상태에 따라 비밀번호 입력 칸과 그 배경의 색상을 변경하여 시각적으로 활성화/비활성화 상태를 구분할 수 있도록 처리합니다.
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // 부드러운 색상 전환을 위해 사용

public class RoomPassword : MonoBehaviour
{
    [Header("UI 연결")]
    public Toggle passwordToggle;         // 비밀번호 사용 체크박스
    public TMP_InputField passwordInput;   // 비밀번호 입력칸
    public Image inputFieldBackground;    // 입력칸의 배경 이미지 (어둡게 만들 대상)
    public TextMeshProUGUI inputText;     // 입력칸 안의 텍스트 (함께 어둡게)

    [Header("비활성화 설정")]
    public Color enabledColor = Color.white;      // 활성화 상태 (밝음)
    public Color disabledColor = new Color(0.2f, 0.2f, 0.2f, 1f); // 비활성화 상태 (아주 어두움)
    public float fadeDuration = 0.3f;             // 색이 변하는 시간

    private void Start()
    {
        // 초기 상태 설정 (현재 토글 값에 따라 즉시 적용)
        RefreshPasswordUI(passwordToggle.isOn, 0f);

        // 토글 값이 바뀔 때마다 함수 호출
        passwordToggle.onValueChanged.AddListener((isOn) => {
            RefreshPasswordUI(isOn, fadeDuration);
        });
    }

    // 비밀번호 UI의 상태를 새로고침하는 함수
    private void RefreshPasswordUI(bool isEnabled, float duration)
    {
        // 1. 실제 클릭 가능 여부 설정
        passwordInput.interactable = isEnabled;

        // 2. 색상 결정
        Color targetColor = isEnabled ? enabledColor : disabledColor;

        // 3. DOTween을 이용한 부드러운 색상 전환
        // 배경 이미지 색상 변경
        if (inputFieldBackground != null)
            inputFieldBackground.DOColor(targetColor, duration);
            
        // 팁: 비활성화 시 텍스트 초기화 (선택 사항)
        if (!isEnabled) passwordInput.text = "";
    }
}
