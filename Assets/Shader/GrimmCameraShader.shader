﻿Shader "Grimm/CameraShader" 
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _ShiftPercentage("Shift Percentage",Range(0,1)) = 0
        _HueShift("Shift Hue", Range(0,1)) = 0
        _SatShift("Saturation Shift", Range(0,1)) = 0
        _ValShift("Value Shift", Range(0,1)) = 0
    }
    SubShader
    {
        Pass 
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform float _bwBlend;
            uniform float _ShiftPercentage;
            uniform float _HueShift;
            uniform float _SatShift;
            uniform float _ValShift;

            const float Epsilon = 1e-10;

            const float hue60 = 60 / 360;
            const float hue300 = 300 / 360;
            const float hue240 = 240 / 360;

            float3 HUEtoRGB(in float H)
            {
                float R = abs(H * 6 - 3) - 1;
                float G = 2 - abs(H * 6 - 2);
                float B = 2 - abs(H * 6 - 4);
                return saturate(float3(R, G, B));
            }

            float3 HSVtoRGB(in float3 HSV)
            {
                float3 RGB = HUEtoRGB(HSV.x);
                return ((RGB - 1) * HSV.y + 1) * HSV.z;
            }

            float3 RGBtoHCV(in float3 RGB)
            {
                // Based on work by Sam Hocevar and Emil Persson
                float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
                float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
                float C = Q.x - min(Q.w, Q.y);
                float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
                return float3(H, C, Q.x);
            }

            float3 RGBtoHSV(in float3 RGB)
            {
                float3 HCV = RGBtoHCV(RGB);
                float S = HCV.y / (HCV.z + Epsilon);
                return float3(HCV.x, S, HCV.z);
            }

            float RedCheck(float adjustedHue)
            {
                return saturate(-abs((6 * adjustedHue) - 3) + 1);
            }

            float4 frag(v2f_img i) : COLOR 
            {
                float4 c = tex2D(_MainTex, i.uv);

                

                float3 redHue = RGBtoHSV(c.rgb);

                float redHueCheck = fmod(redHue.x + 0.5, 1.0);

                float redShiftPercentage = RedCheck(redHueCheck);

                float blueHue = fmod(redHueCheck + _HueShift, 1.0);
                //float blueHue = 1 - fmod((1 - redHue.x) + _HueShift, 1.0);

                //c.rgb = HSVtoRGB(float3(lerp(redHue.x,blueHue, _ShiftPercentage * redShiftPercentage), redHue.y + _SatShift, redHue.z + _ValShift));



                float3 blueRGB = HSVtoRGB(float3(blueHue, redHue.y + _SatShift, redHue.z + _ValShift));

                c.rgb = sqrt(lerp(pow(c.rgb,2), pow(blueRGB.rgb,2), _ShiftPercentage * redShiftPercentage));




                //c.rgb = HUEtoRGB(_HueShift);
                //float3 hsv = RGBtoHSV(c.rgb);
                //hsv.r += _HueShift;
                /*if (hsv.r > 1.0)
                {
                    hsv.r -= 2.0;
                }
                else if (hsv.r < -1.0)
                {
                    hsv.r += 2.0;
                }*/
                //c.rgb = HSVtoRGB(hsv);

                //c = float4(result.r,result.g,result.b,c.a);

                //c.rgb = lerp(c.rgb, c.gbr, _HueShift.r); //90 degrees
                //c.rgb = lerp(c.rgb, c.gbr, _HueShift.g); //180 degrees
                //c.rgb = lerp(c.rgb, c.bgr, _HueShift.b); //270 degrees

                //float lum = c.r * .3 + c.g * .59 + c.b * .11;
                //float3 bw = float3(lum, lum, lum);

                //float4 result = c;
                //result.rgb = lerp(c.rgb, bw, _bwBlend);
                return c;
            }
            ENDCG
        }
    }
}
