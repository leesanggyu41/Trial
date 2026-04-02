using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Resources")]
    public AudioMixer audioMixer;

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
        ApplyAllSettings();
    }
    public void SetVolume(string parameterName, float sliderValue)
    {
        if (audioMixer == null) return;

        float dB = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;
        audioMixer.SetFloat(parameterName, dB);
    }
    public void ApplyAllSettings()
    {
        if (SettingManager.Instance == null || SettingManager.Instance.currentSettings == null) 
            return;

        var settings = SettingManager.Instance.currentSettings;

        // 저장된 각 볼륨 데이터를 하나씩 믹서에 꽂아줍니다.
        SetVolume("Master", settings.masterVolume);
        SetVolume("Music", settings.musicVolume);
        SetVolume("SFX", settings.sfxVolume);

        Debug.Log("AudioManager: 모든 오디오 설정이 실시간 반영되었습니다.");
    }
}