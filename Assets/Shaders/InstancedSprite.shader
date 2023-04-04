Shader "Gameplay/InstancedSprite"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
		_MainTex ("Sprite Texture", 2D) = "white" {}
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

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color    : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD0;
				fixed4 color    : COLOR;
				uint instanceID : BLENDINDICES0;
			};
			
			struct Properties
			{
				float4 color;
			};
			
			StructuredBuffer<Properties> _Properties;

			v2f vert (appdata_t IN, uint instanceID : SV_InstanceID)
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				OUT.instanceID = instanceID;
				return OUT;
			}

			sampler2D _MainTex;
			half4 _Color;

			half4 frag(v2f IN) : SV_Target
			{
				half4 c = tex2D(_MainTex, IN.texcoord) * _Properties[IN.instanceID].color * IN.color * _Color;
				return c;
			}
		ENDCG
		}
	}
}
