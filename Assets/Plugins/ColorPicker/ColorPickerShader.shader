
Shader "UI/ColorPicker"
{
    Properties
    {
		// Variables from default UI shader

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		// Color picker

		_HueCircleInner("Hue circle inner radius", Range(0, .5)) = .4
		_HueSelectorInner("Hue selector inner radius", Range(0, 1)) = .8	
		_SVSquareSize("SV Square size", Range(0, .5)) = .25
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

			static const float recip2Pi = 0.159154943;
			static const float twoPi = 6.2831853;

			float3 hsv2rgb(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

			sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

			float _HueCircleInner, _HueSelectorInner;

			float _SVSquareSize;

			float3 _HSV;

			float _AspectRatio;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

			half4 hueRing(float2 uv)
			{
				float2 coords = uv - .5;

				float r = length(coords);

				float fw = fwidth(r);

				float a = smoothstep(.5, .5 - fw, r) * smoothstep(_HueCircleInner - fw, _HueCircleInner, r);

				float angle = atan2(coords.y, coords.x) * recip2Pi;

				return half4(hsv2rgb(float3(angle, 1, 1)), a);
			}

			half4 whiteRing(float2 uv, float2 pos, float inner, float outer)
			{
				float2 coords = uv - pos;

				float r = length(coords);

				float fw = fwidth(r);

				float a = smoothstep(outer, outer - fw, r) * smoothstep(inner - fw, inner, r);

				return half4(1, 1, 1, a);
			}

			half4 svSquare(float2 uv)
			{
				float2 sv = (uv - .5) / (_SVSquareSize * 2) + .5;

				float dx = abs(ddx(sv.x));
				float dy = abs(ddy(sv.y));

				float a =
					smoothstep(0, dx, sv.x) * smoothstep(1, 1 - dx, sv.x) *
					smoothstep(0, dy, sv.y) * smoothstep(1, 1 - dy, sv.y);

				return float4(hsv2rgb(float3(_HSV.x, sv)), a);
			}

			fixed4 mix(fixed4 bot, fixed4 top)
			{
				return fixed4(lerp(bot.rgb, top.rgb, top.a), max(bot.a, top.a));
			}

            fixed4 frag(v2f IN) : SV_Target
            {
				// Aspect ratio correction

				float2 uv = _AspectRatio > 1 ?
					float2(.5 + (IN.texcoord.x - .5) * _AspectRatio, IN.texcoord.y) :
					float2(IN.texcoord.x, .5 + (IN.texcoord.y - .5) / _AspectRatio);

				// Hue ring

				half4 color = hueRing(uv);

				// Hue ring selector

				float hSelectorR = (.5 - _HueCircleInner) * .5;

				half4 hSelector = whiteRing(
					uv,
					float2(cos(_HSV.x * twoPi), sin(_HSV.x * twoPi)) * (.5 - hSelectorR) + .5,
					hSelectorR * _HueSelectorInner, hSelectorR);

				color = mix(color, hSelector);

				// Saturation value Square

				half4 sv = svSquare(uv);

				color = sv.a > 0 ? sv : color;

				// Saturation value selector

				half4 svSelector = whiteRing(
					uv,
					.5 + 2 * _SVSquareSize * (_HSV.yz - .5),
					hSelectorR * _HueSelectorInner, hSelectorR);

				color = mix(color, svSelector);

				// Unity stuff

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

				#ifdef UNITY_COLORSPACE_GAMMA
                return color;
				#endif

				return fixed4(GammaToLinearSpace(color.rgb), color.a);
            }
        ENDCG
        }
    }
}
