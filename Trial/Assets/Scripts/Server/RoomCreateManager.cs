// RoomCreateManager는 방 생성 UI에서 방을 생성하는 로직을 처리하는 클래스입니다.
// 사용자가 방 이름을 입력하고 비밀번호 설정 여부를 선택한 후 [방 생성] 버튼을 클릭하면 OnClickCreateRoom()이 호출되어 입력된 정보를 바탕으로 방을 생성하도록 ServerConnectionManager에 요청합니다.
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
    public GameObject waitpanal;

    public async void OnClickCreateRoom()
    {
        waitpanal.SetActive(true);
        string roomName = roomNameInput.text;
        //방이름을 적지 않을시 임의로 방이름 생성
        if (string.IsNullOrEmpty(roomName))
            roomName = $"Experiment_{Random.Range(1000, 9999)}";

        var customProps = new Dictionary<string, SessionProperty>();
        customProps["IsPassword"] = passwordToggle.isOn ? 1 : 0;
        if (passwordToggle.isOn) customProps["PwData"] = passwordInput.text;

        // Runner 생성을 ServerConnectionManager에 위임
        await ServerConnectionManager.Instance.CreateRoom(roomName, customProps);
    }
}