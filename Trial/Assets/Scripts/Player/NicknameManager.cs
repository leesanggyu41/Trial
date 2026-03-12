using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NicknameManager : MonoBehaviour
{
    public static NicknameManager Instance;
    private TMP_InputField NicknameInput;
    private const string NICKNAME_KEY = "PlayerNickname";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

   
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject inputObj = GameObject.FindWithTag("NicknameInput");
        if (inputObj == null) return;

        NicknameInput = inputObj.GetComponent<TMP_InputField>();
        if (NicknameInput == null) return;

        string saved = PlayerPrefs.GetString(NICKNAME_KEY, "");
        if (!string.IsNullOrEmpty(saved))
            NicknameInput.text = saved;

        NicknameInput.onSubmit.RemoveAllListeners();
        NicknameInput.onEndEdit.RemoveAllListeners();
        NicknameInput.onSubmit.AddListener((string value) => SaveNickname());
        NicknameInput.onEndEdit.AddListener((string value) => SaveNickname());

        Debug.Log($"NicknameInput 연결 완료 - 씬: {scene.name}");
    }

    public void SaveNickname()
    {
        string nickname = GetNickname();
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("닉네임을 입력해주세요!");
            return;
        }

        PlayerPrefs.SetString(NICKNAME_KEY, nickname);
        PlayerPrefs.Save();
        Debug.Log($"닉네임 저장됨: {nickname}");
    }

    public string GetNickname()
    {
        if (NicknameInput != null && !string.IsNullOrEmpty(NicknameInput.text))
            return NicknameInput.text;

        return PlayerPrefs.GetString(NICKNAME_KEY, $"Player_{Random.Range(1000, 9999)}");
    }
}