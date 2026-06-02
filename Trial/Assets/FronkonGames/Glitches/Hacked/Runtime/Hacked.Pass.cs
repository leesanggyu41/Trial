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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace FronkonGames.Glitches.Hacked
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Hacked
  {
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private HackedVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int Strength = Shader.PropertyToID("_Strength");
        internal static readonly int FrameJump = Shader.PropertyToID("_FrameJump");
        internal static readonly int FrameJumpSpeed = Shader.PropertyToID("_FrameJumpSpeed");
        internal static readonly int Jitter = Shader.PropertyToID("_Jitter");
        internal static readonly int JitterSpeed = Shader.PropertyToID("_JitterSpeed");
        internal static readonly int JitterDensity = Shader.PropertyToID("_JitterDensity");
        internal static readonly int Blocks = Shader.PropertyToID("_Blocks");
        internal static readonly int BlockDensity = Shader.PropertyToID("_BlockDensity");
        internal static readonly int BlockAberration = Shader.PropertyToID("_BlockAberration");
        internal static readonly int BlockNoise = Shader.PropertyToID("_BlockNoise");
        internal static readonly int Waves = Shader.PropertyToID("_Waves");
        internal static readonly int WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        internal static readonly int WaveRGBSplit = Shader.PropertyToID("_WaveRGBSplit");
        internal static readonly int Scanlines = Shader.PropertyToID("_Scanlines");
        internal static readonly int ScanlinesThreshold = Shader.PropertyToID("_ScanlinesThreshold");
        internal static readonly int Noise = Shader.PropertyToID("_Noise");
        internal static readonly int NoiseSpeed = Shader.PropertyToID("_NoiseSpeed");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass() : base()
      {
        profilingSampler = new ProfilingSampler(Constants.Asset.AssemblyName);
      }

      private void UpdateMaterial()
      {
        material.shaderKeywords = null;
        material.SetFloat(ShaderIDs.Intensity, volume.intensity.value);

        float time = volume.useScaledTime.value == true ? Time.time : Time.unscaledTime;
        material.SetVector(ShaderIDs.EffectTime, new Vector4(time / 20.0f, time, time * 2.0f, time * 3.0f));

        material.SetFloat(ShaderIDs.Strength, volume.strength.value);
        material.SetFloat(ShaderIDs.FrameJump, volume.frameJump.value * 0.1f);
        material.SetFloat(ShaderIDs.FrameJumpSpeed, volume.frameJumpSpeed.value);
        material.SetFloat(ShaderIDs.Jitter, volume.jitter.value * 10.0f);
        material.SetFloat(ShaderIDs.JitterSpeed, volume.jitterSpeed.value);
        material.SetFloat(ShaderIDs.JitterDensity, volume.jitterDensity.value);
        material.SetFloat(ShaderIDs.Blocks, volume.blocks.value);
        material.SetFloat(ShaderIDs.BlockDensity, volume.blockDensity.value);
        material.SetVector(ShaderIDs.BlockAberration, volume.blockAberration.value);
        material.SetVector(ShaderIDs.BlockNoise, volume.blockNoise.value * 0.1f);
        material.SetFloat(ShaderIDs.Waves, volume.waves.value);
        material.SetFloat(ShaderIDs.WaveSpeed, volume.waveSpeed.value);
        material.SetFloat(ShaderIDs.WaveRGBSplit, volume.waveRGBSplit.value);
        material.SetFloat(ShaderIDs.Scanlines, volume.scanlines.value * 0.01f);
        material.SetFloat(ShaderIDs.ScanlinesThreshold, Mathf.Min(volume.scanlinesThreshold.value, 0.99f));
        material.SetFloat(ShaderIDs.Noise, volume.noise.value);
        material.SetFloat(ShaderIDs.NoiseSpeed, volume.noiseSpeed.value);

        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<HackedVolume>();
        if (material == null || volume == null || volume.IsActive() == false)
          return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer == true)
          return;

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        if (cameraData.camera.cameraType == CameraType.SceneView && volume.affectSceneView.value == false || cameraData.postProcessEnabled == false)
          return;

        TextureHandle source = resourceData.activeColorTexture;
        TextureHandle destination = renderGraph.CreateTexture(source.GetDescriptor(renderGraph));

        UpdateMaterial();

        RenderGraphUtils.BlitMaterialParameters pass = new(source, destination, material, 0);
        renderGraph.AddBlitPass(pass, $"{Constants.Asset.AssemblyName}.Pass");

        resourceData.cameraColor = destination;
      }
    }
  }
}
