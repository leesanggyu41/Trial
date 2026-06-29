using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Dark;
using UnityEngine.InputSystem;
using System.Collections;
public class InGameSettingUI : MonoBehaviour
{


    public GameObject settingPanal;

    public GameObject settingButton; // 설정 버튼
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
        if (DisplayManager.Instance != null && resolutionDropdown != null)
        {
            DisplayManager.Instance.SetupDropdown(resolutionDropdown);
        }

    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TriggerMenu();
        }
        // 커서 관련 코드 전부 제거
    }

    public void TriggerMenu()
    {
        bool isOpen = !settingPanal.activeSelf;
        settingPanal.SetActive(isOpen);

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 탑뷰나 스펙테이터면 커서 유지
            bool isTopView = PlayerControll.Local != null && PlayerControll.Local.isTopView;
            bool isSpectating = SpectatorManager.Instance != null && SpectatorManager.Instance.IsSpectating;

            if (!isTopView && !isSpectating)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
    }

    void ApplySettingsToUI()
    {
        if (SettingManager.Instance == null) return;
        var currentSettings = SettingManager.Instance.currentSettings;
        if (currentSettings == null) return;

        if (masterVolumeSlider != null) masterVolumeSlider.value = currentSettings.masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = currentSettings.musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = currentSettings.sfxVolume;
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = currentSettings.mouseSensitivity;
        if (resolutionDropdown != null) resolutionDropdown.value = currentSettings.resolutionIndex;
        if (gammaSlider != null) gammaSlider.value = currentSettings.gamma;
        if (motionBlurSlider != null) motionBlurSlider.value = currentSettings.motionBlur;
        if (fullScreenmode != null) fullScreenmode.index = currentSettings.isFullScreen;
    }

    // 인게임 UI 슬라이더에 연결할 함수들
    public void OnMasterVolumeChanged(float value) => SettingManager.Instance.UpdateMasterVolume(value);
    public void OnMusicVolumeChanged(float value) => SettingManager.Instance.UpdateMusicVolume(value);
    public void OnSFXVolumeChanged(float value) => SettingManager.Instance.UpdateSFXVolume(value);
    public void OnSensitivityChanged(float value) => SettingManager.Instance.UpdateSensitivity(value);
    public void OnGammaChanged(float value) => SettingManager.Instance.UpdateGamma(value);
    public void OnMotionBlurChanged(float value) => SettingManager.Instance.UpdateMotionBlur(value);
    public void OnResolutionChanged(int index)
    {
        // 1. 데이터 저장 (기존 코드)
        SettingManager.Instance.UpdateResolution(index);

        // 2. [추가] 인스펙터 연결 대신 싱글톤으로 직접 화면 변경 호출!
        if (DisplayManager.Instance != null)
        {
            DisplayManager.Instance.SetResolution(index);
        }
        else
        {
            Debug.LogError("VideoManager(DisplayManager) 인스턴스를 찾을 수 없습니다!");
        }
    }
    public void OnFullScreenChanged(int value)
    {
        // 1. 데이터 저장 (기존 코드)
        SettingManager.Instance.UpdateFullScreen(value);

        // 2. [추가] 인스펙터 연결 대신 싱글톤으로 직접 화면 모드 변경 호출!
        if (DisplayManager.Instance != null)
        {
            DisplayManager.Instance.SetWindowMode(value);
        }
    }
    public void OnSaveButton()
    {

        SettingManager.Instance.SaveAll();
        if (settingButton != null)
        {
            settingButton.SetActive(false);
        }
    }

    public void OnIngameQuitButton()
    {
        PlayerControll local = PlayerControll.Local;
        if (local != null)
            local.OnQuitButton();

        // 약간 딜레이 후 나가기 (RPC 전송 보장)
        StartCoroutine(LeaveAfterDelay());
    }

    private IEnumerator LeaveAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        ServerConnectionManager.Instance.LeaveRoom();
    }
}