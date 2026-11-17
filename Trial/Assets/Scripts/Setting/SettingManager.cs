// SettingManager는 설정 값(GameSettings)을 보관·저장하고, 로비 설정 UI를 연결하는 클래스입니다. (씬이 바뀌어도 유지)
// - 시작할 때(Awake) 저장 파일을 불러옴
// - 로비 씬에 들어올 때마다 설정 UI를 다시 찾아 '살아 있는' 매니저에 코드로 연결
//   (인스펙터 연결은 로비 재진입 시 삭제되는 복사본을 가리키므로 끄고 사용하지 않음)
// - 값이 바뀌면 잠시 후 자동 저장 (닫기 버튼을 누르지 않아도 유지)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Dark;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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

    [Header("자동 저장")]
    public float autoSaveDelay = 0.5f;
    private float _saveTimer = -1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 로비 재진입 시 생기는 복사본
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 다른 매니저(DisplayManager, AudioManager)가 Start에서 바로 쓸 수 있도록 가장 먼저 불러옴
        currentSettings = SaveSystem.LoadSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀔 때마다 UI 다시 찾아서 연결
        if (scene.name == "LobbyScene")
            FindAndApplyUI();
    }

    private void Update()
    {
        if (_saveTimer < 0f) return;
        _saveTimer -= Time.unscaledDeltaTime;
        if (_saveTimer <= 0f)
        {
            _saveTimer = -1f;
            SaveSystem.SaveSettings(currentSettings);
        }
    }

    private void OnApplicationQuit()
    {
        if (Instance == this) SaveSystem.SaveSettings(currentSettings);
    }

    /// <summary>값이 바뀌었음을 알림 → 잠시 후 파일에 자동 저장</summary>
    public void MarkDirty() => _saveTimer = autoSaveDelay;

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

        // 슬라이더: 저장값 표시(이벤트 없이) 후 이 인스턴스에 연결
        BindSlider(masterVolumeSlider, currentSettings.masterVolume, UpdateMasterVolume);
        BindSlider(musicVolumeSlider, currentSettings.musicVolume, UpdateMusicVolume);
        BindSlider(sfxVolumeSlider, currentSettings.sfxVolume, UpdateSFXVolume);
        BindSlider(mouseSensitivitySlider, currentSettings.mouseSensitivity, UpdateSensitivity);
        BindSlider(gammaSlider, currentSettings.gamma, UpdateGamma);
        BindSlider(motionBlurSlider, currentSettings.motionBlur, UpdateMotionBlur);

        // 해상도 / 화면 모드는 DisplayManager가 직접 연결
        if (DisplayManager.Instance != null)
        {
            DisplayManager.Instance.BindDropdown(resolutionDropdown);
            DisplayManager.Instance.BindWindowModeSelector(fullScreenmode);
        }

        if (cancleButton != null)
        {
            cancleButton.onClick.RemoveListener(SaveAll);
            cancleButton.onClick.AddListener(SaveAll);
        }
        if (settingPanal != null) settingPanal.SetActive(false);
    }

    // 인스펙터 연결(사라진 복사본을 가리킬 수 있음)은 끄고, 값만 반영한 뒤 코드로 다시 연결
    public static void BindSlider(Slider slider, float value, UnityAction<float> onChanged)
    {
        if (slider == null) return;
        for (int i = 0; i < slider.onValueChanged.GetPersistentEventCount(); i++)
            slider.onValueChanged.SetPersistentListenerState(i, UnityEventCallState.Off);
        slider.onValueChanged.RemoveAllListeners();
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(onChanged);
    }

    // --- [UI에서 호출할 public 함수들] ---

    public void UpdateMasterVolume(float value)
    {
        currentSettings.masterVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume("Master", value);
        MarkDirty();
    }

    public void UpdateMusicVolume(float value)
    {
        currentSettings.musicVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume("Music", value);
        MarkDirty();
    }

    public void UpdateSFXVolume(float value)
    {
        currentSettings.sfxVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume("SFX", value);
        MarkDirty();
    }
    public void UpdateSensitivity(float value)
    {
        currentSettings.mouseSensitivity = value;
        if (PlayerControll.Local != null)
            PlayerControll.Local.mouseSensitivity = value;
        IngameSettingManager.Instance?.ApplySettings();
        MarkDirty();
    }
    public void UpdateGamma(float value)
    {
        currentSettings.gamma = value;
        IngameSettingManager.Instance?.ApplySettings();
        MarkDirty();
    }
    public void UpdateMotionBlur(float value)
    {
        currentSettings.motionBlur = value;
        IngameSettingManager.Instance?.ApplySettings();
        MarkDirty();
    }
    // 해상도/화면 모드는 실제 적용까지 DisplayManager가 처리
    public void UpdateResolution(int index)
    {
        if (DisplayManager.Instance != null) DisplayManager.Instance.SetResolution(index);
        else { currentSettings.resolutionIndex = index; MarkDirty(); }
    }
    public void UpdateFullScreen(int isFull)
    {
        if (DisplayManager.Instance != null) DisplayManager.Instance.SetWindowMode(isFull);
        else { currentSettings.isFullScreen = isFull; MarkDirty(); }
    }

    // 최종 저장 버튼이나 설정창을 닫을 때 호출
    public void SaveAll()
    {
        _saveTimer = -1f;
        SaveSystem.SaveSettings(currentSettings);
        Debug.Log("설정이 JSON 파일로 저장되었습니다!");
        if (settingPanal != null)
            settingPanal.SetActive(false);
    }
}
