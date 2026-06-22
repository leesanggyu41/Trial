using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Dark;
using UnityEngine.InputSystem;

public class InGameSettingUI : MonoBehaviour
{


    public GameObject settingPanal;
    [Header("Audio UI")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Control UI")]
    public Slider mouseSensitivitySlider;

    [Header("Video UI")]
    public Slider gammaSlider;
    public Slider motionBlurSlider;
    public TMP_Dropdown resolutionDropdown;
    public HorizontalSelector fullScreenmode;

    private void OnEnable()
    {
        // 설정창이 열릴 때 현재 설정값을 UI에 반영
        ApplySettingsToUI();
    }
    void Update()
    {
        // Keyboard.current가 null이 아닐 때, escapeKey가 이번 프레임에 눌렸는지 확인
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TriggerMenu();
        }
    }

    public void TriggerMenu()
    {
        settingPanal.SetActive(!settingPanal.activeSelf);
    }

    private void ApplySettingsToUI()
    {
        if (SettingManager.Instance == null) return;
        var settings = SettingManager.Instance.currentSettings;

        masterVolumeSlider.value = settings.masterVolume;
        musicVolumeSlider.value = settings.musicVolume;
        sfxVolumeSlider.value = settings.sfxVolume;
        mouseSensitivitySlider.value = settings.mouseSensitivity;
        gammaSlider.value = settings.gamma;
        motionBlurSlider.value = settings.motionBlur;
        resolutionDropdown.value = settings.resolutionIndex;
        fullScreenmode.index = settings.isFullScreen;
    }

    // 인게임 UI 슬라이더에 연결할 함수들
    public void OnMasterVolumeChanged(float value) => SettingManager.Instance.UpdateMasterVolume(value);
    public void OnMusicVolumeChanged(float value) => SettingManager.Instance.UpdateMusicVolume(value);
    public void OnSFXVolumeChanged(float value) => SettingManager.Instance.UpdateSFXVolume(value);
    public void OnSensitivityChanged(float value) => SettingManager.Instance.UpdateSensitivity(value);
    public void OnGammaChanged(float value) => SettingManager.Instance.UpdateGamma(value);
    public void OnMotionBlurChanged(float value) => SettingManager.Instance.UpdateMotionBlur(value);
    public void OnResolutionChanged(int index) => SettingManager.Instance.UpdateResolution(index);
    public void OnFullScreenChanged(int value) => SettingManager.Instance.UpdateFullScreen(value);
    public void OnSaveButton() => SettingManager.Instance.SaveAll();
}