Shader "UI/BlendModes/Screen Colored" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		_ShiftPercentage("Shift Percentage",Range(0,1)) = 0
		_HueShift("Shift Hue", Range(0,1)) = 0
	}
	SubShader {
		LOD 950
		Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		GrabPass {
		}
		Pass {
			LOD 950
			Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ColorMask 0 -1
			ZWrite Off
			Cull Off
			Stencil {
				ReadMask 0
				WriteMask 0
				Comp Always
				Pass Keep
				Fail Keep
				ZFail Keep
			}
			Fog {
				Mode Off
			}
			GpuProgramID 50896
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Color;
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _GrabTexture;
			uniform float _ShiftPercentage;
			uniform float _HueShift;
			
			#include "Coloring.cginc"
			
			
			/*inline float Lerp(in float2 range, in float value)
			{
				return range.x + ((range.y - range.x) * value);
			}
			
			inline float3 Colorize(in float3 c) {
				float average = (c.r + c.g + c.b);

				float redness = ((c.r * 3) - average) * _Sat * _ShiftPercentage;
				//float redness = (c.r - 0.5) * c.g * c.b;

				float2 redblue = c.rb;

				c.r = Lerp(redblue,redness);
				c.b = Lerp(redblue.yx, redness);

				c.rgb += redness * _Brightness * _ShiftPercentage;
				
				return c;
			}*/
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.position = tmp0;
                o.texcoord1 = tmp0;
                o.color = v.color * _Color;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp1.x = tmp0.w * inp.color.w + -0.01;
                tmp1.x = tmp1.x < 0.0;
                if (tmp1.x) {
                    discard;
                }
                tmp1.xy = inp.texcoord1.xy / inp.texcoord1.ww;
                tmp1.xy = tmp1.xy + float2(1.0, 1.0);
                tmp1.x = tmp1.x * 0.5;
                tmp1.z = -tmp1.y * 0.5 + 1.0;
                tmp1 = tex2D(_GrabTexture, tmp1.xz);
                tmp1.xyz = float3(1.0, 1.0, 1.0) - tmp1.xyz;
                tmp0.xyz = -Colorize3(tmp0.xyz * inp.color.xyz) + float3(1.0, 1.0, 1.0);
                tmp0.w = tmp0.w * inp.color.w;
                o.sv_target.w = tmp0.w;
                o.sv_target.xyz = -tmp1.xyz * tmp0.xyz + float3(1.0, 1.0, 1.0);
                return o;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Approximate Screen"
}