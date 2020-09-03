﻿// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Particles/Additive (Soft) (Grimm Version)" {
    Properties{
        _MainTex("Particle Texture", 2D) = "white" {}
        _InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
        _HueShift("Shift Hue", Color) = (0,0,0,0)
    }

        Category{
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
            Blend One OneMinusSrcColor
            ColorMask RGB
            Cull Off Lighting Off ZWrite Off

            SubShader {
                Pass {

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma multi_compile_particles
                    #pragma multi_compile_fog

                    #include "UnityCG.cginc"

                    sampler2D _MainTex;
                    fixed4 _TintColor;

                    struct appdata_t {
                        float4 vertex : POSITION;
                        fixed4 color : COLOR;
                        float2 texcoord : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct v2f {
                        float4 vertex : SV_POSITION;
                        fixed4 color : COLOR;
                        float2 texcoord : TEXCOORD0;
                        UNITY_FOG_COORDS(1)
                        #ifdef SOFTPARTICLES_ON
                        float4 projPos : TEXCOORD2;
                        #endif
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    float4 _MainTex_ST;

                    v2f vert(appdata_t v)
                    {
                        v2f o;
                        UNITY_SETUP_INSTANCE_ID(v);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        #ifdef SOFTPARTICLES_ON
                        o.projPos = ComputeScreenPos(o.vertex);
                        COMPUTE_EYEDEPTH(o.projPos.z);
                        #endif
                        o.color = v.color;
                        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                        UNITY_TRANSFER_FOG(o,o.vertex);
                        return o;
                    }

                    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
                    float _InvFade;
                    fixed4 _HueShift;


                    fixed4 frag(v2f i) : SV_Target
                    {
                        #ifdef SOFTPARTICLES_ON
                        float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                        float partZ = i.projPos.z;
                        float fade = saturate(_InvFade * (sceneZ - partZ));
                        i.color.a *= fade;
                        #endif

                        fixed4 col = i.color * tex2D(_MainTex, i.texcoord);
                        col.rgb = lerp(col.rgb, col.gbr, _HueShift.r); //90 degrees
                        col.rgb = lerp(col.rgb, col.gbr, _HueShift.g); //180 degrees
                        col.rgb = lerp(col.rgb, col.bgr, _HueShift.b); //270 degrees
                        col.rgb *= col.a;
                        UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
                        return col;
                    }
                    ENDCG
                }
            }
        }
}
