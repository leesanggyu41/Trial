// ModeSelectManager는 Host Room을 눌렀을 때 먼저 나오는 모드 선택 화면을 관리합니다.
// [멀티] → 기존 방 생성 패널(Host Setting) 열기 / [AI 대전] → 방 없이 바로 AI와 1:1 대전 시작
using UnityEngine;

public class ModeSelectManager : MonoBehaviour
{
    [Header("패널")]
    public GameObject modeSelectPanel;   // 이 모드 선택 화면
    public GameObject hostSettingPanel;  // 기존 방 생성 패널

    [Header("다른 로비 UI보다 위에 그리기")]
    public int sortingOrder = 10;

    private bool _starting = false;

    private void OnEnable()
    {
        _starting = false;

        // 비활성 상태에서 저장된 중첩 Canvas의 overrideSorting은 유지되지 않으므로 켜질 때 적용
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }
    }

    public void OnClickMulti()
    {
        if (_starting) return;
        if (modeSelectPanel != null) modeSelectPanel.SetActive(false);
        if (hostSettingPanel != null) hostSettingPanel.SetActive(true);
    }

    public async void OnClickBot()
    {
        if (_starting) return; // 중복 클릭 방지
        _starting = true;
        // 접속 대기 패널(waitpanal)이 보이도록 모드 선택 화면은 먼저 닫음
        if (modeSelectPanel != null) modeSelectPanel.SetActive(false);
        await ServerConnectionManager.Instance.StartBotMatch();
        _starting = false;
    }

    public void OnClickClose()
    {
        if (_starting) return;
        if (modeSelectPanel != null) modeSelectPanel.SetActive(false);
    }
}
