using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance;

    public TMP_Dropdown resDropdown;
    private List<Resolution> filteredResolutions; // 필터링된 실제 해상도 데이터

    

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitResolutionDropdown();
    }

    void Start()
    {
        SetWindowMode(SettingManager.Instance.currentSettings.isFullScreen);
    }

    public void InitResolutionDropdown()
    {
        // 1. 시스템에서 지원하는 모든 해상도 가져오기
        Resolution[] allResolutions = Screen.resolutions;
        
        resDropdown.ClearOptions();
        filteredResolutions = new List<Resolution>();
        
        List<string> options = new List<string>();
        HashSet<string> checkDuplicates = new HashSet<string>(); // 중복 문자열 체크용

        int currentResIndex = 0;

        // 2. 루프를 돌며 중복 제거 및 문자열 생성
        for (int i = 0; i < allResolutions.Length; i++)
        {
            // 주사율 반올림 (예: 143.99 -> 144)
            int refreshRate = Mathf.RoundToInt((float)allResolutions[i].refreshRateRatio.value);
            
            // 드롭다운에 표시될 텍스트 형식
            string optionText = allResolutions[i].width + " x " + allResolutions[i].height + " @ " + refreshRate + "Hz";

            // 중복되지 않은 해상도만 리스트에 추가
            if (checkDuplicates.Add(optionText))
            {
                filteredResolutions.Add(allResolutions[i]);
                options.Add(optionText);

                // 현재 모니터 설정과 일치하는 항목 기억
                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height &&
                    Mathf.Abs((float)allResolutions[i].refreshRateRatio.value - (float)Screen.currentResolution.refreshRateRatio.value) < 0.1f)
                {
                    currentResIndex = filteredResolutions.Count - 1;
                }
            }
        }

        // 3. 드롭다운 적용
        resDropdown.AddOptions(options);
        resDropdown.value = currentResIndex;
        resDropdown.RefreshShownValue();
    }

    // 드롭다운 OnValueChanged에 연결할 함수
    public void SetResolution(int index)
    {
        if (index < 0 || index >= filteredResolutions.Count) return;

        Resolution res = filteredResolutions[index];
        
        // 실제 해상도 변경
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);

        // SettingManager 데이터 동기화
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.currentSettings.resolutionIndex = index;
        }
    }

    public void SetWindowMode(int index)
    {
        // 에셋에서 넘겨주는 index: 0(전체화면), 1(전체창), 2(창모드)라고 가정
        FullScreenMode mode;

        switch (index)
        {
            case 0:
                mode = FullScreenMode.ExclusiveFullScreen; // 순수 전체화면
                break;
            case 1:
                mode = FullScreenMode.FullScreenWindow;    // 테두리 없는 창
                break;
            case 2:
                mode = FullScreenMode.Windowed;            // 일반 창모드
                break;
            default:
                mode = FullScreenMode.FullScreenWindow;
                break;
        }

        Screen.fullScreenMode = mode;

        // 설정값 저장
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.currentSettings.isFullScreen = index;
        }
        
        Debug.Log($"화면 모드 변경: {mode} (Index: {index})");
    }
}