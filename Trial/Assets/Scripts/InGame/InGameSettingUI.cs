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
        // 설정창이 열릴 때 저장된 값을 UI에 반영 (값 설정으로 이벤트가 발생해 설정이 덮어써지지 않도록 이벤트 없이 반영)
        ApplySettingsToUI();
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
            ApplySettingsToUI(); // 열 때마다 최신 값으로
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            SettingManager.Instance?.SaveAll(); // 닫을 때 저장

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
        var sm = SettingManager.Instance;
        if (sm == null || sm.currentSettings == null) return;
        var s = sm.currentSettings;

        SettingManager.BindSlider(masterVolumeSlider, s.masterVolume, sm.UpdateMasterVolume);
        SettingManager.BindSlider(musicVolumeSlider, s.musicVolume, sm.UpdateMusicVolume);
        SettingManager.BindSlider(sfxVolumeSlider, s.sfxVolume, sm.UpdateSFXVolume);
        SettingManager.BindSlider(mouseSensitivitySlider, s.mouseSensitivity, sm.UpdateSensitivity);
        SettingManager.BindSlider(gammaSlider, s.gamma, sm.UpdateGamma);
        SettingManager.BindSlider(motionBlurSlider, s.motionBlur, sm.UpdateMotionBlur);

        // 해상도 목록을 먼저 채운 뒤 저장된 해상도를 선택 (예전에는 목록보다 값을 먼저 넣어 해상도가 초기화됐음)
        if (DisplayManager.Instance != null)
        {
            DisplayManager.Instance.BindDropdown(resolutionDropdown);
            DisplayManager.Instance.BindWindowModeSelector(fullScreenmode);
        }
    }

    // 인게임 UI 슬라이더에 연결할 함수들 (인스펙터 호환용으로 유지)
    public void OnMasterVolumeChanged(float value) => SettingManager.Instance.UpdateMasterVolume(value);
    public void OnMusicVolumeChanged(float value) => SettingManager.Instance.UpdateMusicVolume(value);
    public void OnSFXVolumeChanged(float value) => SettingManager.Instance.UpdateSFXVolume(value);
    public void OnSensitivityChanged(float value) => SettingManager.Instance.UpdateSensitivity(value);
    public void OnGammaChanged(float value) => SettingManager.Instance.UpdateGamma(value);
    public void OnMotionBlurChanged(float value) => SettingManager.Instance.UpdateMotionBlur(value);
    public void OnResolutionChanged(int index) => SettingManager.Instance.UpdateResolution(index);
    public void OnFullScreenChanged(int value) => SettingManager.Instance.UpdateFullScreen(value);

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
