using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Dark;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing; // TMP_Dropdown 사용을 위해 필요

public class SettingManager : MonoBehaviour
{

    public static SettingManager Instance;
    [Header("Data")]
    public GameSettings currentSettings;

    [Header("Audio UI")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Control UI")]
    public Slider mouseSensitivitySlider;

    [Header("Video UI")]
    public TMP_Dropdown resolutionDropdown;
    public Slider gammaSlider;
    public Slider motionBlurSlider;
    public HorizontalSelector fullScreenmode; 

    [Header("Setting panal")]
    public GameObject settingPanal;

     void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // 1. 데이터 로드
        currentSettings = SaveSystem.LoadSettings();
        
        // 2. UI에 데이터 적용
        ApplySettingsToUI();
    }

    // --- [UI에서 호출할 public 함수들] ---

public void UpdateMasterVolume(float value)
{
    currentSettings.masterVolume = value;
    AudioManager.Instance.SetVolume("Master", value);
}

public void UpdateMusicVolume(float value)
{
    currentSettings.musicVolume = value;
    AudioManager.Instance.SetVolume("Music", value);
}

public void UpdateSFXVolume(float value)
{
    currentSettings.sfxVolume = value;
    AudioManager.Instance.SetVolume("SFX", value);
}    
    public void UpdateSensitivity(float value)
    {
        currentSettings.mouseSensitivity = value;
        IngameSettingManager.Instance.ApplySettings();
    }
    public void UpdateGamma(float value)
    {
        currentSettings.gamma = value;
        IngameSettingManager.Instance.ApplySettings();
    }
    public void UpdateMotionBlur(float value)
    {
        currentSettings.motionBlur = value;
        IngameSettingManager.Instance.ApplySettings();
    }
    public void UpdateResolution(int index) 
    {
        currentSettings.resolutionIndex = index;
        Debug.Log("실제 저장된 값: " + currentSettings.resolutionIndex);
        
    } 
    public void UpdateFullScreen(int isFull) => currentSettings.isFullScreen = isFull;

    // 최종 저장 버튼이나 설정창을 닫을 때 호출
    public void SaveAll()
    {
        SaveSystem.SaveSettings(currentSettings);
        Debug.Log("설정이 JSON 파일로 저장되었습니다!");
        settingPanal.SetActive(false); // 설정창 닫기
    }

    void ApplySettingsToUI()
    {
        if (currentSettings == null) return;

        // 저장된 값을 UI 요소에 전달
        masterVolumeSlider.value = currentSettings.masterVolume;
        musicVolumeSlider.value = currentSettings.musicVolume;
        sfxVolumeSlider.value = currentSettings.sfxVolume;
        mouseSensitivitySlider.value = currentSettings.mouseSensitivity;
        
        resolutionDropdown.value = currentSettings.resolutionIndex;
        gammaSlider.value = currentSettings.gamma;
        motionBlurSlider.value = currentSettings.motionBlur;
        
        fullScreenmode.index = currentSettings.isFullScreen;
    }
}