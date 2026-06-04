using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinUIManager : NetworkBehaviour
{
    public static WinUIManager Instance;

    public GameObject winPanel;
    public TMP_Text winnerText;

    private void Awake() => Instance = this;

    private void Start() => winPanel.SetActive(false);

    public void ShowWinUI(string winnerName)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        winPanel.SetActive(true);
        winnerText.text = $"{winnerName} 승리!";
        // 나중에 애니메이션 추가
    }

    public void OnQuitButton()
    {
        ServerConnectionManager.Instance.LeaveRoom();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_QuitAll()
    {
        StartCoroutine(QuitCoroutine());
    }

    private IEnumerator QuitCoroutine()
    {
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null)
            yield return runner.Shutdown(destroyGameObject: false);

        SceneManager.LoadScene("LobbyScene");
    }
}