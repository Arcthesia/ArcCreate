// Stolen code: https://forum.unity.com/threads/how-do-i-get-my-additive-particle-effects-to-look-consistent-between-light-and-dark-backgrounds.819141/
Shader "Gameplay/HoldJudge"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
        _MainTex ("Particle Texture", 2D) = "white" {}
	}
	Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off Lighting Off ZWrite Off
 
        SubShader
        {
            Pass
            {
                Blend Zero SrcColor
 
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
 
                #include "UnityCG.cginc"
 
                sampler2D _MainTex;
                fixed4 _TintColor;
 
                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };
 
                struct v2f {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };
 
                float4 _MainTex_ST;
 
                v2f vert (appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                    return o;
                }
 
                fixed4 frag (v2f i) : SV_Target
                {
                    half4 prev = i.color * tex2D(_MainTex, i.texcoord);
                    fixed4 col = lerp(half4(1,1,1,1), prev, prev.a);
                    return col;
                }
                ENDCG
            }
 
            Pass
            {
                Blend SrcAlpha One
       
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
 
                #include "UnityCG.cginc"
 
                sampler2D _MainTex;
                fixed4 _TintColor;
 
                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };
 
                struct v2f {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };
 
                float4 _MainTex_ST;
 
                v2f vert (appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                    return o;
                }
 
                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
                    col.a = saturate(col.a); // alpha should not have double-brightness applied to it, but we can't fix that legacy behavior without breaking everyone's effects, so instead clamp the output to get sensible HDR behavior (case 967476)
                    return col;
                }
                ENDCG
            }
        }
	}
}
