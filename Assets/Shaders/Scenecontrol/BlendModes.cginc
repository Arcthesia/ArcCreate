#ifndef BLEND_MODES_INCLUDED
#define BLEND_MODES_INCLUDED

inline fixed blendOverlay(fixed base, fixed blend)
{
	return base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
}

inline fixed blendColorBurn(fixed base, fixed blend)
{
	return (blend == 0.0) ? blend : max((1.0 - ((1.0 - base) / blend)), 0.0);
}

inline fixed blendColorDodge(fixed base, fixed blend)
{
	return (blend == 1.0) ? blend : min(base / (1.0 - blend), 1.0);
}

inline fixed blendLinearBurn(fixed base, fixed blend)
{
	return max(base + blend - 1.0, 0.0);
}

inline fixed blendLinearDodge(fixed base, fixed blend)
{
	return min(base + blend, 1.0);
}


fixed3 blendOverlay(fixed3 base, fixed3 blend)
{
	return fixed3(
		blendOverlay(base.r, blend.r),
		blendOverlay(base.g, blend.g),
		blendOverlay(base.b, blend.b)
	);
}

fixed3 blendHardLight(fixed3 base, fixed3 blend)
{
	return blendOverlay(blend, base);
}

fixed3 blendColorBurn(fixed3 base, fixed3 blend)
{
	return fixed3(
		blendColorBurn(base.r, blend.r),
		blendColorBurn(base.g, blend.g),
		blendColorBurn(base.b, blend.b)
	);
}

fixed3 blendColorDodge(fixed3 base, fixed3 blend)
{
	return fixed3(
		blendColorDodge(base.r, blend.r),
		blendColorDodge(base.g, blend.g),
		blendColorDodge(base.b, blend.b)
	);
}

fixed3 blendLinearBurn(fixed3 base, fixed3 blend)
{
	return fixed3(
		blendLinearBurn(base.r, blend.r),
		blendLinearBurn(base.g, blend.g),
		blendLinearBurn(base.b, blend.b)
	);
}

fixed3 blendLinearDodge(fixed3 base, fixed3 blend)
{
	return fixed3(
		blendLinearDodge(base.r, blend.r),
		blendLinearDodge(base.g, blend.g),
		blendLinearDodge(base.b, blend.b)
	);
}


fixed3 blendOverlay(fixed3 base, fixed3 blend, fixed opacity)
{
	return (blendOverlay(base, blend) * opacity + base * (1.0 - opacity));
}

fixed3 blendHardLight(fixed3 base, fixed3 blend, fixed opacity)
{
	return (blendOverlay(blend, base) * opacity + base * (1.0 - opacity));
}

fixed3 blendColorBurn(fixed3 base, fixed3 blend, fixed opacity)
{
	return (blendColorBurn(base, blend) * opacity + base * (1.0 - opacity));
}

fixed3 blendColorDodge(fixed3 base, fixed3 blend, fixed opacity)
{
	return (blendColorDodge(base, blend) * opacity + base * (1.0 - opacity));
}

fixed3 blendLinearBurn(fixed3 base, fixed3 blend, fixed opacity)
{
	return (blendLinearBurn(base, blend) * opacity + base * (1.0 - opacity));
}

fixed3 blendLinearDodge(fixed3 base, fixed3 blend, fixed opacity)
{
	return (blendLinearDodge(base, blend) * opacity + base * (1.0 - opacity));
}


#endif // UNITY_CG_INCLUDED