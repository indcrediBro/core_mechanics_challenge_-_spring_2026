Shader "DayNightWeather/ProceduralSkybox"
{
    Properties
    {
        // Sky gradient
        [HDR] _SkyTopColor      ("Sky Top Color",      Color) = (0.1, 0.4, 0.9, 1)
        [HDR] _SkyHorizonColor  ("Sky Horizon Color",  Color) = (0.6, 0.8, 1.0, 1)
        [HDR] _SkyBottomColor   ("Sky Bottom Color",   Color) = (0.18, 0.16, 0.12, 1)
        _HorizonSharpness       ("Horizon Sharpness",  Range(1, 20)) = 6

        // Sun
        [HDR] _SunColor         ("Sun Color",          Color) = (1, 0.95, 0.8, 1)
        _SunSize                ("Sun Size",            Range(0.001, 0.1)) = 0.04
        _SunBloom               ("Sun Bloom",           Range(0, 5)) = 2.0
        _SunDirection           ("Sun Direction",       Vector) = (0, 1, 0, 0)

        // Moon
        [HDR] _MoonColor        ("Moon Color",          Color) = (0.85, 0.9, 1.0, 1)
        _MoonSize               ("Moon Size",            Range(0.001, 0.1)) = 0.025
        _MoonDirection          ("Moon Direction",       Vector) = (0, -1, 0, 0)
        _MoonTexture            ("Moon Texture",         2D) = "white" {}

        // Stars
        _StarBrightness         ("Star Brightness",      Range(0, 2)) = 0
        _StarDensity            ("Star Density",         Range(0, 100)) = 40
        _StarSize               ("Star Size",            Range(0.001, 0.05)) = 0.008
        _StarTwinkleSpeed       ("Star Twinkle Speed",   Range(0, 5)) = 1.5
        _StarTwinkleMagnitude   ("Star Twinkle Magnitude", Range(0, 1)) = 0.3

        // Clouds
        [HDR] _CloudColor       ("Cloud Color",          Color) = (1, 1, 1, 1)
        _CloudCoverage          ("Cloud Coverage",        Range(0, 1)) = 0.4
        _CloudSpeed             ("Cloud Speed",           Range(0, 0.1)) = 0.005
        _CloudSoftness          ("Cloud Softness",        Range(0, 1)) = 0.5
        _CloudScale             ("Cloud Scale",           Range(0.5, 10)) = 3.0
        _CloudHeight            ("Cloud Height",          Range(0, 1)) = 0.1

        // Atmospheric scattering
        _HorizonFogColor        ("Horizon Fog Color",    Color) = (0.8, 0.9, 1.0, 1)
        _HorizonFogStrength     ("Horizon Fog Strength", Range(0, 1)) = 0.4
        _HorizonFogWidth        ("Horizon Fog Width",    Range(0.001, 0.5)) = 0.08
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ─── Properties ──────────────────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _SkyTopColor, _SkyHorizonColor, _SkyBottomColor;
                float  _HorizonSharpness;

                float4 _SunColor;
                float  _SunSize, _SunBloom;
                float4 _SunDirection;

                float4 _MoonColor;
                float  _MoonSize;
                float4 _MoonDirection;

                float  _StarBrightness, _StarDensity, _StarSize;
                float  _StarTwinkleSpeed, _StarTwinkleMagnitude;

                float4 _CloudColor;
                float  _CloudCoverage, _CloudSpeed, _CloudSoftness, _CloudScale, _CloudHeight;

                float4 _HorizonFogColor;
                float  _HorizonFogStrength, _HorizonFogWidth;
            CBUFFER_END

            TEXTURE2D(_MoonTexture); SAMPLER(sampler_MoonTexture);

            // ─── Structs ─────────────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldDir   : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ─── Noise Helpers ───────────────────────────────────────────────
            // Hash function
            float2 Hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            float Hash1(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Smooth value noise for clouds
            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

                float a = Hash1(i);
                float b = Hash1(i + float2(1, 0));
                float c = Hash1(i + float2(0, 1));
                float d = Hash1(i + float2(1, 1));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // FBM (fractal brownian motion) for cloud detail
            float CloudFBM(float2 uv)
            {
                float v   = 0.0;
                float amp = 0.5;
                float freq= 1.0;
                for (int i = 0; i < 6; i++)
                {
                    v   += amp * ValueNoise(uv * freq);
                    amp *= 0.5;
                    freq*= 2.0;
                    uv  += float2(0.1, 0.15) * freq;
                }
                return v;
            }

            // ─── Sky Gradient ────────────────────────────────────────────────
            float3 SkyGradient(float3 dir)
            {
                float y = dir.y;
                // Above horizon
                if (y >= 0.0)
                {
                    float t = pow(clamp(y, 0, 1), 1.0 / _HorizonSharpness);
                    return lerp(_SkyHorizonColor.rgb, _SkyTopColor.rgb, t);
                }
                else
                {
                    float t = pow(clamp(-y, 0, 1), 1.0 / _HorizonSharpness);
                    return lerp(_SkyHorizonColor.rgb, _SkyBottomColor.rgb, t);
                }
            }

            // ─── Sun ─────────────────────────────────────────────────────────
            float3 SunDisc(float3 dir)
            {
                float3 sunDir = normalize(_SunDirection.xyz);
                float  cosA   = dot(dir, sunDir);
                float  angle  = acos(clamp(cosA, -1, 1));

                // Hard disc
                float disc = step(angle, _SunSize);

                // Bloom glow
                float bloom = exp(-max(0, angle - _SunSize) * 80.0) * _SunBloom;

                // Atmosphere limb darkening
                float limb  = smoothstep(_SunSize, _SunSize * 0.5, angle);

                return _SunColor.rgb * (disc * limb + bloom * 0.3);
            }

            // ─── Moon ────────────────────────────────────────────────────────
            float3 MoonDisc(float3 dir)
            {
                float3 moonDir = normalize(_MoonDirection.xyz);
                float  cosA    = dot(dir, moonDir);
                float  angle   = acos(clamp(cosA, -1, 1));

                if (angle > _MoonSize * 2.0) return float3(0, 0, 0);

                // UV onto moon disc for texture
                float3 tangent  = normalize(cross(moonDir, float3(0, 1, 0.001)));
                float3 bitangent= cross(moonDir, tangent);
                float2 moonUV   = float2(dot(dir - moonDir, tangent), dot(dir - moonDir, bitangent));
                moonUV          = moonUV / (_MoonSize) * 0.5 + 0.5;

                float  disc     = step(angle, _MoonSize);
                float3 tex      = SAMPLE_TEXTURE2D(_MoonTexture, sampler_MoonTexture, moonUV).rgb;
                float  glow     = exp(-max(0, angle - _MoonSize) * 60.0) * 0.2;

                return _MoonColor.rgb * (tex * disc + glow);
            }

            // ─── Stars ───────────────────────────────────────────────────────
            float3 Stars(float3 dir, float brightness)
            {
                if (brightness < 0.001) return float3(0,0,0);

                // Convert direction to spherical UV to tile consistently
                float phi   = atan2(dir.z, dir.x) / (2.0 * PI);
                float theta = asin(clamp(dir.y, -1, 1)) / PI + 0.5;
                float2 uv   = float2(phi, theta) * _StarDensity;

                float2 cell = floor(uv);
                float2 frac_ = frac(uv);

                float3 color = float3(0, 0, 0);

                // Check surrounding cells for nearest star
                for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    float2 neighbor = float2(x, y);
                    float2 h        = Hash2(cell + neighbor);
                    float2 starPos  = neighbor + h - frac_;
                    float  dist     = length(starPos);

                    // Twinkle
                    float twinkle   = 1.0 + _StarTwinkleMagnitude *
                                      sin(_Time.y * _StarTwinkleSpeed * (h.x * 10.0 + 1.0));
                    float size      = _StarSize * twinkle;

                    float  star     = smoothstep(size, size * 0.5, dist);
                    // Star color variation
                    float3 starCol  = lerp(float3(0.8, 0.9, 1.0), float3(1.0, 0.9, 0.7), h.x);
                    color          += starCol * star * brightness;
                }

                return color;
            }

            // ─── Clouds ──────────────────────────────────────────────────────
            float4 Clouds(float3 dir)
            {
                // Only show clouds above horizon
                float  elevation = dir.y;
                if (elevation < _CloudHeight - 0.1) return float4(0, 0, 0, 0);

                // Project onto horizontal plane
                float2 cloudUV = dir.xz / max(elevation + _CloudHeight, 0.01);
                cloudUV       *= (1.0 / _CloudScale);
                cloudUV       += float2(_Time.y * _CloudSpeed, _Time.y * _CloudSpeed * 0.6);

                float density = CloudFBM(cloudUV);

                // Coverage threshold
                float coverage = smoothstep(1.0 - _CloudCoverage, 1.0 - _CloudCoverage + _CloudSoftness, density);

                // Fade at horizon
                float horizonFade = smoothstep(_CloudHeight, _CloudHeight + 0.15, elevation);

                float alpha = coverage * horizonFade;
                return float4(_CloudColor.rgb, alpha);
            }

            // ─── Horizon Glow ────────────────────────────────────────────────
            float3 HorizonGlow(float3 dir)
            {
                float y = abs(dir.y);
                float glow = smoothstep(_HorizonFogWidth, 0.0, y) * _HorizonFogStrength;
                return _HorizonFogColor.rgb * glow;
            }

            // ─── Vertex ──────────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldDir   = normalize(mul((float3x3)unity_ObjectToWorld, IN.positionOS.xyz));
                return OUT;
            }

            // ─── Fragment ────────────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.worldDir);

                // Sky base
                float3 sky = SkyGradient(dir);

                // Horizon glow (atmospheric scattering approximation)
                sky += HorizonGlow(dir);

                // Stars (only above horizon)
                if (dir.y > 0.0)
                    sky += Stars(dir, _StarBrightness);

                // Moon
                sky += MoonDisc(dir);

                // Sun
                sky += SunDisc(dir);

                // Clouds (alpha-blend over sky)
                float4 clouds = Clouds(dir);
                sky = lerp(sky, clouds.rgb, clouds.a);

                return half4(sky, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
