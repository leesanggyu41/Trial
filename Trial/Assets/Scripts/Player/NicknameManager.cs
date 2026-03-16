// NicknameManager는 플레이어의 닉네임을 관리하는 클래스입니다.
// 플레이어가 닉네임을 입력하고 저장할 수 있도록 하며, 
//게임 씬에서 해당 닉네임을 불러와 사용할 수 있도록 합니다.
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

    // 씬이 로드될 때마다 닉네임 입력 필드를 찾아서 연결하는 메서드입니다.
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
    // 닉네임을 저장하는 메서드입니다. 입력된 닉네임이 유효한 경우 PlayerPrefs에 저장합니다.
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
    // 저장된 닉네임을 반환하는 메서드입니다. 입력 필드에 값이 있으면 그 값을 반환하고, 그렇지 않으면 PlayerPrefs에서 가져오거나 기본값을 생성하여 반환합니다.
    public string GetNickname()
    {
        if (NicknameInput != null && !string.IsNullOrEmpty(NicknameInput.text))
            return NicknameInput.text;

        return PlayerPrefs.GetString(NICKNAME_KEY, $"Player_{Random.Range(1000, 9999)}");
    }
}