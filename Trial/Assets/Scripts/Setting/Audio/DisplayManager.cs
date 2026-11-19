// DisplayManager는 해상도와 화면 모드를 실제로 적용하고, 해상도 드롭다운/화면 모드 선택기를 연결하는 클래스입니다.
// 씬이 바뀌어도 살아 있는 이 인스턴스에 코드로 연결하므로, 로비에 다시 돌아와도 설정이 정상 동작합니다.
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Michsky.UI.Dark;

public class DisplayManager : MonoBehaviour
{
    public static DisplayManager Instance;

    public TMP_Dropdown resDropdown;
    private List<Resolution> filteredResolutions; // 필터링된 실제 해상도 데이터

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 로비로 돌아왔을 때 생기는 복사본: 아무것도 건드리지 않고 삭제
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildResolutionList();
    }

    void Start()
    {
        // 게임 시작 시 저장된 화면 모드와 해상도를 실제로 적용
        ApplySavedDisplay();
    }

    #region [해상도 목록]
    private void BuildResolutionList()
    {
        filteredResolutions = new List<Resolution>();
        var seen = new HashSet<string>();
        foreach (var r in Screen.resolutions)
        {
            if (seen.Add(ToLabel(r)))
                filteredResolutions.Add(r);
        }
    }

    private static int Hz(Resolution r) => Mathf.RoundToInt((float)r.refreshRateRatio.value);
    private static string ToLabel(Resolution r) => r.width + " x " + r.height + " @ " + Hz(r) + "Hz";

    private List<string> BuildOptions()
    {
        var options = new List<string>();
        foreach (var r in filteredResolutions) options.Add(ToLabel(r));
        return options;
    }

    // 저장된 해상도(가로/세로/주사율)에 해당하는 목록 순번. 없으면 현재 모니터 해상도
    private int FindSavedIndex()
    {
        var s = Settings;
        if (s != null && s.resolutionWidth > 0)
        {
            int best = -1;
            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                var r = filteredResolutions[i];
                if (r.width != s.resolutionWidth || r.height != s.resolutionHeight) continue;
                if (Hz(r) == s.refreshRate) return i;
                best = i; // 같은 크기, 다른 주사율이면 후보로
            }
            if (best >= 0) return best;
        }
        return FindCurrentIndex();
    }

    private int FindCurrentIndex()
    {
        var cur = Screen.currentResolution;
        int fallback = filteredResolutions.Count - 1;
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            var r = filteredResolutions[i];
            if (r.width == cur.width && r.height == cur.height)
            {
                if (Mathf.Abs((float)(r.refreshRateRatio.value - cur.refreshRateRatio.value)) < 0.1f) return i;
                fallback = i;
            }
        }
        return Mathf.Max(0, fallback);
    }
    #endregion

    private static GameSettings Settings =>
        SettingManager.Instance != null ? SettingManager.Instance.currentSettings : null;

    #region [UI 연결]
    /// <summary>
    /// 해상도 드롭다운을 이 매니저에 연결 (로비/인게임 공통).
    /// 인스펙터에 연결된 예전 이벤트는 끄고(사라진 오브젝트를 가리킬 수 있음) 코드로 다시 연결합니다.
    /// </summary>
    public void BindDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null) return;
        if (filteredResolutions == null || filteredResolutions.Count == 0) BuildResolutionList();

        DisablePersistent(dropdown.onValueChanged);
        dropdown.onValueChanged.RemoveListener(SetResolution); // 우리 리스너만 제거 (UI 에셋이 붙인 리스너는 유지)

        dropdown.ClearOptions();
        dropdown.AddOptions(BuildOptions());
        dropdown.SetValueWithoutNotify(FindSavedIndex()); // 값만 바꾸고 이벤트는 발생시키지 않음
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(SetResolution);
    }

    /// <summary>화면 모드 선택기(전체화면/테두리 없는 창/창모드)를 이 매니저에 연결</summary>
    public void BindWindowModeSelector(HorizontalSelector selector)
    {
        if (selector == null) return;

        DisablePersistent(selector.onValueChanged);
        selector.onValueChanged.RemoveListener(SetWindowMode); // 우리 리스너만 제거 (UI 에셋이 붙인 리스너는 유지)

        var s = Settings;
        if (s != null)
        {
            selector.index = Mathf.Clamp(s.isFullScreen, 0, Mathf.Max(0, selector.itemList.Count - 1));
            try { selector.UpdateUI(); } catch { /* 선택기가 아직 초기화 전이면 다음에 열 때 반영 */ }
        }

        selector.onValueChanged.AddListener(SetWindowMode);
    }

    private static void DisablePersistent(UnityEngine.Events.UnityEventBase ev)
    {
        for (int i = 0; i < ev.GetPersistentEventCount(); i++)
            ev.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
    }
    #endregion

    #region [적용]
    // 드롭다운에서 해상도를 골랐을 때
    public void SetResolution(int index)
    {
        if (filteredResolutions == null || index < 0 || index >= filteredResolutions.Count) return;

        Resolution res = filteredResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);

        var s = Settings;
        if (s != null)
        {
            s.resolutionIndex = index;
            s.resolutionWidth = res.width;
            s.resolutionHeight = res.height;
            s.refreshRate = Hz(res);
            SettingManager.Instance.MarkDirty(); // 자동 저장
        }
        Debug.Log($"해상도 변경: {ToLabel(res)}");
    }

    public void SetWindowMode(int index)
    {
        Screen.fullScreenMode = ToMode(index);

        var s = Settings;
        if (s != null)
        {
            s.isFullScreen = index;
            SettingManager.Instance.MarkDirty(); // 자동 저장
        }
        Debug.Log($"화면 모드 변경: {Screen.fullScreenMode} (Index: {index})");
    }

    // 에셋 선택기 index: 0(전체화면), 1(테두리 없는 창), 2(창모드)
    private static FullScreenMode ToMode(int index)
    {
        switch (index)
        {
            case 0: return FullScreenMode.ExclusiveFullScreen;
            case 1: return FullScreenMode.FullScreenWindow;
            case 2: return FullScreenMode.Windowed;
            default: return FullScreenMode.FullScreenWindow;
        }
    }

    // 저장된 화면 모드 + 해상도를 한 번에 적용 (게임 시작 시)
    public void ApplySavedDisplay()
    {
        var s = Settings;
        if (s == null || filteredResolutions == null || filteredResolutions.Count == 0) return;

        int index = FindSavedIndex();
        Resolution res = filteredResolutions[index];
        Screen.SetResolution(res.width, res.height, ToMode(s.isFullScreen), res.refreshRateRatio);

        // 처음 실행이라 저장값이 없으면 지금 해상도를 기록해 둠
        if (s.resolutionWidth <= 0)
        {
            s.resolutionIndex = index;
            s.resolutionWidth = res.width;
            s.resolutionHeight = res.height;
            s.refreshRate = Hz(res);
        }
        Debug.Log($"저장된 화면 설정 적용: {ToLabel(res)}, {ToMode(s.isFullScreen)}");
    }
    #endregion
}
