using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Dark;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing; // TMP_Dropdown 사용을 위해 필요
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{

    public static SettingManager Instance;
    public Button cancleButton; // 닫기 버튼
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
            SceneManager.sceneLoaded += OnSceneLoaded;
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
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀔 때마다 UI 다시 찾아서 연결
        if (scene.name == "LobbyScene")
            FindAndApplyUI();
    }
    private void FindAndApplyUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // 태그로 찾기
        masterVolumeSlider = GameObject.FindGameObjectWithTag("MasterVolumeSlider")?.GetComponent<Slider>();
        musicVolumeSlider = GameObject.FindGameObjectWithTag("MusicVolumeSlider")?.GetComponent<Slider>();
        sfxVolumeSlider = GameObject.FindGameObjectWithTag("SFXVolumeSlider")?.GetComponent<Slider>();
        mouseSensitivitySlider = GameObject.FindGameObjectWithTag("SensitivitySlider")?.GetComponent<Slider>();
        resolutionDropdown = GameObject.FindGameObjectWithTag("ResolutionDropdown")?.GetComponent<TMP_Dropdown>();
        gammaSlider = GameObject.FindGameObjectWithTag("GammaSlider")?.GetComponent<Slider>();
        motionBlurSlider = GameObject.FindGameObjectWithTag("MotionBlurSlider")?.GetComponent<Slider>();
        fullScreenmode = GameObject.FindGameObjectWithTag("FullScreenSelector")?.GetComponent<HorizontalSelector>();
        settingPanal = GameObject.FindGameObjectWithTag("SettingPanel");
        cancleButton = GameObject.FindGameObjectWithTag("CancelButton")?.GetComponent<Button>();

        ApplySettingsToUI();

        
        masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSFXVolume);
        mouseSensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        gammaSlider.onValueChanged.AddListener(UpdateGamma);
        motionBlurSlider.onValueChanged.AddListener(UpdateMotionBlur);
        resolutionDropdown.onValueChanged.AddListener(UpdateResolution);
        cancleButton.onClick.AddListener(SaveAll);
        settingPanal.SetActive(false);

    }

    // --- [UI에서 호출할 public 함수들] ---


    public void UpdateMasterVolume(float value)
    {
        currentSettings.masterVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume("Master", value);
    }

    public void UpdateMusicVolume(float value)
    {
        currentSettings.musicVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume("Music", value);
    }

    public void UpdateSFXVolume(float value)
    {
        currentSettings.sfxVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume("SFX", value);
    }
    public void UpdateSensitivity(float value)
    {
        currentSettings.mouseSensitivity = value;
        if (PlayerControll.Local != null)
            PlayerControll.Local.mouseSensitivity = value;
        IngameSettingManager.Instance?.ApplySettings();
    }
    public void UpdateGamma(float value)
    {
        currentSettings.gamma = value;
        IngameSettingManager.Instance?.ApplySettings();
    }
    public void UpdateMotionBlur(float value)
    {
        currentSettings.motionBlur = value;
        IngameSettingManager.Instance?.ApplySettings();
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
        if (settingPanal != null)
            settingPanal.SetActive(false);
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