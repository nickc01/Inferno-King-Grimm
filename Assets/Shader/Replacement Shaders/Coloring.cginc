#ifndef COLORING_INCLUDED
#define COLORING_INCLUDED
#endif

float3 HUEtoRGB(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return saturate(float3(R, G, B));
}

float3 HSVtoRGB(in float H, in float S, in float V)
{
	float3 RGB = HUEtoRGB(H);
	return ((RGB - 1) * S + 1) * V;
}

float3 RGBtoHCV(in float3 RGB)
{
	// Based on work by Sam Hocevar and Emil Persson
	float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
	float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + 1e-10) + Q.z);
	return float3(H, C, Q.x);
}

float3 RGBtoHSV(in float3 RGB)
{
	float3 HCV = RGBtoHCV(RGB);
	float S = HCV.y / (HCV.z + 1e-10);
	return float3(HCV.x, S, HCV.z);
}

float RedCheck(in float adjustedHue)
{
	return saturate(-abs(2.6 * ((2 * adjustedHue) - 1)) + 1);
}

float4 Colorize4(in float4 c) {
	float3 redHue = RGBtoHSV(c.rgb);

	float redHueCheck = fmod(redHue.x + 0.5, 1.0);

	float redShiftPercentage = RedCheck(redHueCheck);

	float blueHue = fmod(redHueCheck + _HueShift, 1.0);

	c.rgb = lerp(c.rgb, HSVtoRGB(blueHue, redHue.y, redHue.z), _ShiftPercentage * redShiftPercentage);
	
	return c;
}

float3 Colorize3(in float3 c) {
	float3 redHue = RGBtoHSV(c.rgb);

	float redHueCheck = fmod(redHue.x + 0.5, 1.0);

	float redShiftPercentage = RedCheck(redHueCheck);

	float blueHue = fmod(redHueCheck + _HueShift, 1.0);

	return lerp(c.rgb, HSVtoRGB(blueHue, redHue.y, redHue.z), _ShiftPercentage * redShiftPercentage);
}