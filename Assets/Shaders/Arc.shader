Shader "Gameplay/Arc"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_LowColor ("LowColor", Color) = (1,1,1,1)
		_RedColor ("RedColor", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true" "IgnoreProjector"="true"}

		Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; 
				float2 uv : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
				uint instanceID : BLENDINDICES0;
			};

			struct Properties
			{
				float4 color;
				float redValue;
				int selected;
			};
			
			int _Selected;
			float4 _RedColor;
			float4 _Color, _LowColor;
            float4 _MainTex_ST;
			sampler2D _MainTex;
			
			StructuredBuffer<Properties> _Properties;

			v2f vert (appdata v, uint instanceID : SV_InstanceID)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.instanceID = instanceID;
				return o;
			}
			
			half4 Highlight(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				if(c.r<0.5) hsv.r += 0.1f;
				else hsv.g += 1.2f;
				return half4(hsv2rgb(hsv),c.a);
			}

			half4 frag (v2f i) : SV_Target
			{
				Properties properties = _Properties[i.instanceID];
			    if(i.worldpos.z > 50 || i.worldpos.z < -100) discard;
				float4 c = tex2D(_MainTex, i.uv); 
				
				float4 inColor = lerp(_LowColor, _Color, clamp((i.worldpos.y - 1) / 4.5f, 0, 1));
				float4 color = lerp(inColor, _RedColor, properties.redValue);

				if(properties.selected == 1) 
				{
					color = Highlight(color);
				}

				c *= color * properties.color;  
				c.a *= 0.9;
				return c;
			}
			ENDCG
		}
	}
}
