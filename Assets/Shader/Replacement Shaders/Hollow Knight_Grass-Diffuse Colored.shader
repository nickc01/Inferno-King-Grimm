Shader "Hollow Knight/Grass-Diffuse" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Vector) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		_SwaySpeed ("SwaySpeed", Float) = 1
		_SwayAmount ("Sway Amount", Float) = 1
		_WorldOffset ("World Offset", Float) = 1
		_HeightOffset ("Height Offset", Float) = 0
		_ClampZ ("Clamp Z Position", Float) = 1
		[PerRendererData] _PushAmount ("Push Amount (Player)", Float) = 0
		_ShiftPercentage("Shift Percentage",Range(0,1)) = 0
		_HueShift("Shift Hue", Range(0,1)) = 0
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Tags { "CanUseSpriteAtlas" = "true" "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend One OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			GpuProgramID 55436
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Color;
			float _SwaySpeed;
			float _SwayAmount;
			float _WorldOffset;
			float _HeightOffset;
			float _ClampZ;
			float _PushAmount;
			uniform float _ShiftPercentage;
			uniform float _HueShift;
			
			#include "Coloring.cginc"
			
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			CBUFFER_START(UnityPerDrawSprite)
				float4 _RendererColor;
				float2 _Flip;
			CBUFFER_END
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = unity_ObjectToWorld._m23 + unity_ObjectToWorld._m03;
                tmp0.x = tmp0.x * _WorldOffset;
                tmp0.x = _Time.y * _SwaySpeed + tmp0.x;
                tmp0.x = sin(tmp0.x);
                tmp0.x = tmp0.x * _SwayAmount;
                tmp0.x = tmp0.x * 1.25 + _PushAmount;
                tmp0.y = unity_ObjectToWorld._m10 + unity_ObjectToWorld._m11;
                tmp0.y = tmp0.y + unity_ObjectToWorld._m12;
                tmp0.y = _HeightOffset * tmp0.y + v.vertex.y;
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.y = max(-_ClampZ, unity_ObjectToWorld._m23);
                tmp0.y = min(tmp0.y, _ClampZ);
                tmp0.y = abs(tmp0.y) + 1.0;
                tmp0.x = tmp0.x * tmp0.y + v.vertex.x;
                tmp0.y = v.vertex.y;
                tmp0.xy = tmp0.xy * _Flip;
                tmp1 = tmp0.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0 = v.color * _Color;
                o.color = tmp0 * _RendererColor;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xyz = inp.color.xyz * glstate_lightmodel_ambient.xyz;
                tmp0.xyz = tmp0.xyz + tmp0.xyz;
                tmp1 = Colorize4(tex2D(_MainTex, inp.texcoord.xy));
                tmp0.w = inp.color.w;
                tmp0 = tmp0 * tmp1;
                o.sv_target.xyz = tmp0.www * tmp0.xyz;
                o.sv_target.w = tmp0.w;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Diffuse"
}