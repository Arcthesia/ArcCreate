// Based on Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// 0thElement: Do not question this shader. i don't even know why it works myself
Shader "UI/PatternOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _PatternOffset ("Pattern Offset", Vector) = (0, 0, 0, 0)
        _HSVOffset ("HSV Offset", Vector) = (0, 0, 0, 0)
        _Scale ("Scale", Float) = 1

        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            #include "../ColorSpace.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
				float4 screenPos  : TEXCOORD2;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _OverlayTex;
			float4 _OverlayTex_TexelSize;

            fixed4 _Color;
            float _Scale;
            fixed4 _PatternOffset;
            fixed4 _HSVOffset;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.vertex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            half4 grabPattern(float4 screenPos, float layer)
            {
                layer = layer + 1;
                screenPos.y = -screenPos.y - layer * 0.2;
				float2 screenCoord = (screenPos.xy / screenPos.w + _PatternOffset * layer * _Time.x / 100) / _Scale * layer;
				screenCoord.y = frac(screenCoord.y * 720 / _OverlayTex_TexelSize.w / 2);
				screenCoord.x = frac(screenCoord.x * 720 * _ScreenParams.x / _ScreenParams.y / _OverlayTex_TexelSize.z / 2);
				return tex2D(_OverlayTex, screenCoord);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				half4 pattern0 = grabPattern(IN.screenPos, 0);
				half4 pattern1 = grabPattern(IN.screenPos, 0.7);
                half pattern = clamp(pattern0 + pattern1, 0, 1);

                fixed3 hsv = rgb2hsv(color.rgb);
                hsv += _HSVOffset.rgb;
                half4 shifted = half4(hsv2rgb(hsv), color.a);

                color = lerp(color, shifted, pattern);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}