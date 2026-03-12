using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System.Collections.Generic;

public class RoomCreateManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField roomNameInput;
    public Toggle passwordToggle;
    public TMP_InputField passwordInput;

    public async void OnClickCreateRoom()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
            roomName = $"Experiment_{Random.Range(1000, 9999)}";

        var customProps = new Dictionary<string, SessionProperty>();
        customProps["IsPassword"] = passwordToggle.isOn ? 1 : 0;
        if (passwordToggle.isOn) customProps["PwData"] = passwordInput.text;

        // ✅ Runner 생성을 ServerConnectionManager에 위임
        await ServerConnectionManager.Instance.CreateRoom(roomName, customProps);
    }
}