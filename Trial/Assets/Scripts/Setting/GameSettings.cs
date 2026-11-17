using UnityEngine;
[System.Serializable]
public class GameSettings
{
    //Audio
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    //Controls
    public float mouseSensitivity = 1f;
    //Video
    public int resolutionIndex = 0;      // 드롭다운 순번 (참고용)
    public int resolutionWidth = 0;      // 실제 해상도 (0이면 모니터 기본 해상도 사용)
    public int resolutionHeight = 0;
    public int refreshRate = 0;
    public float gamma = 1f;
    public float motionBlur = 0f;
    public int isFullScreen = 1;         // 0: 전체화면, 1: 테두리 없는 창, 2: 창모드

}
