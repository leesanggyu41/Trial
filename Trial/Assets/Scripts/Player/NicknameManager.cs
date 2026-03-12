using UnityEngine;
using TMPro;
public class NicknameManager : MonoBehaviour
{
    public static NicknameManager Instance;
    [Header("UI연결")]
    public TMP_InputField NicknameInput;

    private const string NICKNAME_KEY = "PlayerNickname";
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        string saved = PlayerPrefs.GetString(NICKNAME_KEY, "");
        if (!string.IsNullOrEmpty(saved) && NicknameInput != null)
            NicknameInput.text = saved;

            if (NicknameInput != null)
        {
            //  엔터 치면 저장
            NicknameInput.onSubmit.AddListener(_ => SaveNickname());

            //  다른 곳 클릭해서 포커스 잃으면 저장
            NicknameInput.onEndEdit.AddListener(_ => SaveNickname());
        }
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

        // 인풋 없으면 저장된 값 반환
        return PlayerPrefs.GetString(NICKNAME_KEY, $"Player_{Random.Range(1000, 9999)}");
    }
}
