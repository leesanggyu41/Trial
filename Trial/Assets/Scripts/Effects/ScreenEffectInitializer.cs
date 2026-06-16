using UnityEngine;

// 게임 시작 시 화면 효과 알파 값을 초기화합니다.
public class ScreenEffectInitializer : MonoBehaviour
{
    [SerializeField] private Material screenMaterial;
    [SerializeField] private string alphaPropertyName = "_Alpha";
    private static readonly string[] fallbackAlphaPropertyNames = new[] { "_Alpha", "Alpha" };

    private void Start()
    {
        ResetScreenEffect();
    }

    private void ResetScreenEffect()
    {
        var propertyNames = GetAlphaPropertyNames();
        bool materialApplied = false;
        bool controllerApplied = false;

        if (screenMaterial != null)
        {
            foreach (var prop in propertyNames)
            {
                if (screenMaterial.HasProperty(prop))
                {
                    screenMaterial.SetFloat(prop, 0f);
                    materialApplied = true;
                }
            }
        }

        foreach (var prop in propertyNames)
        {
            Shader.SetGlobalFloat(prop, 0f);
        }

        if (MaterialPropertyController.Instances.Count > 0)
        {
            foreach (var prop in propertyNames)
            {
                MaterialPropertyController.SetFloatOnAll(prop, 0f);
            }
            controllerApplied = true;
        }

        if (materialApplied || controllerApplied)
        {
            Debug.Log("ScreenEffectInitializer: 화면 효과 알파 값을 0으로 초기화했습니다.");
        }
        else if (screenMaterial == null)
        {
            Debug.LogWarning("ScreenEffectInitializer: screenMaterial이 할당되지 않았습니다. 초기화할 화면 효과 머티리얼을 할당하세요.");
        }
        else
        {
            Debug.LogWarning($"ScreenEffectInitializer: screenMaterial '{screenMaterial.name}'에 '{alphaPropertyName}' 프로퍼티를 찾을 수 없었습니다. 글로벌 프로퍼티를 0으로 초기화했습니다.");
        }
    }

    private string[] GetAlphaPropertyNames()
    {
        if (string.IsNullOrWhiteSpace(alphaPropertyName))
        {
            return fallbackAlphaPropertyNames;
        }

        if (alphaPropertyName == fallbackAlphaPropertyNames[0] || alphaPropertyName == fallbackAlphaPropertyNames[1])
        {
            return fallbackAlphaPropertyNames;
        }

        return new[] { alphaPropertyName, fallbackAlphaPropertyNames[0], fallbackAlphaPropertyNames[1] };
    }
}
