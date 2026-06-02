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
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FronkonGames.Glitches.Hacked
{
  /// <summary> Hacked Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Glitches/Hacked")]
  public sealed class HackedVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    /// <remarks> An effect with Intensity equal to 0 will not be executed. </remarks>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Hacked settings.

    /// <summary> Modulates the strength of ALL effects [0, 2]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 2.0f, "Modulates the strength of ALL effects [0, 2]. Default 1.")]
    public FloatSliderParameterNoInterpolation strength = new(1.0f, 0.0f, 2.0f);

    /// <summary> Intensity of vertical frame jump [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Intensity of vertical frame jump [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation frameJump = new(0.1f, 0.0f, 1.0f);

    /// <summary> Vertical frame jump speed [0, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Vertical frame jump speed [0, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation frameJumpSpeed = new(1.0f, 0.0f, 10.0f);

    /// <summary> Deformation intensity of horizontal slides [0, 5]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 5.0f, "Deformation intensity of horizontal slides [0, 5]. Default 1.")]
    public FloatSliderParameterNoInterpolation jitter = new(1.0f, 0.0f, 5.0f);

    /// <summary> Deformation velocity of horizontal slides [0, 10]. Default 0.2. </summary>
    [FloatSliderWithReset(0.2f, 0.0f, 10.0f, "Deformation velocity of horizontal slides [0, 10]. Default 0.2.")]
    public FloatSliderParameterNoInterpolation jitterSpeed = new(0.2f, 0.0f, 10.0f);

    /// <summary> Number of slides [0, 50]. Default 15. </summary>
    [FloatSliderWithReset(15.0f, 0.0f, 50.0f, "Number of slides [0, 50]. Default 15.")]
    public FloatSliderParameterNoInterpolation jitterDensity = new(15.0f, 0.0f, 50.0f);

    /// <summary> Intensity of block deformation [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(0.5f, 0.0f, 1.0f, "Intensity of block deformation [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation blocks = new(0.5f, 0.0f, 1.0f);

    /// <summary> Block density [0, 50]. Default 10. </summary>
    [FloatSliderWithReset(10.0f, 0.0f, 50.0f, "Block density [0, 50]. Default 10.")]
    public FloatSliderParameterNoInterpolation blockDensity = new(10.0f, 0.0f, 50.0f);

    /// <summary> Chromatic aberration of block deformation. </summary>
    public Vector2ParameterNoInterpolation blockAberration = new(Vector2.one);

    /// <summary> Noise applied to the position of the blocks. </summary>
    public Vector2ParameterNoInterpolation blockNoise = new(new Vector2(0.5f, 0.5f));

    /// <summary> Intensity of the wave effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Intensity of the wave effect [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation waves = new(1.0f, 0.0f, 1.0f);

    /// <summary> Wave speed [0, 25]. Default 10. </summary>
    [FloatSliderWithReset(10.0f, 0.0f, 25.0f, "Wave speed [0, 25]. Default 10.")]
    public FloatSliderParameterNoInterpolation waveSpeed = new(10.0f, 0.0f, 25.0f);

    /// <summary> Color channel shifts [0, 50]. Default 30. </summary>
    [FloatSliderWithReset(30.0f, 0.0f, 50.0f, "Color channel shifts [0, 50]. Default 30.")]
    public FloatSliderParameterNoInterpolation waveRGBSplit = new(30.0f, 0.0f, 50.0f);

    /// <summary> Intensity of the scanlines effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(0.2f, 0.0f, 1.0f, "Intensity of the scanlines effect [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation scanlines = new(0.2f, 0.0f, 1.0f);

    /// <summary> Range of scanline effect [0, 1]. Default 0.8. </summary>
    [FloatSliderWithReset(0.8f, 0.0f, 1.0f, "Range of scanline effect [0, 1]. Default 0.8.")]
    public FloatSliderParameterNoInterpolation scanlinesThreshold = new(0.8f, 0.0f, 1.0f);

    /// <summary> Noise intensity [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Noise intensity [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation noise = new(0.1f, 0.0f, 1.0f);

    /// <summary> Noise speed [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Noise speed [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation noiseSpeed = new(0.1f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Color settings.

    /// <summary> Brightness [-1, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, -1.0f, 1.0f, "Brightness [-1, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation brightness = new(0.0f, -1.0f, 1.0f);

    /// <summary> Contrast [0, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Contrast [0, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation contrast = new(1.0f, 0.0f, 10.0f);

    /// <summary> Gamma [0.1, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 10.0f, "Gamma [0.1, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation gamma = new(1.0f, 0.1f, 10.0f);

    /// <summary> The color wheel [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "The color wheel [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation hue = new(0.0f, 0.0f, 1.0f);

    /// <summary> Intensity of a colors [0, 2]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 2.0f, "Intensity of a colors [0, 2]. Default 1.")]
    public FloatSliderParameterNoInterpolation saturation = new(1.0f, 0.0f, 2.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Advanced settings.

    /// <summary> Does it affect the Scene View? </summary>
    [ToggleWithReset(false, "Does it affect the Scene View?")]
    public BoolParameterNoInterpolation affectSceneView = new(false);

    /// <summary> Use scaled time. </summary>
    [ToggleWithReset(false, "Use scaled time.")]
    public BoolParameterNoInterpolation useScaledTime = new(true);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary> Reset to default values. </summary>
    public void Reset()
    {
      intensity.value = 1.0f;

      strength.value = 1.0f;
      frameJump.value = 0.1f;
      frameJumpSpeed.value = 1.0f;
      jitter.value = 1.0f;
      jitterSpeed.value = 0.2f;
      jitterDensity.value = 15.0f;
      blocks.value = 0.5f;
      blockDensity.value = 10.0f;
      blockAberration.value = Vector2.one;
      blockNoise.value = new Vector2(0.5f, 0.5f);
      waves.value = 1.0f;
      waveSpeed.value = 10.0f;
      waveRGBSplit.value = 30.0f;
      scanlines.value = 0.2f;
      scanlinesThreshold.value = 0.8f;
      noise.value = 0.1f;
      noiseSpeed.value = 0.1f;

      brightness.value = 0.0f;
      contrast.value = 1.0f;
      gamma.value = 1.0f;
      hue.value = 0.0f;
      saturation.value = 1.0f;

      affectSceneView.value = false;
      useScaledTime.value = true;
    }

    /// <summary> Is the effect active? </summary>
    public bool IsActive() => intensity.overrideState == true && intensity.value > 0.0f;

    /// <summary> Is the effect tile compatible? </summary>
    public bool IsTileCompatible() => false;
  }
}
