using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.Rendering;
using FronkonGames.Glitches.Hacked;

public class SpectatorManager : MonoBehaviour
{
    public static SpectatorManager Instance;

    [Header("UI")]
    public GameObject spectatorUI;
    public TMP_Text nicknameText;

    public Volume glitchVolume;
    private HackedVolume _hackedVolume;
    //public CanvasGroup noisePanel;

    private List<PlayerControll> _alivePlayers = new List<PlayerControll>();
    private int _currentIndex = 0;
    public bool IsSpectating { get; private set; } = false;

    private void Awake() => Instance = this;

    private void Start()
    {
        // HackedVolume 가져오기
        glitchVolume.profile.TryGet(out _hackedVolume);
        
        // 시작할 때 꺼두기
        if (_hackedVolume != null)
            _hackedVolume.intensity.value = 0f;
    }

    public void StartSpectating()
    {
        IsSpectating = true;
        spectatorUI.SetActive(true);

        RefreshAlivePlayers();

        if (_alivePlayers.Count > 0)
            SwitchTo(0);
    }

    private void RefreshAlivePlayers()
    {
        _alivePlayers = FindObjectsByType<PlayerControll>(FindObjectsSortMode.None)
            .Where(p => !p.GetComponent<PlayerGameData>().IsDead)
            .ToList();
    }

    private void LateUpdate()
    {
        if (!IsSpectating) return;
        if (_alivePlayers.Count == 0) return;
        PlayerControll target = _alivePlayers[_currentIndex];
        Camera cam = PlayerControll.Local.PlayerCamera;
        cam.transform.rotation = target.HeadCameraPoint.parent.rotation * target.NetworkedHeadRotation;
        if (Mouse.current.leftButton.wasPressedThisFrame)
            StartCoroutine(SwitchWithNoise(1));

        if (Mouse.current.rightButton.wasPressedThisFrame)
            StartCoroutine(SwitchWithNoise(-1));
    }

    private IEnumerator SwitchWithNoise(int direction)
    {
        if (_hackedVolume != null)
            _hackedVolume.intensity.value = 1f;

        yield return new WaitForSeconds(0.3f);

        
        if (_hackedVolume != null)
            _hackedVolume.intensity.value = 0f;

        RefreshAlivePlayers();
        if (_alivePlayers.Count == 0) yield break;

        _currentIndex = (_currentIndex + direction + _alivePlayers.Count) % _alivePlayers.Count;
        SwitchTo(_currentIndex);
    }

    private void SwitchTo(int index)
    {
        _currentIndex = index;
        PlayerControll target = _alivePlayers[index];

        // 카메라를 관전 대상의 HeadCameraPoint에 붙이기
        Camera cam = PlayerControll.Local.PlayerCamera;
        cam.transform.SetParent(target.HeadCameraPoint);
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;

        nicknameText.text = "<" + target.NameText.text + ">";
    }
}