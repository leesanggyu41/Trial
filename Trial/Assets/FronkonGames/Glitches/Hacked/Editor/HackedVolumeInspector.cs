////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace FronkonGames.Glitches.Hacked.Editor
{
  /// <summary> Hacked inspector. </summary>
  [CustomEditor(typeof(HackedVolume))]
  public sealed class HackedVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      DrawFloatSliderWithReset("intensity");

      Separator();

      DrawFloatSliderWithReset("strength");

      DrawFloatSliderWithReset("frameJump");
      IndentLevel++;
      DrawFloatSliderWithReset("frameJumpSpeed", "Speed");
      IndentLevel--;

      DrawFloatSliderWithReset("jitter");
      IndentLevel++;
      DrawFloatSliderWithReset("jitterSpeed", "Speed");
      DrawFloatSliderWithReset("jitterDensity", "Density");
      IndentLevel--;

      DrawFloatSliderWithReset("blocks");
      IndentLevel++;
      DrawFloatSliderWithReset("blockDensity", "Density");
      DrawVector2WithReset("blockAberration", "Aberration", Vector2.one);
      DrawVector2WithReset("blockNoise", "Noise", new Vector2(0.5f, 0.5f));
      IndentLevel--;

      DrawFloatSliderWithReset("waves");
      IndentLevel++;
      DrawFloatSliderWithReset("waveSpeed", "Speed");
      IndentLevel--;

      DrawFloatSliderWithReset("scanlines");
      IndentLevel++;
      DrawFloatSliderWithReset("scanlinesThreshold", "Threshold");
      IndentLevel--;

      DrawFloatSliderWithReset("noise");
      IndentLevel++;
      DrawFloatSliderWithReset("noiseSpeed", "Speed");
      IndentLevel--;

      DrawFloatSliderWithReset("waveRGBSplit");
    }

    protected override void ResetValues() => ((HackedVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Hacked.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox("Renderer Feature 'Hacked' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Hacked[] effects = Hacked.Instances;

        bool anyEnabled = false;
        for (int i = 0; i < effects.Length; i++)
        {
          if (effects[i].isActive == true)
          {
            anyEnabled = true;
            break;
          }
        }

        if (anyEnabled == false)
        {
          Separator();

          EditorGUILayout.HelpBox($"No Renderer Feature '{Constants.Asset.Name}' is active. You must activate it in the Render Features.", MessageType.Warning);
        }
      }
    }
  }
}
