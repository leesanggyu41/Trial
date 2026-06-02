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
Shader "Hidden/Fronkon Games/Glitches/Hacked URP"
{
  Properties
  {
    _MainTex("Main Texture", 2D) = "white" {}
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
    }
    LOD 100
    ZTest Always ZWrite Off Cull Off

    Pass
    {
      Name "Fronkon Games Glitches Hacked"

      HLSLPROGRAM
      #pragma vertex GlitchesVert
      #pragma fragment GlitchesFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile_instancing
      #pragma multi_compile _ STEREO_INSTANCING_ON
      #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_MULTIVIEW_ON

      #include "Glitches.hlsl"

      float _Strength;
      float _FrameJump;
      float _FrameJumpSpeed;
      float _Jitter;
      float _JitterSpeed;
      float _JitterDensity;
      float _Blocks;
      float _BlockDensity;
      float2 _BlockAberration;
      float2 _BlockNoise;
      float _Waves;
      float _WaveSpeed;
      float _WaveRGBSplit;
      float _Scanlines;
      float _ScanlinesThreshold;
      float _Noise;
      float _NoiseSpeed;

      inline float RandomNoise(float2 seed)
      {
        return frac(sin(dot(seed * floor(_EffectTime.y * 30.0), float2(17.13, 3.71))) * 43758.5453123);
      }

      inline float RandomNoise(float seed)
      {
        return RandomNoise(float2(seed, 1.0));
      }

      inline float RandomNoise(float x, float y)
      {
        return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
      }

      half4 GlitchesFrag(GlitchesVaryings input) : SV_Target
      {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        float2 uvR, uvG, uvB;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        // Frame jump.
        float jump = lerp(uv.y, frac(uv.y + (_EffectTime.y * _FrameJumpSpeed)), _FrameJump * _Strength);
        uvR = uvG = uvB = frac(float2(uv.x, jump));

        // Tile jitter.
        float pixelSizeX = 1.0 / _ScreenParams.x;
        UNITY_BRANCH
        if (mod(uvR.y * _JitterDensity, 2.0) < 1.0)
        {
          float tileJitter = pixelSizeX * cos(_EffectTime.y * _JitterSpeed) * _Jitter * _Strength;
          uvR.x += tileJitter;
          uvG.x += tileJitter;
          uvB.x += tileJitter;
        }

        // Blocks.
        float block = RandomNoise(floor(uv * _BlockDensity));
        float displaceNoise = SafePositivePow_float(block, 8.0) * SafePositivePow_float(block, 3.0);
        float splitRGBNoise = SafePositivePow_float(RandomNoise(7.2341), 17.0);
        float offsetX = displaceNoise - splitRGBNoise * _BlockAberration.x;
        float offsetY = displaceNoise - splitRGBNoise * _BlockAberration.y;
        float noiseX = _BlockNoise.x * RandomNoise(13.0);
        float noiseY = _BlockNoise.y * RandomNoise(7.0);
        float2 offset = float2(offsetX * noiseX, offsetY * noiseY) * _Strength;

        uvG = lerp(uvG, uvG + offset, _Blocks);
        uvB = lerp(uvB, uvB - offset, _Blocks);

        // Wave jitter.
        float noise_wave_1 = snoise(float2(uv.y, _EffectTime.y * _WaveSpeed * 20.0)) * (_Strength * _Waves * 32.0);
        float noise_wave_2 = snoise(float2(uv.y, _EffectTime.y * _WaveSpeed * 10.0)) * (_Strength * _Waves * 4.0);
        float noise_wave_x = noise_wave_1 * noise_wave_2 / _ScreenParams.x;
        float rgbSplit_uv_x = (_WaveRGBSplit * 50.0 + (20.0 * _Strength + 1.0)) * noise_wave_x / _ScreenParams.x;

        uvR.x += rgbSplit_uv_x;
        uvG.x += noise_wave_x;
        uvB.x += rgbSplit_uv_x;

        // Scanline.
        float jitter = RandomNoise(uv.y, unity_DeltaTime.x) * 2.0 - 1.0;
        jitter = step(_ScanlinesThreshold, abs(jitter)) * _Scanlines * _Strength;
        jitter = frac(jitter);

        uvR.x += jitter;
        uvG.x += jitter;
        uvB.x += jitter;

        // Mixing.
        pixel.r = SAMPLE_MAIN(uvR).r;
        pixel.g = SAMPLE_MAIN(uvG).g;
        pixel.b = SAMPLE_MAIN(uvB).b;

        // Analog noise.
        float3 noiseColor = pixel.rgb;

        UNITY_BRANCH
        if (RandomNoise(float2(unity_DeltaTime.x * _NoiseSpeed, unity_DeltaTime.x * _NoiseSpeed)) > 0.9)
          noiseColor = Luminance(noiseColor);

        noiseColor += 0.25 * float3(RandomNoise(unity_DeltaTime.x * _NoiseSpeed + uvR / float2(-213.0,  5.53)),
                                    RandomNoise(unity_DeltaTime.x * _NoiseSpeed - uvG / float2( 213.0, -5.53)),
                                    RandomNoise(unity_DeltaTime.x * _NoiseSpeed + uvB / float2( 213.0,  5.53))) - 0.125;

        pixel.rgb = lerp(pixel.rgb, noiseColor, _Noise * _Strength);

        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }
  
  FallBack "Diffuse"
}
