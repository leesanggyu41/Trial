// RoomCreateManager는 방 생성 UI에서 방을 생성하는 로직을 처리하는 클래스입니다.
// 사용자가 방 이름을 입력하고 비밀번호 설정 여부를 선택한 후 [방 생성] 버튼을 클릭하면 OnClickCreateRoom()이 호출되어 입력된 정보를 바탕으로 방을 생성하도록 ServerConnectionManager에 요청합니다.
// 모드 선택: [멀티] 기존처럼 방을 만들어 다른 플레이어를 기다림 / [AI 대전] 방 없이 바로 AI와 1:1 대전 시작
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

    [Header("모드 선택")]
    public Toggle multiModeToggle;
    public Toggle botModeToggle;
    [Tooltip("AI 대전일 때 숨길 오브젝트 (방 이름, 비밀번호 등)")]
    public GameObject[] multiOnlyObjects;

    [Header("모드 버튼 색")]
    public Color selectedColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    public Color unselectedColor = new Color(0.12f, 0.12f, 0.12f, 0.9f);

    private bool _creating = false;

    private bool IsBotMode => botModeToggle != null && botModeToggle.isOn;

    private void Start()
    {
        if (multiModeToggle != null) multiModeToggle.onValueChanged.AddListener(_ => RefreshModeUI());
        if (botModeToggle != null) botModeToggle.onValueChanged.AddListener(_ => RefreshModeUI());
        RefreshModeUI();
    }

    private void OnEnable() => _creating = false;

    private void RefreshModeUI()
    {
        bool bot = IsBotMode;

        if (multiOnlyObjects != null)
            foreach (var go in multiOnlyObjects)
                if (go != null) go.SetActive(!bot);

        PaintToggle(multiModeToggle);
        PaintToggle(botModeToggle);
    }

    private void PaintToggle(Toggle t)
    {
        if (t == null || t.targetGraphic == null) return;
        t.targetGraphic.color = t.isOn ? selectedColor : unselectedColor;
    }

    public async void OnClickCreateRoom()
    {
        if (_creating) return; // 중복 클릭 방지
        _creating = true;

        // AI 대전: 방을 만들지 않고 바로 AI와 1:1
        if (IsBotMode)
        {
            await ServerConnectionManager.Instance.StartBotMatch();
            _creating = false;
            return;
        }

        string roomName = roomNameInput.text;
        //방이름을 적지 않을시 임의로 방이름 생성
        if (string.IsNullOrEmpty(roomName))
            roomName = $"Experiment_{Random.Range(1000, 9999)}";

        var customProps = new Dictionary<string, SessionProperty>();
        customProps["IsPassword"] = passwordToggle.isOn ? 1 : 0;
        if (passwordToggle.isOn) customProps["PwData"] = passwordInput.text;

        // Runner 생성을 ServerConnectionManager에 위임
        await ServerConnectionManager.Instance.CreateRoom(roomName, customProps);
        _creating = false;
    }
}
