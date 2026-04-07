using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class IngameSettingManager : MonoBehaviour
{
    public static IngameSettingManager Instance;

    public LiftGammaGain liftGammaGain;
    public MotionBlur motionBlur;
    public PlayerControll playerController;

    private Volume postProcessVolume;

     void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; DontDestroyOnLoad(gameObject); 
            SceneManager.sceneLoaded += OnSceneLoaded; // 씬이 로드될 때마다 OnSceneLoaded 호출하도록 등록
        
        }
        else { Destroy(gameObject); }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        postProcessVolume = FindFirstObjectByType<Volume>();
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out liftGammaGain);
            postProcessVolume.profile.TryGet(out motionBlur);
        }
        if(SceneManager.GetActiveScene().name == "GameScene")
        {
            Debug.Log("GameScene loaded, finding PlayerController...");
            Invoke(nameof(FindPlayerController), 0.5f); // 약간의 지연 후에 PlayerController 찾기
        }

        

        ApplySettings();
    }

    void FindPlayerController()
    {
        playerController = FindFirstObjectByType<PlayerControll>();
        if (playerController != null)
        {
            Debug.Log("PlayerController found!");
            ApplySettings();
        }
        else
        {
            Debug.LogWarning("PlayerController not found in the scene.");
        }
    }

    public void ApplySettings()
    {
        if (liftGammaGain != null)
        {
            liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, SettingManager.Instance.currentSettings.gamma - 1f));
        }

        if (motionBlur != null)
        {
            motionBlur.intensity.value = SettingManager.Instance.currentSettings.motionBlur;

            motionBlur.active = SettingManager.Instance.currentSettings.motionBlur > 0.01f;
        }
            

        if (playerController != null)
            playerController.mouseSensitivity = SettingManager.Instance.currentSettings.mouseSensitivity;
    }

}
