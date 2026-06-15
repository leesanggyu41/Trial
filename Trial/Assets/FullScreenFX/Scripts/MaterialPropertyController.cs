using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MaterialPropertyController : MonoBehaviour
{
    [TextArea(2, 5)]
    public string HowToUse = "For the effect to appear, Add the material(s) in this list as a Full Screen Pass Renderer Feature to the active Universal Renderer Data asset. For more details check out the Documentation.";
    [Tooltip("Drag and drop your materials here in the Inspector.")]   
    public List<Material> materials; // Drag and drop your materials here in the Inspector
    string propertyName = "_enabled"; // Name of the property in the shader

    private bool isEnabled = true; // Internal "Is Enabled" state

    private void OnValidate()
    {
        // Update the shader properties for all materials whenever the "Is Enabled" state changes in the Inspector
        UpdateShaderProperties();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateShaderProperties();
        }
    }

    // Runtime registry of all controllers in the scene for global access
    public static readonly System.Collections.Generic.List<MaterialPropertyController> Instances = new System.Collections.Generic.List<MaterialPropertyController>();

    /// <summary>
    /// Set a float property on all materials managed by all MaterialPropertyController instances.
    /// Useful for runtime-driven full-screen effect values.
    /// </summary>
    public static void SetFloatOnAll(string propertyName, float value)
    {
        foreach (var ctrl in Instances)
        {
            if (ctrl == null || ctrl.materials == null) continue;
            foreach (var mat in ctrl.materials)
            {
                if (mat == null) continue;
                mat.SetFloat(propertyName, value);
            }
        }
    }

    private void OnEnable()
    {
        if (!Instances.Contains(this)) Instances.Add(this);
        isEnabled = true;
        UpdateShaderProperties();
    }

    private void OnDisable()
    {
        Instances.Remove(this);
        isEnabled = false;
        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        if (materials != null && materials.Count > 0)
        {
            int value = isEnabled ? 1 : 0;
            foreach (var material in materials)
            {
                if (material != null)
                {
                    material.SetInt(propertyName, value);
                }
            }
        }
    }
}
