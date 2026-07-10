using UnityEngine;

public class GameResultData : MonoBehaviour
{
    public static GameResultData Instance;

    public string WinnerName = "???";
    public string DeadNamesJoined = "";

    private void Awake()
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
}
