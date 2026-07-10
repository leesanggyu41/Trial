using UnityEngine;

// Minimal HPScreenEffect: set FS material alpha from local player's HP
public class HPScreenEffect : MonoBehaviour
{
    [SerializeField] private Material screenMaterial;
    [SerializeField] private string alphaPropertyName = "_Alpha";
    [SerializeField] private int maxHp = 4;
    [SerializeField] private int minHp = 1;

    private static readonly string[] alphaPropertyNames = new[] { "_Alpha", "Alpha" };
    private PlayerGameData localPlayer;
    private int lastHp = int.MinValue;

    private void Start()
    {
        if (screenMaterial == null)
        {
            Debug.LogWarning("HPScreenEffect: screenMaterial이 할당되지 않았습니다.");
        }
        else
        {
            Debug.Log($"HPScreenEffect started: material={screenMaterial.name}, alphaPropertyName={alphaPropertyName}");
        }
    }

    private void Update()
    {
        if (localPlayer == null) FindLocalPlayer();
        if (screenMaterial == null)
        {
            if (lastHp != int.MinValue)
            {
                ResetScreenEffect();
                lastHp = int.MinValue;
            }
            return;
        }

        if (localPlayer == null)
        {
            ResetScreenEffect();
            lastHp = int.MinValue;
            return;
        }

        int hp = localPlayer.HP;
        if (hp == lastHp && !IsGameInactive()) return;
        lastHp = hp;

        float alpha;
        if (localPlayer.IsDead || IsGameInactive())
        {
            alpha = 0f;
        }
        else
        {
            alpha = Mathf.Clamp01(Mathf.InverseLerp(maxHp, minHp, hp));
        }

        ApplyAlpha(alpha);
        //Debug.Log($"HPScreenEffect: HP {hp} -> Alpha {alpha:F2}");
    }

    private bool IsGameInactive()
    {
        return GameTurnManager.Instance != null && GameTurnManager.Instance.NowTurn == GameTurn.Win;
    }

    private void ApplyAlpha(float alpha)
    {
        if (screenMaterial != null)
        {
            foreach (var prop in alphaPropertyNames)
            {
                if (screenMaterial.HasProperty(prop))
                {
                    screenMaterial.SetFloat(prop, alpha);
                    //Debug.Log($"HPScreenEffect: screenMaterial.SetFloat('{prop}', {alpha:F2})");
                }
            }

            if (!screenMaterial.HasProperty(alphaPropertyNames[0]) && !screenMaterial.HasProperty(alphaPropertyNames[1]))
            {
                Debug.LogWarning($"HPScreenEffect: screenMaterial에 '{alphaPropertyNames[0]}' 또는 '{alphaPropertyNames[1]}' 프로퍼티가 없습니다.");
            }
        }

        // if (MaterialPropertyController.Instances.Count > 0)
        // {
        //     foreach (var prop in alphaPropertyNames)
        //     {
        //         MaterialPropertyController.SetFloatOnAll(prop, alpha);
        //         Shader.SetGlobalFloat(prop, alpha);
        //     }
        // }
        // else
        // {
        //     Debug.LogWarning("HPScreenEffect: MaterialPropertyController 인스턴스가 씬에 없습니다. FS_LowHP 메테리얼을 FullScreenFX 컨트롤러에 등록했는지 확인하세요.");
        // }
    }

    private void ResetScreenEffect()
    {
        ApplyAlpha(0f);
    }

    private void FindLocalPlayer()
    {
        // Try PlayerControll.Local first
        if (PlayerControll.Local != null)
        {
            var pd = PlayerControll.Local.GetComponent<PlayerGameData>();
            if (pd != null)
            {
                localPlayer = pd;
                Debug.Log("HPScreenEffect: found local via PlayerControll.Local");
                return;
            }
        }

        // Fallback: scan PlayerGameData for InputAuthority
        var all = FindObjectsOfType<PlayerGameData>();
        foreach (var p in all)
        {
            try
            {
                if (p.Object != null && p.Object.HasInputAuthority)
                {
                    localPlayer = p;
                    Debug.Log("HPScreenEffect: found local via Object.HasInputAuthority");
                    return;
                }
            }
            catch { }
        }
    }
}
